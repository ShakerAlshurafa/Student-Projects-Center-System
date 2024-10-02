using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
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
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) throw new ArgumentException("Recipient email address cannot be empty.");
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject cannot be empty.");
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message body cannot be empty.");


            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Student Projects Center", configuration["EmailSettings:FromEmail"]));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message };

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
                }
                catch(Exception ex){
                    throw new InvalidOperationException("Error sending email", ex);
                }
                finally{
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
            }
        }
    }
}

