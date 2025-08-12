using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Infrastructure;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Commands.Handlers
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IShippingService> _mockShippingService; 
        private readonly Mock<IPaymentService> _mockPaymentService;   
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockShippingService = new Mock<IShippingService>(); 
            _mockPaymentService = new Mock<IPaymentService>();  

            _handler = new CreateOrderCommandHandler(
                _mockOrderRepository.Object,
                _mockCartRepository.Object,
                _mockProductRepository.Object,
                _mockShippingService.Object,
                _mockPaymentService.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreateOrder_WhenCartIsNotEmptyAndProductsHaveStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto { Street = "Rua Teste", PostalCode = "12345-678" },
                PaymentDetails = new OrderPaymentDto { PaymentMethod = Domain.Enums.PaymentMethod.CreditCard }
            };

            var productInStock = new Product { Id = productId, Name = "Produto Teste", StockQuantity = 10, Price = 100 };
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1, UnitPrice = 100, Product = productInStock } }
            };

            // "Ensinar" os mocks de repositório 
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productInStock);

            // "Ensinar" os mocks de serviço a retornarem valores de sucesso
            _mockShippingService.Setup(s => s.CalculateShippingCostAsync(It.IsAny<string>(), It.IsAny<string>()))
                                .ReturnsAsync(10.00m); // Retorna um frete de R$10,00

            _mockPaymentService.Setup(p => p.ProcessPaymentAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                               .ReturnsAsync("pi_mock_transaction_id"); // Retorna um ID de transação falso

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);

            // Verificamos se os serviços foram chamados
            _mockShippingService.Verify(s => s.CalculateShippingCostAsync(It.IsAny<string>(), "12345-678"), Times.Once);
            _mockPaymentService.Verify(p => p.ProcessPaymentAsync(110.00m, "brl", It.IsAny<string>()), Times.Once); // 100 (produto) + 10 (frete)

            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
            _mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
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