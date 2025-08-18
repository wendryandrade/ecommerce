using Ecommerce.API.Consumers;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
public class OrderConsumerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IShippingService> _mockShippingService;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly OrderConsumer _consumer;

    public OrderConsumerTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockShippingService = new Mock<IShippingService>();
        _mockPaymentService = new Mock<IPaymentService>();
        var mockLogger = new Mock<ILogger<OrderConsumer>>();

        _consumer = new OrderConsumer(
            mockLogger.Object,
            _mockOrderRepository.Object,
            _mockProductRepository.Object,
            _mockShippingService.Object,
            _mockPaymentService.Object
        );
    }

    [Fact]
    public async Task Consume_ShouldProcessOrderAndSaveChanges_WhenEventIsValid()
    {
        // Arrange
        var orderEvent = new OrderSubmissionEvent
        {
            OrderId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CartItems = new List<EventCartItem> { new EventCartItem { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100 } },
            ShippingAddress = new(),
            PaymentDetails = new()
        };

        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product { StockQuantity = 10 });
        _mockShippingService.Setup(s => s.CalculateShippingCostAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20.0m);
        _mockPaymentService.Setup(p => p.ProcessPaymentAsync(It.IsAny<decimal>(), "brl", It.IsAny<string>())).ReturnsAsync("pi_mock_123");

        var consumeContext = new Mock<ConsumeContext<OrderSubmissionEvent>>();
        consumeContext.Setup(c => c.Message).Returns(orderEvent);

        // Act
        await _consumer.Consume(consumeContext.Object);

        // Assert
        // Verifica se a lógica principal (salvar o pedido) foi chamada.
        _mockOrderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockProductRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once); // Verifica a baixa de estoque
    }
}