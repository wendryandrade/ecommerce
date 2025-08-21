using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Commands.Handlers
{
    public class UpdateOrderStatusCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly UpdateOrderStatusCommandHandler _handler;

        public UpdateOrderStatusCommandHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new UpdateOrderStatusCommandHandler(_mockOrderRepository.Object);
        }

        [Fact]
        // Deveria atualizar o status e retornar verdadeiro quando o pedido existe
        public async Task Handle_ShouldUpdateStatusAndReturnTrue_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new UpdateOrderStatusCommand { OrderId = orderId, NewStatus = OrderStatus.Paid };
            var existingOrder = new Order { Id = orderId, Status = OrderStatus.Pending };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(existingOrder);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.Is<Order>(o => o.Status == OrderStatus.Paid)), Times.Once);
        }

        [Fact]
        // Deveria retornar falso quando o pedido não existe
        public async Task Handle_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new UpdateOrderStatusCommand { OrderId = orderId, NewStatus = OrderStatus.Paid };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockOrderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never);
        }
    }
}