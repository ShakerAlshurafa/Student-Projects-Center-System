using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Security.Claims;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public ProjectsController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        /* GetAll
         * 
        [HttpGet]
        [ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? projectName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
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
                    Id = project.Id,
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

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }
        */

        /* GetAllForUser
         * 
         * 
        [HttpGet("get-all-for-user")]
        [Authorize]
        //[ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAllForUser([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            // Retrieve the logged-in user's ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(401, "User not Find."));
            }

            // Filter workgroups where the logged-in user is associated
            Expression<Func<StudentProjectsCenterSystem.Core.Entities.project.Project, bool>> filter = x =>
                x.UserProjects.Any(up => up.UserId == userId);

            var projects = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User");
            if (!projects.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projectDTOs = mapper.Map<List<MyProjectDTO>>(projects);

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }
        */


        
        [HttpGet("details/{id}")]
        public async Task<ActionResult<ApiResponse>> GetById(int id)
        {
            var model = await unitOfWork.projectRepository.GetByIdWithDetails(id);
            if (model == null)
            {
                return NotFound(new ApiResponse(404, "No Projects Found!"));
            }

            return Ok(new ApiResponse(200, result: model));
        }


        /* GetDeletedUsers
         * 
         * 
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
        */


        [HttpGet("project-statuses")]
        public ActionResult<ApiResponse> GetAllProjectStatuses()
        {
            return Ok(new ApiResponse(200, result: new
            {
                active = "The project is currently ongoing, with tasks and activities actively being worked on.",
                completed = "The project has been successfully finished, with all objectives and deliverables met.",
                pending = "The project is in a state of preparation, awaiting the necessary decisions or requirements to begin or reach completion.",
                canceled = "The project has been terminated before completion due to specific circumstances or changes in requirements."
            }));
        }

        /* Create
         * 
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] ProjectCreateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
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
            var model = new StudentProjectsCenterSystem.Core.Entities.project.Project
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
        */


        /* AddStudent
         * 
        [HttpPost("students")]
        public async Task<ActionResult<ApiResponse>> AddStudent([Required] int projectId, [Required] CreateStudentDTO students)
        {
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate that all students exist in the system
            foreach (var userId in students.usersId)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {userId} not found." }));
                }
            }

            foreach (var userId in students.usersId)
            {
                var existingUserProject = existingProject.UserProjects
                .FirstOrDefault(u => u.UserId == userId);


                if (existingUserProject != null)
                {
                    if (existingUserProject.IsDeleted)
                    {
                        // Change IsDeleted to false
                        existingUserProject.JoinAt = DateTime.UtcNow;
                        existingUserProject.IsDeleted = false;
                        existingUserProject.Role = "student";
                        existingUserProject.DeletedNotes = null;
                    }
                    else if (existingUserProject.Role == "student")
                    {
                        return BadRequest(new ApiResponse(400, $"A student {existingUserProject.User.UserName} already exists and is active."));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse(400, "User already assigned to the project with a different role."));
                    }
                }
                else
                {
                    existingProject.UserProjects.Add(new UserProject { UserId = userId, Role = "Student" });
                }
            }

            existingProject.Status = "active";

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            // Save changes to the database
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add students."));
            }

            return Ok(new ApiResponse(200, "Students added successfully."));
        }
        */


        /* AddCoSupervisor
         * 
         * 
         * 
        [HttpPost("co-supervisor")]
        public async Task<ActionResult<ApiResponse>> AddCoSupervisor([Required] int projectId, [Required] CreateCoSupervisorDTO supervisor)
        {

            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            var existingUserProject = existingProject.UserProjects
                .FirstOrDefault(u => u.UserId == supervisor.userId);

            if (existingUserProject != null)
            {
                if (existingUserProject.IsDeleted)
                {
                    // Change IsDeleted to false
                    existingUserProject.IsDeleted = false;
                    existingUserProject.Role = "co-supervisor";
                    existingUserProject.DeletedNotes = null;
                }
                else if (existingUserProject.Role == "co-supervisor")
                {
                    return BadRequest(new ApiResponse(400, "A co-supervisor already exists and is active."));
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "User already assigned to the project with a different role."));
                }
            }
            else
            {
                var user = await userManager.FindByIdAsync(supervisor.userId);
                if (user == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {supervisor.userId} not found." }));
                }

                existingProject.UserProjects.Add(new UserProject { UserId = supervisor.userId, Role = "co-supervisor" });
            }


            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            // Save changes to the database
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add Co-Supervisor."));
            }

            return Ok(new ApiResponse(200, "Co-Supervisor added successfully."));
        }
        */


        /* Update
         * 
         * 
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update([Required] int id, [FromBody] ProjectCreateDTO project)
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

            // Update project fields
            existingProject.Name = project.Name;

            // Clear existing UserProjects and set new supervisor and customer
            existingProject.UserProjects.Clear();
            existingProject.UserProjects.Add(new UserProject { UserId = supervisor.Id, Role = "Supervisor" });
            existingProject.UserProjects.Add(new UserProject { UserId = customer.Id, Role = "Customer" });

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);
            int successSave = await unitOfWork.save();

            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Project updated successfully"));
        }
        */


        [HttpPut("{projectId}/status")]
        public async Task<ActionResult<ApiResponse>> UpdateProjectStatus(
            int projectId,
            [FromBody, Required] ChangeProjectStatusDTO changeProjectStatusDTO)
        {
            // Check if project exists
            var project = await unitOfWork.projectRepository.GetById(projectId);
            if (project == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate the new status
            var validStatuses = new List<string> { "active", "completed", "pending", "canceled" };
            if (string.IsNullOrWhiteSpace(changeProjectStatusDTO.status) || !validStatuses.Contains(changeProjectStatusDTO.status.ToLower()))
            {
                return BadRequest(new ApiResponse(400, $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}"));
            }

            // Update the project status
            project.Status = changeProjectStatusDTO.status;
            project.ChangeStatusNotes = changeProjectStatusDTO.notes;
            project.ChangeStatusAt = DateTime.UtcNow;

            if (project.Status == "completed")
            {
                project.EndDate = DateTime.UtcNow;
            }

            // Save changes to database
            unitOfWork.projectRepository.Update(project);
            var result = await unitOfWork.save();
            if (result <= 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update project status."));
            }

            return Ok(new ApiResponse(200, $"Project status updated to '{changeProjectStatusDTO.status}'."));
        }



        /* ChangeSupervisor
         * 
         * 
         * 
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
        */


        /* DeleteStudent
         * 
         * 
         * 
        [Authorize(Roles = "supervisor,admin")]
        [HttpDelete("{projectId}/students")]
        public async Task<ActionResult<ApiResponse>> DeleteStudent(
            int projectId,
            [FromQuery, Required] string studentId,
            [FromBody] NotesDTO notes)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Student ID is required." }));
            }

            // Fetch the project and include UserProjects to check if the student exists
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Find the student entry in the UserProjects collection
            var studentEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.UserId == studentId && up.Role == "student");

            if (studentEntry == null)
            {
                return NotFound(new ApiResponse(404, "Student not found in this project."));
            }

            studentEntry.IsDeleted = true;
            studentEntry.DeletedNotes = notes.Notes;
            studentEntry.DeletededAt = DateTime.UtcNow;

            //// Remove the student entry from the UserProjects collection
            //existingProject.UserProjects.Remove(studentEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the student from the project."));
            }

            return Ok(new ApiResponse(200, "Student removed successfully."));
        }
        */


        /* DeleteCoSupervisor
         * 
         * 
         * 
         * 
        [Authorize(Roles = "admin")]
        [HttpDelete("{projectId}/co-supervisor")]
        public async Task<ActionResult<ApiResponse>> DeleteCoSupervisor(
            int projectId,
            [FromQuery, Required] string co_supervisorId,
            [FromBody] NotesDTO notes)
        {

            // Fetch the project and include UserProjects to check if the student exists
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Find the student entry in the UserProjects collection
            var co_supervisorEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.UserId == co_supervisorId && up.Role == "co-supervisor");

            if (co_supervisorEntry == null)
            {
                return NotFound(new ApiResponse(404, "Co-Supervisor not found in this project."));
            }

            co_supervisorEntry.IsDeleted = true;
            co_supervisorEntry.DeletedNotes = notes.Notes;
            co_supervisorEntry.DeletededAt = DateTime.UtcNow;

            //// Remove the student entry from the UserProjects collection
            //existingProject.UserProjects.Remove(supervisorEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the Co-Supervisor from the project."));
            }

            return Ok(new ApiResponse(200, "Co-Supervisor removed successfully."));
        }
        */

    }

}