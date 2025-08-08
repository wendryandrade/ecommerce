using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Orders.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Queries.Handlers
{
    public class GetOrdersByUserIdQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly GetOrdersByUserIdQueryHandler _handler;

        public GetOrdersByUserIdQueryHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _handler = new GetOrdersByUserIdQueryHandler(_mockOrderRepository.Object);
        }

        [Fact]
        // Deveria retornar uma lista de OrderDtos quando o usuário tem pedidos
        public async Task Handle_ShouldReturnListOfOrderDtos_WhenUserHasOrders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetOrdersByUserIdQuery(userId);

            var ordersFromRepo = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), UserId = userId, OrderItems = new List<OrderItem>() },
                new Order { Id = Guid.NewGuid(), UserId = userId, OrderItems = new List<OrderItem>() }
            };

            _mockOrderRepository.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(ordersFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<OrderDto>>(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        // Deveria retornar uma lista vazia quando o usuário não tem pedidos
        public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoOrders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetOrdersByUserIdQuery(userId);

            _mockOrderRepository.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new List<Order>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}