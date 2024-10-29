using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using System.Linq.Expressions;
using System.Web;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository userRepository;
        private readonly UserManager<LocalUser> userManager;
        private readonly IEmailService emailService;

        public AuthController(IAuthRepository userRepository, UserManager<LocalUser> userManager, IEmailService emailService)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            if (ModelState.IsValid)
            {
                var user = await userRepository.Login(model);
                if (user.User == null)
                {
                    return Unauthorized(new ApiValidationResponse(new List<string>() { "Email or password inCorrect" }, 401));
                }
                return Ok(user);
            }
            return BadRequest(new ApiValidationResponse(new List<string>() { "Please try to enter the email and password correctly" }, 400));
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            if (model.Role.ToLower() != "customer" && model.Role.ToLower() != "student")
            {
                return BadRequest(new ApiResponse(400,"Invalid role. The role must be either 'customer' or 'student'."));
            }
            if (model.Role.ToLower() == "customer" && string.IsNullOrEmpty(model.CompanyName))
            {
                return BadRequest(new ApiResponse(400, "Company name is required for customers."));
            }

            var response = await userRepository.Register(model);

            if (response is ApiValidationResponse validationResponse)
            {
                return BadRequest(validationResponse);
            }
            else if (response is ApiResponse apiResponse && !apiResponse.IsSuccess)
            {
                return StatusCode(apiResponse.StatusCode ?? 500, apiResponse);
            }

            return Ok(response);
        }


        [HttpPost("auth/request-password-reset")]
        public async Task<IActionResult> SendEmailForUser(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"Email '{email}' not found." }));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordLink = Url.Action(
                "ResetPassword",
                "Users",
                new { token, email },
                Request.Scheme
            );

            var subject = "Reset Password Request";
            var message = $"Please click the following link to reset your password: {resetPasswordLink}";

            await emailService.SendEmailAsync(email, subject, message);

            return Ok(new ApiResponse(200, "Password reset link has been sent. Please check your email."));
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid input. Please check your information."));
            }

            if (model.newPassword != model.confirmNewPassword)
            {
                return BadRequest(new ApiResponse(400, "Passwords do not match."));
            }

            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new ApiResponse(400, "Invalid or missing token."));
            }

            // Decode the token
            string decodedToken = HttpUtility.UrlDecode(model.Token);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User with the provided email not found."));
            }

            var result = await userManager.ResetPasswordAsync(user, decodedToken, model.newPassword);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "Password reset successfully."));
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new ApiValidationResponse(errors, 400));
        }


    }
}
