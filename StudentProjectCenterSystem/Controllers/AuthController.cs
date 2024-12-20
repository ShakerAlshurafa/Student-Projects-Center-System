using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                var response = await userRepository.Login(model);

                if (!response.IsSuccess)
                {
                    return Unauthorized(new ApiValidationResponse(new List<string> { response.ErrorMessage ?? "" }, 401));
                }

                return Ok(response);
            }

            return BadRequest(new ApiValidationResponse(new List<string> { "Please try to enter the email and password correctly." }, 400));
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
        {
            var response = await userRepository.Register(model);

            if (response is ApiValidationResponse validationResponse)
            {
                return BadRequest(validationResponse);
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
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "UserId and Token are required." });
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Email confirmation failed.", errors = result.Errors });
            }

            return Ok(new { message = "Email confirmed successfully. You can now log in." });
        }



        [HttpPost("send-password-reset-link")]
        public async Task<IActionResult> SendPasswordResetLink([Required] string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"Email '{email}' not found." }));
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordLink = Url.Action(
                "ResetPassword",
                "Auth",
                new { token, email },
                Request.Scheme
            );

            var subject = "Reset Password Request";
            // HTML email body with a button
            var message = $@"
                <p>Please click the button below to reset your password:</p>
                <br>
                <a href='{resetPasswordLink}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007bff; text-decoration: none; border-radius: 5px;'>Reset Password</a>
                <p>If you did not request a password reset, please ignore this email.</p>";

            var emailSent = await emailService.SendEmailAsync(email, subject, message, isHtml: true);

            if (!emailSent.IsSuccess)
            {
                return StatusCode(500, $"An error occurred while sending the message: {emailSent.ErrorMessage}");
            }
            return Ok(new ApiResponse(200, "Password reset link has been sent. Please check your email."));
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model, string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(400, "Invalid input. Please check your information."));
            }

            if (model.newPassword != model.confirmNewPassword)
            {
                return BadRequest(new ApiResponse(400, "Passwords do not match."));
            }

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse(400, "Invalid or missing token."));
            }

            // Decode the token
            string decodedToken = HttpUtility.UrlDecode(token);

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
