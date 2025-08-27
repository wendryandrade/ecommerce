using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Services
{
    using Ecommerce.Application.Interfaces;

    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendOrderConfirmationAsync(Guid userId, Guid orderId, decimal totalAmount, CancellationToken cancellationToken = default)
        {
            // Implementação simples que apenas loga. Em produção, integrar com um provider (SMTP, SendGrid, etc.).
            _logger.LogInformation("[EmailService] Enviando confirmação de pedido. UserId: {UserId}, OrderId: {OrderId}, Total: {Total}", userId, orderId, totalAmount);
            return Task.CompletedTask;
        }
    }
}
