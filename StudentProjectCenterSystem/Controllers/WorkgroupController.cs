using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/workgroups")]
    [ApiController]
    public class WorkgroupController : ControllerBase
    {
        private readonly IUnitOfWork<Workgroup> unitOfWork;
        private readonly IMapper mapper;

        public WorkgroupController(IUnitOfWork<Workgroup> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpGet("get-all")]
        public async Task<ActionResult<ApiResponse>> GetAll(
            [FromQuery] string? workgroupName = null, 
            [FromQuery] int PageSize = 6, 
            [FromQuery] int PageNumber = 1)
        {
            Expression<Func<Workgroup, bool>> filter = x => true;
            if (!string.IsNullOrEmpty(workgroupName))
            {
                filter = x => x.Name.Contains(workgroupName);
            }
            var model = await unitOfWork.workgroupRepository.GetAll(filter, PageSize, PageNumber, "Project.UserProjects.User");

            if (!model.Any())
            {
                return new ApiResponse(200, "No Workgroups Found");
            }

            var viewModel = model.Select(w =>
            {
                var userProjects = w?.Project?.UserProjects ?? new List<UserProject>();
                return new AllWorkgroupsDTO
                {
                    Id = w.Id,
                    Name = w.Name,
                    SupervisorName = userProjects
                                        .FirstOrDefault(u => u.Role == "Supervisor")?.User.UserName ?? string.Empty,
                    CoSupervisorName = userProjects
                                        .FirstOrDefault(u => u.Role == "Co-Supervisor")?.User.UserName ?? string.Empty,
                    CustomerName = userProjects
                                        .FirstOrDefault(u => u.Role == "Customer")?.User.UserName ?? string.Empty,
                    Company = userProjects
                                        .FirstOrDefault(u => u.Role == "Customer")?.User.CompanyName ?? string.Empty,
                };
            });

            return Ok(new ApiResponse(200, "Workgroups retrieved successfully", viewModel));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetById([Required] int id)
        {
            // Retrieve the workgroup with related data for Project, UserProjects, and associated users.
            var workgroup = await unitOfWork.workgroupRepository.GetById(id, includeProperty: "Project.UserProjects.User");

            if (workgroup == null)
            {
                return NotFound(new ApiResponse(404, "Workgroup not found."));
            }

            var userProjects = workgroup.Project?.UserProjects ?? new List<UserProject>();
            var workgroupDto = new WorkgroupDTO
            {
                Id = workgroup.Id,
                Name = workgroup.Name,
                Progress = workgroup.Progress,
                SupervisorName = userProjects
                                .FirstOrDefault(u => u.Role == "Supervisor")?.User.UserName ?? string.Empty,
                CoSupervisorName = userProjects
                                .FirstOrDefault(u => u.Role == "Co-Supervisor")?.User.UserName,
                CustomerName = userProjects
                                .FirstOrDefault(u => u.Role == "Customer")?.User.UserName ?? string.Empty,
                Company = userProjects
                                .FirstOrDefault(u => u.Role == "Customer")?.User.CompanyName ?? string.Empty,
                Team = userProjects
                        .Where(u => u.Role.ToLower() == "student" && u.User?.UserName != null)
                        .Select(u => u.User!.UserName!)  
                        .ToList() ?? new List<string>()

            };

            return Ok(new ApiResponse(200, "Workgroup retrieved successfully", workgroupDto));
        }


    }
}
