namespace WebApp1.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmation(string userEmail, string confirmationLink);
    }
}
