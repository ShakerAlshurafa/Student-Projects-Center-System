using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Controllers.project
{
    [Authorize(Roles ="admin")]
    [Route("api/admin/projects")]
    [ApiController]
    public class ProjectsAdminController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<LocalUser> userManager;
        private readonly IChatService chatService;

        public ProjectsAdminController(
            IUnitOfWork unitOfWork,
            UserManager<LocalUser> userManager,
            IChatService chatService)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.chatService = chatService;
        }


        [HttpGet("{PageSize}/{PageNumber}")]
        //[ResponseCache(CacheProfileName = "defaultCache")]
        public async Task<ActionResult<ApiResponse>> GetAll(
            [FromQuery] string? projectName = null, 
            int PageSize = 6, 
            int PageNumber = 1)
        {
            Expression<Func<StudentProjectsCenterSystem.Core.Entities.project.Project, bool>> filter = x => true;
            if (!string.IsNullOrEmpty(projectName))
            {
                filter = x => x.Name.Contains(projectName);
            }
            var model = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User"); ;

            if (!model.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            int project_count = await unitOfWork.projectRepository.Count();

            var projectDTOs = model.Select(project =>
            {
                var supervisor = project?.UserProjects?
                    .FirstOrDefault(up => up.Role == "supervisor" && !up.IsDeleted);
                var co_supervisor = project?.UserProjects?
                    .FirstOrDefault(up => up.Role == "co-supervisor" && !up.IsDeleted);
                var customer = project?.UserProjects?
                    .FirstOrDefault(up => up.Role == "customer" && !up.IsDeleted);


                return new ProjectDTO
                {
                    Id = project!.Id, //null-forgiving operator
                    Name = project.Name,
                    Status = project.Status ?? "",
                    SupervisorName = supervisor?.User?.FirstName + " " + supervisor?.User?.LastName
                                    ?? "No Supervisor Assigned",
                    CoSupervisorName = co_supervisor?.User?.FirstName + " " + co_supervisor?.User?.LastName
                                    ?? "No Co-Supervisor Assigned",
                    CustomerName = customer?.User?.FirstName + " " + customer?.User?.LastName
                                    ?? "No Customer Assigned",
                    Company = customer?.User?.CompanyName
                                    ?? "No Customer Assigned",
                    WorkgroupName = project?.Workgroup?.Name ?? "No Workgroup",
                    Team = project?.UserProjects?.Where(up => up.Role == "student" && !up.IsDeleted)?
                                    .Select(up => up?.User?.FirstName + " " + up?.User?.LastName)
                                    .ToList()
                                    ?? new List<string>(),
                    Favorite = project?.Favorite ?? false
                };
            }).ToList();

            return new ApiResponse(200, "Projects retrieved successfully", new { 
                Total = project_count, Projects = projectDTOs 
            });
        }


        [HttpGet("{projectId}/archive/users")]
        public async Task<ActionResult<ApiResponse>> GetDeletedUsers(int projectId)
        {
            var users = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            var deletedUsers = users.UserProjects
                .Where(u => u.IsDeleted)
                .Select(u => new
                {
                    u.UserId,
                    u.Role,
                    u.JoinAt,
                    u.DeletededAt,
                    u.DeletedNotes
                });


            if (!deletedUsers.Any())
            {
                return Ok(new ApiResponse(200, "No deleted users found for this project."));
            }

            return Ok(new ApiResponse(200, "Deleted users retrieved successfully.", deletedUsers));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([FromBody, Required] ProjectCreateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return BadRequest(new ApiValidationResponse(errors));
            }

            // Validate the supervisor and customer exist in the system
            var supervisor = await userManager.FindByIdAsync(project.SupervisorId);
            var customer = await userManager.FindByIdAsync(project.CustomerId);

            if (supervisor == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Supervisor not found." }));
            }

            if (customer == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Customer not found." }));
            }

            var workgroup = new Workgroup
            {
                Name = project.Name + " Workgroup"
            };

            // Create the project object
            var model = new Project
            {
                Name = project.Name,
                StartDate = DateTime.Now,
                Workgroup = workgroup,
                UserProjects = new List<UserProject>
                {
                    new UserProject { UserId = supervisor.Id, Role = "supervisor" },
                    new UserProject { UserId = customer.Id, Role = "customer" }
                }
            };

            await unitOfWork.projectRepository.Create(model);
            int successSave = await unitOfWork.save();

            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = model.Id }, new ApiResponse(201, result: project));
        }


        [HttpPut("{projectId}/change-supervisor")]
        public async Task<ActionResult<ApiResponse>> ChangeSupervisor(
            [Required] int projectId,
            [Required] string newSupervisorId,
            [FromBody] NotesDTO notes)
        {
            // Find the user by ID
            var newSupervisor = await userManager.FindByIdAsync(newSupervisorId);
            if (newSupervisor == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"Supervisor with ID {newSupervisorId} not found." }));
            }

            // Get roles of the user
            var roles = await userManager.GetRolesAsync(newSupervisor);

            // Check if the user has the "supervisor" role
            if (!roles.Contains("supervisor"))
            {
                return BadRequest(new ApiResponse(400, "The user is not a supervisor."));
            }

            // Retrieve the project by ID
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Check if the new supervisor is already assigned to the project
            if (existingProject.UserProjects.Select(x => x.UserId).Contains(newSupervisorId))
            {
                return BadRequest(new ApiResponse(400, "The user is already in the project."));
            }

            // Find the old supervisor entry
            var supervisorEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.Role == "supervisor");

            if (supervisorEntry == null)
            {
                return NotFound(new ApiResponse(404, "Old supervisor not found."));
            }

            supervisorEntry.IsDeleted = true;
            supervisorEntry.DeletedNotes = notes.Notes;
            supervisorEntry.DeletededAt = DateTime.UtcNow;

            //// Remove the old supervisor and assign the new one
            //existingProject.UserProjects.Remove(supervisorEntry);

            existingProject.UserProjects.Add(new UserProject { UserId = newSupervisorId, Role = "supervisor" });

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to change the supervisor."));
            }

            return Ok(new ApiResponse(200, "Supervisor changed successfully."));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(
            int id, 
            [FromBody, Required] UpdateProjectDTO updateProjectDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            // Find the project by id
            var existingProject = await unitOfWork.projectRepository.GetById(id);
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            if (!updateProjectDTO.Name.IsNullOrEmpty())
            {
                existingProject.Name = updateProjectDTO.Name;
                if (existingProject.Workgroup != null)
                    existingProject.Workgroup.Name = updateProjectDTO.Name + " " + "Workgroup";
            }

            if (!updateProjectDTO.SupervisorId.IsNullOrEmpty())
            {
                var newSupervisor = await userManager.FindByIdAsync(updateProjectDTO.SupervisorId);
                if (newSupervisor == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"Supervisor with ID {updateProjectDTO.SupervisorId} not found." }));
                }

                // Get roles of the user
                var roles = await userManager.GetRolesAsync(newSupervisor);

                // Check if the user has the "supervisor" role
                if (!roles.Contains("supervisor"))
                {
                    return BadRequest(new ApiResponse(400, "The user is not a supervisor."));
                }

                var supervisorEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.Role.ToLower() == "supervisor");

                if (supervisorEntry == null)
                {
                    return NotFound(new ApiResponse(404, "Old supervisor not found."));
                }

                supervisorEntry.IsDeleted = true;
                supervisorEntry.DeletedNotes = updateProjectDTO.ChangeOldSupervisorNotes;
                supervisorEntry.DeletededAt = DateTime.UtcNow;

                existingProject.UserProjects.Add(new UserProject { UserId = updateProjectDTO.SupervisorId, Role = "supervisor" });
            }

            if (!updateProjectDTO.CustomerId.IsNullOrEmpty())
            {
                var newCustomer = await userManager.FindByIdAsync(updateProjectDTO.CustomerId);
                if (newCustomer == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"Customer with ID {updateProjectDTO.CustomerId} not found." }));
                }

                // Get roles of the user
                var roles = await userManager.GetRolesAsync(newCustomer);

                var customerrEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.Role == "customer");

                if (customerrEntry == null)
                {
                    return NotFound(new ApiResponse(404, "Old customer not found."));
                }

                customerrEntry.IsDeleted = true;
                customerrEntry.DeletedNotes = updateProjectDTO.ChangeOldCustomerNotes;
                customerrEntry.DeletededAt = DateTime.UtcNow;

                existingProject.UserProjects.Add(new UserProject { UserId = updateProjectDTO.CustomerId, Role = "customer" });
            }

            if(updateProjectDTO.Status != null)
            {
                // Validate the new status
                var validStatuses = new List<string> { "active", "completed", "pending", "canceled" };
                if (string.IsNullOrWhiteSpace(updateProjectDTO.Status) || !validStatuses.Contains(updateProjectDTO.Status.ToLower()))
                {
                    return BadRequest(new ApiResponse(400, $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}"));
                }

                // Update the project status
                existingProject.Status = updateProjectDTO.Status;
                existingProject.ChangeStatusNotes = updateProjectDTO.ChangeStatusNotes;
                existingProject.ChangeStatusAt = DateTime.UtcNow;

                if (existingProject.Status == "completed")
                {
                    existingProject.EndDate = DateTime.UtcNow;
                }
            }

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);
            int successSave = await unitOfWork.save();

            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Project updated successfully"));
        }


        [HttpPut("favorites/{id}/toggle")]
        public async Task<ActionResult<ApiResponse>> UpdateToggleFavorite(int id)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            // Find the project by id
            var existingProject = await unitOfWork.projectRepository.GetById(id);
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            existingProject.Favorite = !existingProject.Favorite;

            unitOfWork.projectRepository.Update(existingProject);
            int successSave = await unitOfWork.save();

            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Project updated successfully"));
        }


    }
}
