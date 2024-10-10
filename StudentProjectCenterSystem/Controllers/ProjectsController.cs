using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/[controller]")]
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


        [HttpGet("GetAll")]
        [ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? projectName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<Project, bool>> filter = null!;
            if (!string.IsNullOrEmpty(projectName))
            {
                filter = x => x.Name.Contains(projectName);
            }
            var model = await unitOfWork.projectRepository.GetAllWithUser(filter, PageSize, PageNumber);

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
            }).ToList();

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }


        [HttpGet("GetAllMyProject")]
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

            var model = await unitOfWork.projectRepository.GetAllWithUser(filter, PageSize, PageNumber);
            if (!model.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projects = model.Where(p => p.UserProjects.Any(up => up.UserId == user?.Id)).ToList();

            var projectDTOs = mapper.Map<List<MyProjectDTO>>(projects);

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }


        // To return project with details
        [HttpGet("{id}")]
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
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] ProjectCreateDTO project)
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


        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.projectRepository.Delete(id);
            if (successDelete == 0)
            {
                return NotFound(new ApiResponse(404));
            }

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Deleted failed!"));
            }

            return Ok(new ApiResponse(200, "Deleted Successfully"));
        }

    }

}
