using Backend.Model.Email;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class EmailService
    {
        public EmailSettings _emailSettings; 

        public EmailService(IOptions<EmailSettings> options) 
        { 
            _emailSettings = options.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtp.SendMailAsync(mailMessage);
        }
    }
}
