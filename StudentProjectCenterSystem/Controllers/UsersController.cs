using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenterSystem.Core.Entities;
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

        public UsersController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<LocalUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }


        // Get all users
        [Authorize(Roles = "admin,supervisor")]
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
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    Role = roles.ToList() ?? new List<string> { "No Role" }
                });
            }

            return Ok(new ApiResponse(200, "Users retrieved successfully", userDTOs));
        }

        // Get limit number of users
        [Authorize(Roles = "admin")]
        [HttpGet("get-with-pagination/{PageSize}/{PageNumber}")]
        public async Task<ActionResult<ApiResponse>> GetWithPagination(
            [FromQuery] string? userName = null,
            int PageSize = 6,
            int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = x => x.EmailConfirmed;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName != null && x.UserName.Contains(userName) && x.EmailConfirmed;
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

            int users_count = await unitOfWork.userRepository.Count(filter);

            return new ApiResponse(200, "Users retrieved successfully", new
            {
                Total = users_count,
                Users = userDTOs
            });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("supervisors")]
        public async Task<ActionResult<ApiResponse>> GetSupervisors()
        {
            var supervisors = await userManager.GetUsersInRoleAsync("supervisor");
            var userDTOs = new List<SupervisorDTO>();

            foreach (var user in supervisors)
            {
                Expression<Func<Project, bool>> filter = x => x.UserProjects
                    .Any(u => u.UserId == user.Id && !u.IsDeleted);
                var projects = await unitOfWork.projectRepository.GetAll(filter, "UserProjects");
                var projectsName = projects.Select(p => p.Name).ToList();
                var CountActive = projects.Count(p => p.Status == "active");
                var CountCompleted = projects.Count(p => p.Status == "completed");

                userDTOs.Add(new SupervisorDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    ProjectsName = projectsName,
                    CountActive = CountActive,
                    CountCompleted = CountCompleted
                });
            }

            return new ApiResponse(200, "Users retrieved successfully", userDTOs);
        }

        [Authorize(Roles = "admin")]
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

        [Authorize(Roles = "admin")]
        [HttpGet("students")]
        public async Task<ActionResult<ApiResponse>> GetStudents()
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.Any(u => u.Role.ToLower() == "student");

            var usersList = await unitOfWork.userRepository.GetAll(filter, "UserProjects.Project");

            var userDTOs = new List<StudentDTO>();

            foreach (var user in usersList)
            {
                var projects = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => new UserProjectDTO()
                    {
                        Name = p.Project.Name,
                        Status = p?.Project?.Status ?? ""
                    })
                    .ToList();

                userDTOs.Add(new StudentDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    Projects = projects
                });
            }

            int users_count = await unitOfWork.userRepository.Count(filter);

            return new ApiResponse(200, "Users retrieved successfully", new
            {
                Total = users_count,
                Students = userDTOs
            });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("co-supervisor")]
        public async Task<ActionResult<ApiResponse>> GetCoSupervisor()
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.Any(u => u.Role.ToLower() == "co-supervisor");

            var usersList = await unitOfWork.userRepository.GetAll(filter, "UserProjects.Project");

            var userDTOs = new List<StudentDTO>();

            foreach (var user in usersList)
            {
                var projects = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => new UserProjectDTO()
                    {
                        Name = p.Project.Name,
                        Status = p?.Project?.Status ?? ""
                    })
                    .ToList();

                userDTOs.Add(new StudentDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    Projects = projects
                });
            }

            int users_count = await unitOfWork.userRepository.Count(filter);

            return new ApiResponse(200, "Users retrieved successfully", new
            {
                Total = users_count,
                CoSupervisor = userDTOs
            });
        }

        [Authorize(Roles = "admin")]
        [HttpGet("customers")]
        public async Task<ActionResult<ApiResponse>> GetCustomers()
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "customer");

            var usersList = await unitOfWork.userRepository.GetAll(filter, "UserProjects.Project");

            var userDTOs = new List<CustomerDTO>();

            foreach (var user in usersList)
            {
                var workgroups = user.UserProjects?
                    .Where(u => u.UserId == user.Id)
                    .Select(p => p.Project.Name)
                    .ToList();

                userDTOs.Add(new CustomerDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? "", // Ensure Email is never null
                    WorkgroupName = workgroups ?? new List<string>(),
                    Company = user?.UserProjects?
                    .Where(p => p.ProjectId == p.Project.Id)
                    .Select(p => p.Project.CompanyName).FirstOrDefault() ?? ""
                });
            }
            int users_count = await unitOfWork.userRepository.Count(filter);

            return new ApiResponse(200, "Users retrieved successfully", new
            {
                Total = users_count,
                Customers = userDTOs
            });
        }

        [HttpGet("our-customers")]
        public async Task<ActionResult<ApiResponse>> GetOurCustomers()
        {
            Expression<Func<LocalUser, bool>> filter = x => x.UserProjects.Count > 0
                && x.UserProjects.Any(u => u.Role.ToLower() == "customer" && !u.IsDeleted)
                && x.UserProjects.Any(p=> p.Project.Status == "completed");

            var usersList = await unitOfWork.userRepository.GetAll(filter, "UserProjects.Project");

            var ourCustomer = mapper.Map<List<OurCustomerDTO>>(usersList);

            return new ApiResponse(200, "Users retrieved successfully", ourCustomer);
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
                x.UserProjects.All(u => u.Role.ToLower() == "supervisor")
            );
            var co_supervisorsActiveCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "co-supervisor")
            );

            var customersCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "customer")
            );
            var StudentsCount = await unitOfWork.userRepository.Count(
                x => x.UserProjects.Count > 0 &&
                x.UserProjects.All(u => u.Role.ToLower() == "student")
            );

            var projectsActiveCount = await unitOfWork.projectRepository.Count(x => x.Status == "active");
            var projectsCompletedCount = await unitOfWork.projectRepository.Count(x => x.Status == "completed");
            var projectsPendingCount = await unitOfWork.projectRepository.Count(x => x.Status == "pending");
            var projectsCanceledCount = await unitOfWork.projectRepository.Count(x => x.Status == "canceled");
            var projectTotalCount = projectsActiveCount + projectsCompletedCount + projectsPendingCount + projectsCanceledCount;

            return Ok(new ApiResponse(200, result: new
            {
                usersCount,
                usersActiveCount,

                supervisorsCount,
                supervisorsActiveCount,
                co_supervisorsActiveCount,
                customersCount,
                StudentsCount,

                projectTotalCount,
                projectsActiveCount,
                projectsCompletedCount,
                projectsPendingCount,
                projectsCanceledCount

            }));
        }

        [Authorize]
        [HttpGet("get-user-info/{userId}")]
        public async Task<ActionResult<ApiResponse>> GetUserInfo(string userId)
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

        [Authorize]
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
