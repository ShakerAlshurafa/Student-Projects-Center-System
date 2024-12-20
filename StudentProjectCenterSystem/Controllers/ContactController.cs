using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Controllers
{
    [Route("api/contact-us")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService emailService;

        public ContactController(IEmailService emailService)
        {
            this.emailService = emailService;
        }

        [HttpPost("contact-us")]
        public async Task<IActionResult> ContactUs([FromBody, Required] ContactUsDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string emailContent = $@"
                <p><strong>Message:</strong></p>
                <p>{model.Message}</p>
                <p><strong>Email:</strong> {model.Email}</p>
                <p><strong>Phone:</strong> {model.Phone}</p>";

            // Process the message (e.g., send email, save to the database, etc.)
            var emailSent = await emailService.SendEmailAsync("studentprojectscentersystem@gmail.com", "New Contact Us Message", emailContent, true);

            if (!emailSent.IsSuccess)
            {
                return StatusCode(500, $"An error occurred while sending the message: {emailSent.ErrorMessage}");
            }

            return Ok(new ApiResponse(200, "Your message has been sent successfully!"));
        }
    }
}
