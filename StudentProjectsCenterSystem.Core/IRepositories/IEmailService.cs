using StudentProjectsCenter.Core.Entities.DTO;

namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IEmailService
    {
        Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }
}
