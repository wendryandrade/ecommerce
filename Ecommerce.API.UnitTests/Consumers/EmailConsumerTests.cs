using Ecommerce.API.Consumers;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Consumers
{
    public class EmailConsumerTests
    {
        [Fact]
        public async Task Consume_ShouldCallEmailService_WithCorrectParameters()
        {
            // Arrange
            var logger = new Mock<ILogger<EmailConsumer>>();
            var emailService = new Mock<IEmailService>();
            var consumer = new EmailConsumer(logger.Object, emailService.Object);

            var evt = new OrderProcessedEvent
            {
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TotalAmount = 123.45m
            };

            var context = new Mock<ConsumeContext<OrderProcessedEvent>>();
            context.Setup(c => c.Message).Returns(evt);
            context.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            // Act
            await consumer.Consume(context.Object);

            // Assert
            emailService.Verify(s => s.SendOrderConfirmationAsync(evt.UserId, evt.OrderId, evt.TotalAmount, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
