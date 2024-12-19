using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/roles")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<LocalUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController(
            IUnitOfWork unitOfWork, 
            UserManager<LocalUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            var roles = await roleManager.Roles.ToListAsync();
            if (roles == null || !roles.Any())
            {
                return NotFound(new ApiResponse(404, "No roles found."));
            }

            // Select only the role names
            var roleNames = roles.Select(r => new
            {
                r.Id,
                r.Name
            }).ToList();

            return Ok(new ApiResponse(200, "Roles retrieved successfully.", roleNames));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateRole([Required] string roleName)
        {
            var role = new IdentityRole(roleName);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Failed to create role."));
            }

            return Ok(new ApiResponse(200, "Role created successfully."));
        }


        [HttpPut("{roleId}")]
        public async Task<ActionResult<ApiResponse>> UpdateRole(string roleId, [Required] string newRoleName)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }

            if (role.Name == "user" || role.Name == "admin" || role.Name == "supervisor")
            {
                return BadRequest(new ApiResponse(400, "Not acceptable to update this system role."));
            }


            role.Name = newRoleName;
            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Failed to update role."));
            }

            return Ok(new ApiResponse(200, "Role updated successfully."));
        }


        [HttpPost("{roleId}/assign-to-user")]
        public async Task<ActionResult<ApiResponse>> AssignRoleToUser(string roleId, [Required] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }

            var result = await userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Failed to assign role to user."));
            }

            return Ok(new ApiResponse(200, "Role assigned to user successfully."));
        }


        [HttpPost("{roleId}/remove-from-user")]
        public async Task<ActionResult<ApiResponse>> RemoveRoleFromUser(string roleId, [Required] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }

            // Prevent deletion of the "user" role
            if (role.Name == "user")
            {
                return BadRequest(new ApiResponse(400, "Not acceptable to remove the 'user' role."));
            }

            var result = await userManager.RemoveFromRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Failed to remove role from user."));
            }

            return Ok(new ApiResponse(200, "Role removed from user successfully."));
        }
        
        
        [HttpPut("change/{userId}")]
        public async Task<ActionResult<ApiResponse>> ChangeRole(string userId, [FromBody, Required] string newRole)
        {

            // Find the user by ID
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            newRole = newRole.ToLower();

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


        [HttpDelete("{roleId}")]
        public async Task<ActionResult<ApiResponse>> DeleteRole(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }

            if (role.Name == "user" || role.Name == "admin" || role.Name == "supervisor")
            {
                return BadRequest(new ApiResponse(400, "Not acceptable to remove this system role."));
            }


            // Proceed with role deletion
            var result = await roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "Role deleted successfully."));
            }

            return StatusCode(500, new ApiResponse(500, "Failed to delete the role."));
        }




    }
}
