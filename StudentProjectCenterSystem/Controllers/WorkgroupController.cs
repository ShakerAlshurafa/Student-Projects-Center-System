using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.Linq.Expressions;
using System.Security.Claims;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/workgroups")]
    [ApiController]
    public class WorkgroupController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public WorkgroupController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? workgroupName = null)
        {
            Expression<Func<Workgroup, bool>> filter = x =>
                x.Project != null && x.Project.UserProjects.Any(u => !u.IsDeleted);
            if (!string.IsNullOrEmpty(workgroupName))
            {
                filter = x => x.Name.Contains(workgroupName);
            }

            var model = await unitOfWork.workgroupRepository.GetAll(filter, "Project.UserProjects.User");

            if (!model.Any())
            {
                return new ApiResponse(200, "No Workgroups Found");
            }

            var workgroupDTOs = model.Select(w =>
            {
                var userProjects = w?.Project?.UserProjects
                    .Where(u => u.ProjectId == w.Project.Id && !u.IsDeleted) 
                    ?? new List<UserProject>();
                var customer = userProjects
                        .FirstOrDefault(u => u?.Role == "customer" && !u.IsDeleted);
                return new AllWorkgroupsDTO
                {
                    Id = w.Id,
                    Name = w.Name,
                    SupervisorName = userProjects
                                        .FirstOrDefault(u => u?.Role == "supervisor")?.User.UserName ?? string.Empty,
                    CoSupervisorName = userProjects
                                        .Where(u => u?.Role == "co-supervisor" && !u.IsDeleted)
                                        .Select(u => u.User.UserName)
                                        .ToList(),
                    CustomerName = customer?.User.UserName ?? string.Empty,
                    Company = w?.Project?.CompanyName ?? string.Empty,
                    Team = userProjects
                                .Where(u => u?.Role == "student" && !u.IsDeleted)?
                                .Select(up => up?.User?.FirstName + " " + up?.User?.LastName)
                                .ToList()
                                ?? new List<string>(),
                };
            });

            int workgroups_count = await unitOfWork.workgroupRepository.Count(filter);

            return new ApiResponse(200, "Workgroups retrieved successfully", new
            {
                Total = workgroups_count,
                Workgroups = workgroupDTOs
            });
        }


        [HttpGet("get-all-for-user/{PageSize}/{PageNumber}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> GetAllForUser(
            int PageSize = 6,
            int PageNumber = 1)
        {
            // Retrieve the logged-in user's ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(401, "User not Find."));
            }

            // Filter workgroups where the logged-in user is associated
            Expression<Func<Workgroup, bool>> filter = x => x.Project != null &&
                x.Project.UserProjects != null &&
                x.Project.UserProjects.Any(up => up.UserId == userId && !up.IsDeleted);

            var workgroups = await unitOfWork.workgroupRepository.GetAll(filter, PageSize, PageNumber, "Project.UserProjects.User");
            if (!workgroups.Any())
            {
                return Ok(new ApiResponse(200, "No Workgroups Found for the user."));
            }

            var role = workgroups
                .Select(p => p?.Project?.UserProjects
                    .Where(u => u.UserId == userId && !u.IsDeleted)
                    .Select(u => u.Role)
                    .FirstOrDefault())
                .FirstOrDefault();

            // Transform data into DTO
            var workgroupDTOs = workgroups.Select(w =>
            {
                var userProjects = w?.Project?.UserProjects ?? new List<UserProject>();
                var customer = userProjects
                        .FirstOrDefault(u => u.Role == "customer" && !u.IsDeleted);
                return new AllWorkgroupsDTO
                {
                    Id = w.Id,
                    Name = w.Name,
                    Role = role ?? "",
                    SupervisorName = userProjects
                        .FirstOrDefault(u => u.Role == "supervisor" && !u.IsDeleted)?.User.UserName ?? string.Empty,
                    CoSupervisorName = userProjects
                                        .Where(u => u.Role == "co-supervisor" && !u.IsDeleted)
                                        .Select(u => u.User.UserName)
                                        .ToList(),
                    CustomerName = customer?.User.UserName ?? string.Empty,
                    Company = w?.Project?.CompanyName ?? string.Empty,
                    Team = userProjects
                        .Where(u => u.Role == "student" && u.User?.UserName != null && !u.IsDeleted)
                        .Select(u => u.User!.UserName!)
                        .ToList() ?? new List<string>()
                };
            });

            int workgroups_count = await unitOfWork.workgroupRepository.Count(filter);

            return Ok(new ApiResponse(200, "Workgroups retrieved successfully for the user.", new
            {
                Total = workgroups_count,
                Workgroups = workgroupDTOs
            }));
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetById(int id)
        {
            // Retrieve the workgroup with related data for Project, UserProjects, and associated users.
            var workgroup = await unitOfWork.workgroupRepository.GetById(id, includeProperty: "Project.UserProjects.User");

            if (workgroup == null)
            {
                return NotFound(new ApiResponse(404, "Workgroup not found."));
            }

            var role = "";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                role = workgroup.Project?.UserProjects
                        .Where(u => u.UserId == userId && !u.IsDeleted)
                        .Select(u => u.Role)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(role))
            {
                return Ok(new ApiResponse(200, "You are not in any workgroup"));
            }

            var userProjects = workgroup.Project?.UserProjects.Where(u => !u.IsDeleted).ToList();

            var supervisor = userProjects?.FirstOrDefault(u => u.Role == "supervisor")?.User;
            var co_supervisor = userProjects?
                                    .Where(u => u.Role == "co-supervisor")
                                    .Select(u => new WorkgroupUsersDTO
                                    {
                                        userId = u?.UserId ?? "",
                                        FullName = string.Join(" ", [u?.User?.FirstName, u?.User?.MiddleName, u?.User?.LastName]),
                                        Email = u?.User?.Email ?? "",
                                        Role = "co_supervisor"
                                    })
                                    .ToList();
            var customer = userProjects?.FirstOrDefault(u => u.Role == "customer")?.User;
            var students = userProjects?
                        .Where(u => u.Role == "student")
                        .Select(u => new WorkgroupUsersDTO()
                        {
                            userId = u.UserId,
                            FullName = string.Join(" ", [u?.User.FirstName, u?.User.MiddleName, u?.User.LastName]),
                            Email = u?.User.Email ?? "",
                            Role = u?.Role ?? ""
                        })
                        .ToList() ?? new List<WorkgroupUsersDTO>();

            var members = new List<WorkgroupUsersDTO>()
            {
                new WorkgroupUsersDTO
                {
                    userId = supervisor?.Id ?? "",
                    FullName = string.Join(" ", [supervisor?.FirstName, supervisor?.MiddleName, supervisor?.LastName]),
                    Email = supervisor?.Email ?? "",
                    Role = "supervisor"
                },
                new WorkgroupUsersDTO
                {
                    userId = customer?.Id ?? "",
                    FullName = string.Join(" ", [customer?.FirstName, customer?.MiddleName, customer?.LastName]),
                    Email = customer?.Email ?? "",
                    Role = "customer"
                },
            };
            foreach (var student in students)
            {
                members.Add(student);
            }
            if (co_supervisor != null)
            {
                members.AddRange(co_supervisor);
            }
            var workgroupDto = new WorkgroupDTO
            {
                Id = workgroup.Id,
                Name = workgroup.Name,
                Role = (role == "" ? null : role),
                Progress = workgroup.Progress,
                Members = members.ToList(),
                ProjectId = workgroup?.Project?.Id
            };

            return Ok(new ApiResponse(200, "Workgroup retrieved successfully", workgroupDto));
        }

    }
}
