using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ProyetoSetilPF.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Reemplazar URL base generada por Identity
            var baseUrl = _configuration["Identity:ServerUrl"];
            message = message.Replace("https://localhost", baseUrl);

            // Leer configuración del appsettings.json
            var host = _configuration["Smtp:Host"];
            var port = int.Parse(_configuration["Smtp:Port"]);
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];

            var client = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(user),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}