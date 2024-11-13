using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

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

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetById([Required,FromQuery] int workgroupId)
        {
            // Retrieve the workgroup with related data for Project, UserProjects, and associated users.
            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId, includeProperty: "Project.UserProjects.User");

            if (workgroup == null)
            {
                return NotFound(new ApiResponse(404, "Workgroup not found."));
            }

            var workgroupDto = new WorkgroupDTO
            {
                Id = workgroup.Id,
                Name = workgroup.Name,
                Progress = workgroup.Progress,
                SupervisorName = workgroup.Project?.UserProjects
                                .FirstOrDefault(u => u.Role == "Supervisor")?.User.UserName ?? string.Empty,
                CoSupervisorName = workgroup.Project?.UserProjects
                                .FirstOrDefault(u => u.Role == "Co-Supervisor")?.User.UserName,
                CustomerName = workgroup.Project?.UserProjects
                                .FirstOrDefault(u => u.Role == "Customer")?.User.UserName ?? string.Empty,
                Company = workgroup.Project?.UserProjects
                                .FirstOrDefault(u => u.Role == "Customer")?.User.CompanyName ?? string.Empty,
                Team = workgroup?.Project?.UserProjects?
                        .Where(u => u.Role.ToLower() == "student" && u.User?.UserName != null)
                        .Select(u => u.User!.UserName!)  
                        .ToList() ?? new List<string>()

            };

            return Ok(new ApiResponse(200, "Workgroup retrieved successfully", workgroupDto));
        }

    }
}
