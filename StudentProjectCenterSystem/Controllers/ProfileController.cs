using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using StudentProjectsCenter.Core.Entities.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using StudentProjectsCenter.Core.Entities.DTO.Users;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System.Linq.Expressions;
using StudentProjectsCenter.Core.Entities.DTO.Profile;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;

namespace StudentProjectsCenter.Controllers
{
    [Authorize]
    [Route("api/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<LocalUser> _userManager;
        private readonly IMapper mapper;

        public ProfileController(
            IWebHostEnvironment environment, 
            UserManager<LocalUser> userManager,
            IMapper mapper)
        {
            _environment = environment;
            _userManager = userManager;
            this.mapper = mapper;
        }

        /* UploadProfileImage
         * 
         * 
         * 
         * 
         * 
        [HttpPost("upload")]
        public async Task<IActionResult> UploadProfileImage([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse { Message = "File is empty." });

            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Message = "User is not authenticated." });
            }

            // Retrieve the user from the database
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Message = "User not found." });
            }

            // Allowed file extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension.ToLower()))
                return BadRequest(new ApiResponse { Message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." });

            // File path setup
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            try
            {
                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate the file URL
                var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

                // Update the user's profile image URL
                user.ProfileImageUrl = fileUrl;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(new ApiResponse { Message = "Failed to update user profile." });
                }

                // Return a success response
                return Ok(new ApiResponse(
                    message: "File uploaded successfully.",
                    result: fileUrl
                ));
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse {
                        Message = $"An error occurred while processing the request. Details: {ex.Message}"
                    });
            }
        }
        */

        
        [HttpGet("user-info")]
        public async Task<ActionResult<ApiResponse>> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var userInfo = mapper.Map<UserProfileDTO>(user);

            var roles = await _userManager.GetRolesAsync(user);
            userInfo.Role = roles.ToList();

            return Ok(new ApiResponse(200, result: userInfo));
        }

        [HttpPut("profile-image")]
        public async Task<IActionResult> UpdateProfileImage([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse { Message = "File is empty." });

            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Message = "User is not authenticated." });
            }

            // Retrieve the user from the database
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Message = "User not found." });
            }

            // Allowed file extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension.ToLower()))
                return BadRequest(new ApiResponse { Message = "Invalid file type. Only .jpg, .jpeg, and .png are allowed." });

            // File path setup
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            try
            {
                // Delete the old profile image if it exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, "uploads", Path.GetFileName(user.ProfileImageUrl));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save the new file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate the file URL
                var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

                // Update the user's profile image URL
                user.ProfileImageUrl = fileUrl;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(new ApiResponse { Message = "Failed to update user profile." });
                }

                // Return a success response
                return Ok(new ApiResponse(
                    message: "Profile image updated successfully.",
                    result: fileUrl
                ));
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse
                    {
                        Message = $"An error occurred while processing the request. Details: {ex.Message}"
                    });
            }
        }


        [HttpPut("user-info")]
        public async Task<IActionResult> UpdateUserInfo(UpdateUserProfileDTO userProfileDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return BadRequest(new ApiValidationResponse(errors));
            }

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user ID
            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update user info
            user.FirstName = userProfileDTO.FirstName;
            user.MiddleName = userProfileDTO.MiddleName;
            user.LastName = userProfileDTO.LastName;
            user.PhoneNumber = userProfileDTO.PhoneNumber;
            user.Address = userProfileDTO.Address;
            user.UserName = userProfileDTO.FirstName + "_" + userProfileDTO.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to update user information.");
            }

            return Ok(new ApiResponse(200, "User information updated successfully."));
        }

    }
}
