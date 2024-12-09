namespace StudentProjectsCenterSystem.Core.IRepositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }
}
