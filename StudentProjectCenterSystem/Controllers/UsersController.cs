using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper
                            , UserManager<LocalUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? userName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x=> x.EmailConfirmed;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName.Contains(userName) && x.EmailConfirmed;
            }

            var usersList = await unitOfWork.userRepository.GetAll(filter , PageSize, PageNumber);

            var userDTOs = new List<UserDTO>();

            foreach (var user in usersList)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? "No Role"
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }


        [HttpGet("supervisors")]
        public async Task<ActionResult<ApiResponse>> GetSupervisors()
        {
            var supervisors = await userManager.GetUsersInRoleAsync("supervisor");
            var userDTOs = new List<SupervisorDTO>();

            foreach (var user in supervisors)
            {
                userDTOs.Add(new SupervisorDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? ""
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }


        [HttpGet("students/active")]
        public async Task<ActionResult<ApiResponse>> GetStudents([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "student");

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber,"UserProjects.Project");

            var userDTOs = new List<StudentDTO>();

            foreach (var user in usersList)
            {
                var workgroupName = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => p.Project.Name)
                    .FirstOrDefault() ?? "";

                userDTOs.Add(new StudentDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "", // Ensure Email is never null
                    WorkgroupName = workgroupName
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }
        
        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse>> GetCustomers([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "customer");

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber, "UserProjects.Project");

            var userDTOs = new List<CustomerDTO>();

            foreach (var user in usersList)
            {
                var workgroupName = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => p.Project.Name)
                    .FirstOrDefault() ?? "";

                userDTOs.Add(new CustomerDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? "", // Ensure Email is never null
                    WorkgroupName = workgroupName,
                    Company = user.CompanyName ?? ""
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }


        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse>> GetStatistics()
        {
            var usersCount = await unitOfWork.userRepository.Count(x => x.EmailConfirmed);
            var usersActiveCount = await unitOfWork.userRepository.Count(x => x.UserProjects.Count > 0);

            var supervisors = await userManager.GetUsersInRoleAsync("supervisor");
            var supervisorsCount = supervisors.Count;

            var supervisorsActiveCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "supervisor" && !u.IsDeleted)
            );
            var co_supervisorsActiveCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "co-supervisor" && !u.IsDeleted)
            );

            var customersCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "customer" && !u.IsDeleted)
            );
            var StudentsCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "student" && !u.IsDeleted)
            );

            var projectsActiveCount = await unitOfWork.projectRepository.Count(x => x.Status == "active");
            var projectsCompletedCount = await unitOfWork.projectRepository.Count(x => x.Status == "completed");
            var projectsPendingCount = await unitOfWork.projectRepository.Count(x => x.Status == "pending");


            return Ok(new ApiResponse(200, result: new {
                usersCount,
                usersActiveCount,

                supervisorsCount,
                supervisorsActiveCount,
                co_supervisorsActiveCount,
                customersCount,
                StudentsCount,

                projectsActiveCount,
                projectsCompletedCount,
                projectsPendingCount
            }));
        }

    }
}
