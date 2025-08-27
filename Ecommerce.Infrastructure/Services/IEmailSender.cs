namespace Ecommerce.Infrastructure.Services
{
    public interface IEmailSender
    {
        Task SendAsync(Models.EmailSettings settings, string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
