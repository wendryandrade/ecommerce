using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Features.Orders.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using MassTransit;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Commands.Handlers
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();

            _handler = new CreateOrderCommandHandler(
                _mockCartRepository.Object,
                _mockProductRepository.Object,
                _mockPublishEndpoint.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldPublishOrderSubmissionEvent_WhenCartIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto(),
                PaymentDetails = new OrderPaymentDto()
            };

            var productInStock = new Product { Id = productId, Name = "Produto Teste", StockQuantity = 10, Price = 100 };
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1, UnitPrice = 100, Product = productInStock } }
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // --- A CORREÇÃO ESTÁ AQUI ---
            // Adicionamos de volta a configuração do mock do produto.
            // Isso é necessário para a verificação de estoque que acontece ANTES de publicar.
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productInStock);
            // -------------------------

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            _mockPublishEndpoint.Verify(p => p.Publish(
                It.IsAny<OrderSubmissionEvent>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        // Deveria lançar uma exceção quando o carrinho está vazio
        public async Task Handle_ShouldThrowException_WhenCartIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreateOrderCommand { UserId = userId };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(new Cart { CartItems = new List<CartItem>() });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("O carrinho está vazio.", exception.Message);
        }

        [Fact]
        // Deveria lançar uma exceção quando um produto está fora de estoque
        public async Task Handle_ShouldThrowException_WhenProductIsOutOfStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new CreateOrderCommand { UserId = userId };

            var productOutOfStock = new Product { Id = productId, StockQuantity = 0 };
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1 } }
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productOutOfStock);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));
        }
    }
}