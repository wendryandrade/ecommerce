using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Orders.Commands.Handlers
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            // Agora temos 3 dependências para "mockar"
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();

            _handler = new CreateOrderCommandHandler(
                _mockOrderRepository.Object,
                _mockCartRepository.Object,
                _mockProductRepository.Object
            );
        }

        [Fact]
        // Deveria criar uma ordem com sucesso quando o carrinho não está vazio e os produtos tem estoque suficiente
        public async Task Handle_ShouldCreateOrder_WhenCartIsNotEmptyAndProductsHaveStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new CreateOrderCommand
            {
                UserId = userId,
                ShippingAddress = new OrderAddressDto { Street = "Rua Teste" },
                PaymentDetails = new OrderPaymentDto { PaymentMethod = Domain.Enums.PaymentMethod.CreditCard }
            };

            var productInStock = new Product { Id = productId, Name = "Produto Teste", StockQuantity = 10, Price = 100 };
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1, UnitPrice = 100, Product = productInStock } }
            };

            // "Ensinar" os mocks
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productInStock);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result); // Verifica se um ID de pedido foi retornado

            // Verifica se os métodos corretos foram chamados o número certo de vezes
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once); // Verificando a baixa de stock
            _mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once); // Verificando se o pedido foi salvo
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Once); // Verificando se o carrinho foi atualizado (limpo)
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