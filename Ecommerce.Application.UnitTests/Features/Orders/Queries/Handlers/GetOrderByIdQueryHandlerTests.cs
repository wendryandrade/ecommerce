using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Orders.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Queries.Handlers
{
    public class GetOrderByIdQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new GetOrderByIdQueryHandler(_mockOrderRepository.Object);
        }

        [Fact]
        // Deveria retornar um OrderDto quando o pedido existe
        public async Task Handle_ShouldReturnOrderDto_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var query = new GetOrderByIdQuery { Id = orderId };

            var orderFromRepo = new Order
            {
                Id = orderId,
                UserId = Guid.NewGuid(),
                TotalAmount = 150.50m,
                Payment = new Payment { TransactionId = "trans_123" },
                ShippingAddress = new Address { Street = "Rua Teste" },
                OrderItems = new List<OrderItem>()
            };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(orderFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderDto>(result);
            Assert.Equal(orderId, result.Id);
            Assert.NotNull(result.Payment); // Verifica se o pagamento foi mapeado
            Assert.NotNull(result.ShippingAddress); // Verifica se o endereço foi mapeado
        }

        [Fact]
        // Deveria retornar nulo quando o pedido não existe
        public async Task Handle_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var query = new GetOrderByIdQuery { Id = orderId };

            _mockOrderRepository.Setup(repo => repo.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync((Order?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}