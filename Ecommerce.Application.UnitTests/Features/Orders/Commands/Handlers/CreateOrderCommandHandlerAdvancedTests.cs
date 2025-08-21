using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Events;
using Ecommerce.Application.Features.Orders.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using MassTransit;
using Moq;
using Xunit;

namespace Ecommerce.Application.UnitTests.Features.Orders.Commands.Handlers
{
    public class CreateOrderCommandHandlerAdvancedTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerAdvancedTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            _handler = new CreateOrderCommandHandler(
                _mockCartRepository.Object,
                _mockProductRepository.Object,
                _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCartIsNull()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                UserId = Guid.NewGuid(),
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Cart)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            Assert.Equal("O carrinho está vazio.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenCartHasNoItems()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem>() // Empty cart
            };

            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(cart);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            Assert.Equal("O carrinho está vazio.", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenProductIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 50m
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };

            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync((Product)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            Assert.Contains("fora de estoque", exception.Message);
            Assert.Contains($"ID: {productId}", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenProductOutOfStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                StockQuantity = 1 // Less than requested quantity
            };

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = 2, // More than available stock
                UnitPrice = 50m
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };

            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            Assert.Contains("Test Product", exception.Message);
            Assert.Contains("fora de estoque", exception.Message);
        }

        [Fact]
        public async Task Handle_ShouldPublishEventAndClearCart_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                StockQuantity = 10 // Sufficient stock
            };

            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = 2,
                UnitPrice = 50m
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem> { cartItem }
            };

            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _mockCartRepository.Setup(x => x.UpdateAsync(It.IsAny<Cart>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            // Verify event was published
            _mockPublishEndpoint.Verify(x => x.Publish(
                It.IsAny<OrderSubmissionEvent>(),
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify cart was updated
            _mockCartRepository.Verify(x => x.UpdateAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldValidateAllCartItems_WhenMultipleProductsInCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            
            var product1 = new Product
            {
                Id = productId1,
                Name = "Product 1",
                StockQuantity = 5
            };

            var product2 = new Product
            {
                Id = productId2,
                Name = "Product 2",
                StockQuantity = 1 // This will cause out of stock
            };

            var cartItem1 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId1,
                Quantity = 2,
                UnitPrice = 50m
            };

            var cartItem2 = new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId2,
                Quantity = 3, // More than available
                UnitPrice = 30m
            };

            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CartItems = new List<CartItem> { cartItem1, cartItem2 }
            };

            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto
                {
                    Street = "Test Street",
                    City = "Test City",
                    State = "TS",
                    PostalCode = "12345"
                },
                PaymentDetails = new OrderPaymentDto
                {
                    PaymentMethod = Domain.Enums.PaymentMethod.CreditCard
                }
            };

            _mockCartRepository.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(cart);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId1))
                .ReturnsAsync(product1);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId2))
                .ReturnsAsync(product2);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
            
            Assert.Contains("Product 2", exception.Message);
            Assert.Contains("fora de estoque", exception.Message);

            // Verify all products were checked
            _mockProductRepository.Verify(x => x.GetByIdAsync(productId1), Times.Once);
            _mockProductRepository.Verify(x => x.GetByIdAsync(productId2), Times.Once);
        }
    }
}
