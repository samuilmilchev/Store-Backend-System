using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace WebApp1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailConfirmation(string userEmail, string confirmationLink)
        {
            var email = _configuration["EmailSettings:Email"];
            var password = _configuration["EmailSettings:AppPassword"];

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(email, password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(email),
                    Subject = "Confirm your email",
                    Body = $"Please confirm your email by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(userEmail);

                await client.SendMailAsync(mailMessage);

            }
        }
    }
}
