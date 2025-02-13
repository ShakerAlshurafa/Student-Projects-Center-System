using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Roles = "admin")]
    [Route("api/admin/projects")]
    [ApiController]
    public class ProjectsAdminController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<LocalUser> userManager;
        private readonly IChatService chatService;
        private readonly IEmailService emailService;

        public ProjectsAdminController(
            IUnitOfWork unitOfWork,
            UserManager<LocalUser> userManager,
            IChatService chatService,
            IEmailService emailService)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.chatService = chatService;
            this.emailService = emailService;
        }


        [HttpGet]
        //[ResponseCache(CacheProfileName = "defaultCache")]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? projectName = null)
        {
            Expression<Func<StudentProjectsCenterSystem.Core.Entities.project.Project, bool>> filter = x => true;
            if (!string.IsNullOrEmpty(projectName))
            {
                filter = x => x.Name.Contains(projectName);
            }
            var model = await unitOfWork.projectRepository.GetAll(filter, "Workgroup,UserProjects.User"); ;

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
                    Company = project?.CompanyName
                                    ?? "No Company Name Assigned",
                    WorkgroupName = project?.Workgroup?.Name ?? "No Workgroup",
                    Team = project?.UserProjects?.Where(up => up.Role == "student" && !up.IsDeleted)?
                                    .Select(up => up?.User?.FirstName + " " + up?.User?.LastName)
                                    .ToList()
                                    ?? new List<string>(),
                    Favorite = project?.Favorite ?? false
                };
            }).ToList();

            return new ApiResponse(200, "Projects retrieved successfully", new
            {
                Total = project_count,
                Projects = projectDTOs
            });
        }


        [HttpGet("{projectId}/archive/users")]
        public async Task<ActionResult<ApiResponse>> GetDeletedUsers(int projectId)
        {
            var users = await unitOfWork.projectRepository.GetById(projectId, "UserProjects.User");
            var deletedUsers = users.UserProjects
                .Where(u => u.IsDeleted)
                .Select(u => new
                {
                    Id = u.UserId,
                    Name = u.User.UserName ?? "",
                    role = u.Role,
                    JoinAt = u.JoinAt,
                    DeletededAt = u.DeletededAt,
                    DeletedNotes = u.DeletedNotes
                }).ToList();


            if (!deletedUsers.Any())
            {
                return Ok(new ApiResponse(200, "No deleted users found for this project."));
            }

            return Ok(new ApiResponse(200, "Deleted users retrieved successfully.", deletedUsers));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(
            [FromBody] ProjectCreateDTO project)
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
                CompanyName = project.CompanyName,
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

            // Define email templates
            string supervisorEmailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #0275d8; text-align: center;'>You Have Been Added as a Supervisor</h2>
                    <p style='font-size: 16px; color: #555;'>Hello <strong>{supervisor.FirstName}</strong>,</p>
                    <p style='font-size: 16px; color: #555;'>You have been added as a <strong>Supervisor</strong> for the project <strong>'{project.Name}'</strong>.</p>
                    <p style='text-align: center; margin-top: 20px;'>
                        <a href='http://localhost:5173/workgroups/{workgroup.Id}' 
                            style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                            color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                            View Workgroup
                        </a>
                    </p>
                    <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                        If you have any questions, please contact support.
                    </p>
                </div>";

            // Send email to supervisor
            await emailService.SendEmailAsync(supervisor.Email ?? "", "Added as Supervisor", supervisorEmailContent, true);

            // Define customer email template
            string customerEmailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #0275d8; text-align: center;'>You Have Been Added as a Customer</h2>
                    <p style='font-size: 16px; color: #555;'>Hello <strong>{customer.FirstName}</strong>,</p>
                    <p style='font-size: 16px; color: #555;'>You have been added as a <strong>Customer</strong> for the project <strong>'{project.Name}'</strong>.</p>
                    <p style='text-align: center; margin-top: 20px;'>
                        <a href='http://localhost:5173/workgroups/{workgroup.Id}' 
                            style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                            color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                            View Workgroup
                        </a>
                    </p>
                    <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                        If you have any questions, please contact support.
                    </p>
                </div>";

            // Send email to customer
            await emailService.SendEmailAsync(customer.Email ?? "", "Added as Customer", customerEmailContent, true);



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
            [FromBody] UpdateProjectDTO updateProjectDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            // Find the project by id
            var existingProject = await unitOfWork.projectRepository.GetById(id, "Workgroup,UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            existingProject.Name = updateProjectDTO.Name;
            if (existingProject.Workgroup != null)
                existingProject.Workgroup.Name = updateProjectDTO.Name + " " + "Workgroup";

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

            var users = existingProject.UserProjects;

            var oldSupervisorEntry = users
                            .FirstOrDefault(up => up.Role.ToLower() == "supervisor" && !up.IsDeleted);

            var deletedSupervisor = users
                            .Where(u => u.UserId == updateProjectDTO.SupervisorId)
                            .FirstOrDefault();
            if (deletedSupervisor != null)
            {
                users.Remove(deletedSupervisor);
                deletedSupervisor.IsDeleted = false;
                users.Add(deletedSupervisor);
            }
            else if (oldSupervisorEntry == null || oldSupervisorEntry.UserId != updateProjectDTO.SupervisorId)
            {
                if (oldSupervisorEntry != null)
                {
                    users.Remove(oldSupervisorEntry);
                    oldSupervisorEntry.IsDeleted = true;
                    oldSupervisorEntry.DeletedNotes = updateProjectDTO.ChangeOldSupervisorNotes;
                    oldSupervisorEntry.DeletededAt = DateTime.UtcNow;
                    users.Add(oldSupervisorEntry);

                }

                var newSupervisorEntry = new UserProject { UserId = updateProjectDTO.SupervisorId, Role = "supervisor" };
                if (newSupervisorEntry == null)
                {
                    return BadRequest(new ApiResponse(400, "Update Faild"));
                }
                users.Add(newSupervisorEntry);
            }



            var newCustomer = await userManager.FindByIdAsync(updateProjectDTO.CustomerId);
            if (newCustomer == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"Customer with ID {updateProjectDTO.CustomerId} not found." }));
            }

            var oldCustomerEntry = users
                            .FirstOrDefault(up => up.Role.ToLower() == "customer" && !up.IsDeleted);

            var deletedCustomer = users
                            .Where(u => u.UserId == updateProjectDTO.CustomerId)
                            .FirstOrDefault();

            if (deletedCustomer != null)
            {
                users.Remove(deletedCustomer);
                deletedCustomer.IsDeleted = false;
                users.Add(deletedCustomer);
            }
            else if (oldCustomerEntry == null || oldCustomerEntry.UserId != updateProjectDTO.CustomerId)
            {
                if (oldCustomerEntry != null)
                {
                    users.Remove(oldCustomerEntry);
                    oldCustomerEntry.IsDeleted = true;
                    oldCustomerEntry.DeletedNotes = updateProjectDTO.ChangeOldCustomerNotes;
                    oldCustomerEntry.DeletededAt = DateTime.UtcNow;
                    users.Add(oldCustomerEntry);
                }

                var newCustomerEntry = new UserProject { UserId = updateProjectDTO.CustomerId, Role = "customer" };
                if (newCustomerEntry == null)
                {
                    return BadRequest(new ApiResponse(400, "Update Faild"));
                }
                users.Add(newCustomerEntry);
            }

            // Validate the new status
            var validStatuses = new List<string> { "active", "completed", "pending", "canceled" };
            if (string.IsNullOrWhiteSpace(updateProjectDTO.Status) || !validStatuses.Contains(updateProjectDTO.Status.ToLower()))
            {
                return BadRequest(new ApiResponse(400, $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}"));
            }

            // Update the project status
            existingProject.Status = updateProjectDTO.Status.ToLower();
            existingProject.ChangeStatusNotes = updateProjectDTO.ChangeStatusNotes;
            existingProject.ChangeStatusAt = DateTime.UtcNow;
            existingProject.UserProjects = users;

            if (existingProject.Status == "completed")
            {
                existingProject.EndDate = DateTime.UtcNow;
            }

            existingProject.CompanyName = updateProjectDTO.CompanyName;

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
