using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IUnitOfWork<Project> unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public ProjectsController(IUnitOfWork<Project> unitOfWork, IMapper mapper, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        [HttpGet]
        [ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? projectName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<Project, bool>> filter = null!;
            if (!string.IsNullOrEmpty(projectName))
            {
                filter = x => x.Name.Contains(projectName);
            }
            var model = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User"); ;

            if (!model.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projectDTOs = model.Select(project => new ProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                SupervisorName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Supervisor")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Supervisor")?.User?.LastName
                                ?? "No Supervisor Assigned",
                CoSupervisorName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Co-Supervisor")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Co-Supervisor")?.User?.LastName
                                ?? "No Co-Supervisor Assigned",
                CustomerName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.LastName
                                ?? "No Customer Assigned",
                Company = project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.CompanyName
                                ?? "No Customer Assigned",
                WorkgroupName = project?.Workgroup?.Name ?? "No Workgroup",
                Team = project?.UserProjects?.Where(up => up.Role == "Student")?
                                .Select(up => up?.User?.FirstName + " " + up?.User?.LastName)
                                .ToList()
                                ?? new List<string>(),
                Favorite = project?.Favorite ?? false
            }).ToList();

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }


        // test this end point
        [HttpGet("mine")]
        //[ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAllMyProject([FromQuery] string? projectName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<Project, bool>> filter = null!;
            if (!string.IsNullOrEmpty(projectName))
            {
                filter = x => x.Name.Contains(projectName);
            }

            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new ApiResponse(401, "User not logged in"));
            }

            // Find the user by username
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(401, "User not found"));
            }

            var model = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User");
            if (!model.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projects = model.Where(p => p.UserProjects.Any(up => up.UserId == user?.Id)).ToList();

            var projectDTOs = mapper.Map<List<MyProjectDTO>>(projects);

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }


        // To return project with details
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
            var model = new Project
            {
                Name = project.Name,
                StartDate = DateTime.Now,
                Workgroup = workgroup,
                UserProjects = new List<UserProject>
                {
                    new UserProject { UserId = supervisor.Id, Role = "Supervisor" },
                    new UserProject { UserId = customer.Id, Role = "Customer" }
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


        //[HttpDelete("{id}")]
        //public async Task<ActionResult<ApiResponse>> Delete([Required] int id)
        //{
        //    int successDelete = unitOfWork.projectRepository.Delete(id);
        //    if (successDelete == 0)
        //    {
        //        return NotFound(new ApiResponse(404));
        //    }

        //    int successSave = await unitOfWork.save();
        //    if (successSave == 0)
        //    {
        //        return StatusCode(500, new ApiResponse(500, "Deleted failed!"));
        //    }

        //    return Ok(new ApiResponse(200, "Deleted Successfully"));
        //}


        [HttpPut("{projectId}/status")]
        public async Task<ActionResult<ApiResponse>> UpdateProjectStatus(int projectId, [FromBody, Required] string status)
        {
            // Check if project exists
            var project = await unitOfWork.projectRepository.GetById(projectId);
            if (project == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate the new status
            var validStatuses = new List<string> { "Active", "Completed", "Pending" };
            if (string.IsNullOrWhiteSpace(status) || !validStatuses.Contains(char.ToUpper(status[0]) + status.Substring(1).ToLower()))
            {
                return BadRequest(new ApiResponse(400, $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}"));
            }

            // Update the project status
            project.Status = status;

            // Save changes to database
            unitOfWork.projectRepository.Update(project);
            var result = await unitOfWork.save();
            if (result <= 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update project status."));
            }

            return Ok(new ApiResponse(200, $"Project status updated to '{status}'."));
        }



        [HttpPost("students")]
        public async Task<ActionResult<ApiResponse>> AddStudent([Required] int projectId, [Required] CreateStudentDTO students)
        {
            if (students.usersId == null || !students.usersId.Any())
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "No student IDs provided." }));
            }

            var existingProject = await unitOfWork.projectRepository.GetById(projectId);
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
                existingProject.UserProjects.Add(new UserProject { UserId = userId, Role = "Student" });
            }

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


        [HttpDelete("students")]
        public async Task<ActionResult<ApiResponse>> DeleteStudent(
            [FromQuery, Required] int projectId,
            [FromQuery, Required] string studentId)
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
                                .FirstOrDefault(up => up.UserId == studentId && up.Role == "Student");

            if (studentEntry == null)
            {
                return NotFound(new ApiResponse(404, "Student not found in this project."));
            }

            // Remove the student entry from the UserProjects collection
            existingProject.UserProjects.Remove(studentEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the student from the project."));
            }

            return Ok(new ApiResponse(200, "Student removed successfully."));
        }



        [HttpPost("co-supervisor")]
        public async Task<ActionResult<ApiResponse>> AddCoSupervisor([Required] int projectId, [Required] CreateCoSupervisorDTO supervisor)
        {

            var existingProject = await unitOfWork.projectRepository.GetById(projectId);
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate that all students exist in the system
            var user = await userManager.FindByIdAsync(supervisor.userId);
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {supervisor.userId} not found." }));
            }

            existingProject.UserProjects.Add(new UserProject { UserId = supervisor.userId, Role = "Co-Supervisor" });

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


        [HttpDelete("co-supervisor")]
        public async Task<ActionResult<ApiResponse>> DeleteCoSupervisor(
            [FromQuery, Required] int projectId,
            [FromQuery, Required] string supervisorId)
        {
            if (string.IsNullOrEmpty(supervisorId))
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Co-Supervisor ID is required." }));
            }

            // Fetch the project and include UserProjects to check if the student exists
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Find the student entry in the UserProjects collection
            var supervisorEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.UserId == supervisorId && up.Role == "Co-Supervisor");

            if (supervisorEntry == null)
            {
                return NotFound(new ApiResponse(404, "Supervisor not found in this project."));
            }

            // Remove the student entry from the UserProjects collection
            existingProject.UserProjects.Remove(supervisorEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the Co-Supervisor from the project."));
            }

            return Ok(new ApiResponse(200, "Co-Supervisor removed successfully."));
        }
    }

}