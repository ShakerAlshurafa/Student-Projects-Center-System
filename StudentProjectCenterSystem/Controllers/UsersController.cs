using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork<LocalUser> unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersController(IUnitOfWork<LocalUser> unitOfWork, IMapper mapper
                            , UserManager<LocalUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }


        [HttpGet("get-users")]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] string? userName = null, [FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<LocalUser, bool>> filter = null!;
            if (!string.IsNullOrEmpty(userName))
            {
                filter = x => x.UserName.Contains(userName);
            }

            var usersList = await unitOfWork.userRepository.GetAll(filter, PageSize, PageNumber);

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

        [HttpPut("change-role")]
        public async Task<ActionResult<ApiResponse>> ChangeRole([FromQuery, Required] string userId, [FromBody, Required] string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                return BadRequest(new ApiResponse(400, "User ID and new role are required."));
            }

            // Find the user by ID
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            // Check if the role exists
            var roleExists = await roleManager.RoleExistsAsync(newRole);
            if (!roleExists)
            {
                return BadRequest(new ApiResponse(400, $"The role '{newRole}' does not exist."));
            }

            // Get the current roles of the user
            var currentRoles = await userManager.GetRolesAsync(user);

            // Remove current roles
            var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRolesResult.Succeeded)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove current roles."));
            }

            // Add the new role
            var addRoleResult = await userManager.AddToRoleAsync(user, newRole);
            if (!addRoleResult.Succeeded)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add new role."));
            }

            return Ok(new ApiResponse(200, $"Role changed successfully to '{newRole}' for user '{user.UserName}'."));
        }

    }
}
