using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Authentication;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Authentication;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly UserManager<LocalUser> userManager;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;

        public AuthController(
            IAuthRepository authRepository, 
            UserManager<LocalUser> userManager, 
            IEmailService emailService,
            IConfiguration configuration)
        {
            this.authRepository = authRepository;
            this.userManager = userManager;
            this.emailService = emailService;
            this.configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody,Required] LoginRequestDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await authRepository.Login(model);

                if (!response.IsSuccess)
                {
                    return Unauthorized(new ApiValidationResponse(new List<string> { response.ErrorMessage ?? "" }, 401));
                }

                return Ok(response);
            }

            return BadRequest(new ApiValidationResponse(new List<string> { "Please try to enter the email and password correctly." }, 400));
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody, Required] RegisterationRequestDTO model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                return BadRequest(new ApiValidationResponse(errors));
            }

            model.FirstName = model.FirstName.Replace(" ", "");
            model.MiddleName = model.MiddleName?.Replace(" ", "");
            model.LastName = model.LastName.Replace(" ", "");

            var response = await authRepository.Register(model);

            if (response is ApiValidationResponse)
            {
                return BadRequest(response);
            }
            else if (response is ApiResponse apiResponse && !apiResponse.IsSuccess)
            {
                return StatusCode(apiResponse.StatusCode ?? 500, apiResponse);
            }

            return Ok(new
            {
                message = "Registration successful. Please check your email to confirm your account.",
                user = response.Result
            });
        }


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(
            [Required] string userId, 
            [Required] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Email and Token are required." });
            }


            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Email confirmation failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            var frontendUrl = configuration["FrontendBaseUrl"];
            return Redirect($"{frontendUrl}/confirm-email");
        }


        [HttpPost("send-reset-code/{email}")]
        public async Task<IActionResult> SendPasswordResetLink([Required] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"Email '{email}' not found." }));
            }

            var resetCode = new Random().Next(100000, 999999).ToString();

            user.PasswordResetCode = resetCode;
            user.PasswordResetCodeExpiration = DateTime.UtcNow.AddMinutes(10); // Code valid for 10 minutes
            await userManager.UpdateAsync(user);

            var subject = "Password Reset Code";
            var message = $@"
                <p>You requested a password reset. Use the following 6-digit code to reset your password:</p>
                <h2>{resetCode}</h2>
                <p>This code is valid for 10 minutes.</p>
                <p>If you did not request a password reset, please ignore this email.</p>";

            var emailSent = await emailService.SendEmailAsync(email, subject, message, isHtml: true);
            if (!emailSent.IsSuccess)
            {
                return StatusCode(500, $"An error occurred while sending the message: {emailSent.ErrorMessage}");
            }

            return Ok(new ApiResponse(200, "Password reset code has been sent. Please check your email."));
        }


        [HttpPost("reset-forgotten-password/{email}")]
        public async Task<IActionResult> ResetForgottenPassword(
            string email,
            [FromBody, Required] ResetForgetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid input. Please check your information."));
            }

            if (model.newPassword != model.confirmNewPassword)
            {
                return BadRequest(new ApiResponse(400, "Passwords do not match."));
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null || user.PasswordResetCode != model.ResetCode || user.PasswordResetCodeExpiration < DateTime.UtcNow)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Invalid or expired reset code." }));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse(400, "Invalid token."));
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.newPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiValidationResponse(result.Errors.Select(e => e.Description).ToList()));
            }

            // Clear the reset code after successful password reset
            user.PasswordResetCode = null;
            user.PasswordResetCodeExpiration = null;
            await userManager.UpdateAsync(user);

            return Ok(new ApiResponse(200, "Password has been reset successfully."));
        }


        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody, Required] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid input. Please check your information."));
            }

            if (model.newPassword != model.confirmNewPassword)
            {
                return BadRequest(new ApiResponse(400, "Passwords do not match."));
            }

            var user = await userManager.GetUserAsync(User); // Get user object
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"User not found." }));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse(400, "Invalid or missing token."));
            }

            var result = await userManager.ResetPasswordAsync(user, token, model.newPassword);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "Password reset successfully."));
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new ApiValidationResponse(errors, 400));
        }

    }
}
