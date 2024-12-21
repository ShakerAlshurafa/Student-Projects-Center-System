using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Data;
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


        // Get all users
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            var usersList = await userManager.Users.Where(u => u.EmailConfirmed).ToListAsync();
            var userDTOs = new List<UserDTO>();

            foreach (var user in usersList)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    FullName = string.Join(" ",
                        new[] { user.FirstName, user.MiddleName, user.LastName }
                            .Where(name => !string.IsNullOrWhiteSpace(name))),
                    Email = user.Email ?? "",
                    Role = roles.ToList() ?? new List<string> { "No Role" }
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }

        // Get limit number of users
        [HttpGet("get-with-pagination")]
        public async Task<ActionResult<ApiResponse>> GetWithPagination([FromQuery] string? userName = null, [FromQuery] int PageSize = 6, int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x => x.EmailConfirmed;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName.Contains(userName) && x.EmailConfirmed;
            }

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber);

            var userDTOs = new List<UserDTO>();

            foreach (var user in usersList)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    FullName = string.Join(" ",
                        new[] { user.FirstName, user.MiddleName, user.LastName }
                            .Where(name => !string.IsNullOrWhiteSpace(name))),
                    Email = user.Email ?? "",
                    Role = roles.ToList() ?? new List<string> { "No Role" }
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
                Expression<Func<Project, bool>> filter = x => x.UserProjects
                    .Any(u => u.UserId == user.Id);
                var projects = await unitOfWork.projectRepository.GetAll(filter, "UserProjects");
                var projectsName = projects.Select(p => p.Name).ToList();
                userDTOs.Add(new SupervisorDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    ProjectsName = projectsName,
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }


        [HttpGet("role/{role}")]
        public async Task<ActionResult<ApiResponse>> GetByRole(string role)
        {
            try
            {
                var users = await userManager.GetUsersInRoleAsync(role.ToLower());

                if (users == null || !users.Any())
                {
                    return NotFound(new ApiResponse(404, "No users found for the given role."));
                }

                var userDTOs = mapper.Map<List<GetByRoleDTO>>(users);

                return Ok(new ApiResponse(200, result: userDTOs));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while fetching users.", ex.Message));
            }
        }


        [HttpGet("students")]
        public async Task<ActionResult<ApiResponse>> GetStudents([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "student");

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber, "UserProjects.Project");

            var userDTOs = new List<StudentDTO>();

            foreach (var user in usersList)
            {
                var project = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => new
                    {
                        p.Project.Status,
                        p.Project.Name
                    })
                    .FirstOrDefault();

                userDTOs.Add(new StudentDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    ProjectName = project?.Name ?? "",
                    ProjectStatus = project?.Status ?? ""
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


            return Ok(new ApiResponse(200, result: new
            {
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


        [HttpGet("get-user-info/{userId}")]
        public async Task<ActionResult<ApiResponse>> GetUserInfo(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var userInfo = mapper.Map<UserInfoDto>(user);

            var roles = await userManager.GetRolesAsync(user);
            userInfo.Role = roles.ToList();

            Expression<Func<Project, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.Any(u => u.UserId == userId);

            var projects = await unitOfWork.projectRepository.GetAll(filter, "UserProjects");
            var projectsName = projects.Select(projects => projects.Name).ToList();

            userInfo.ProjectsName = projectsName;

            return Ok(new ApiResponse(200, result: userInfo));
        }

        private string UpdateIfNotNullOrWhiteSpace(string target, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? target : value;
        }

        [HttpPut("change-user-info/{userId}")]
        public async Task<ActionResult<ApiResponse>> UpdateUserInfo(
            string userId,
            [FromBody, Required] UpdateUserInfoDTO model)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Update the user's information
            user.UserName = UpdateIfNotNullOrWhiteSpace(user.UserName ?? "", model.UserName);
            user.FirstName = UpdateIfNotNullOrWhiteSpace(user.FirstName, model.FirstName);
            user.MiddleName = UpdateIfNotNullOrWhiteSpace(user.MiddleName ?? "", model.MiddleName);
            user.LastName = UpdateIfNotNullOrWhiteSpace(user.LastName, model.LastName);


            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new ApiResponse(200, "User information updated successfully."));
        }

    }
}
