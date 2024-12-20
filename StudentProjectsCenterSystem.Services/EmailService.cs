using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using StudentProjectsCenter.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.IRepositories;

namespace StudentProjectsCenterSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<EmailResult> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("Recipient email address cannot be empty.");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject cannot be empty.");
            if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Message body cannot be empty.");


            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Student Projects Center", configuration["EmailSettings:FromEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            // Determine the content type based on isHtml
            emailMessage.Body = isHtml
                ? new TextPart("html") { Text = body }
                : new TextPart("plain") { Text = body };

            using (var client = new SmtpClient())
            {
                try{
                    var smtpServer = configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("SmtpServer");
                    var portString = configuration["EmailSettings:Port"] ?? throw new ArgumentNullException("Port");
                    var useSSLString = configuration["EmailSettings:UseSSL"] ?? throw new ArgumentNullException("UseSSL");

                    int port = int.TryParse(portString, out int parsedPort) ? parsedPort : throw new FormatException("Port must be a valid integer");
                    bool useSSL = bool.TryParse(useSSLString, out bool parsedSSL) ? parsedSSL : throw new FormatException("UseSSL must be a valid boolean");

                    await client.ConnectAsync(smtpServer, port, useSSL).ConfigureAwait(false);


                    await client.AuthenticateAsync(
                        configuration["EmailSettings:FromEmail"], 
                        configuration["EmailSettings:Password"]
                    ).ConfigureAwait(false);
                
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    return new EmailResult { IsSuccess = true };
                }
                catch(Exception ex){
                    return new EmailResult { IsSuccess = false, ErrorMessage = ex.Message };
                }
                finally{
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
        }


    }
}

