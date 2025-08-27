using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
using MassTransit;

namespace Ecommerce.API.Consumers
{
    public class EmailConsumer : IConsumer<OrderProcessedEvent>
    {
        private readonly ILogger<EmailConsumer> _logger;
        private readonly IEmailService _emailService;

        public EmailConsumer(ILogger<EmailConsumer> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task Consume(ConsumeContext<OrderProcessedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Recebido OrderProcessedEvent para OrderId {OrderId}", msg.OrderId);
            await _emailService.SendOrderConfirmationAsync(msg.UserId, msg.OrderId, msg.TotalAmount, context.CancellationToken);
            _logger.LogInformation("E-mail de confirmação enviado para UserId {UserId} do pedido {OrderId}", msg.UserId, msg.OrderId);
        }
    }
}
