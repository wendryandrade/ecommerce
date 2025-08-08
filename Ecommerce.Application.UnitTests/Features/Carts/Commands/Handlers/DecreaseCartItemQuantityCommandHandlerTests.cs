using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Carts.Commands.Handlers
{
    public class DecreaseCartItemQuantityCommandHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly DecreaseCartItemQuantityCommandHandler _handler;

        public DecreaseCartItemQuantityCommandHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _handler = new DecreaseCartItemQuantityCommandHandler(_mockCartRepository.Object);
        }

        [Fact]
        // Deveria diminuir a quantidade quando a quantidade do item for maior que um
        public async Task Handle_ShouldDecreaseQuantity_WhenItemQuantityIsGreaterThanOne()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new DecreaseCartItemQuantityCommand { UserId = userId, ProductId = productId };

            var cartItem = new CartItem { ProductId = productId, Quantity = 5 };
            var cart = new Cart { UserId = userId, CartItems = new List<CartItem> { cartItem } };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(4, cartItem.Quantity); // Verifica se a quantidade foi diminuída
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once); // Verifica se o carrinho foi atualizado
        }

        [Fact]
        // Deveria remover o item quando a quantidade do item for um
        public async Task Handle_ShouldRemoveItem_WhenItemQuantityIsOne()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new DecreaseCartItemQuantityCommand { UserId = userId, ProductId = productId };

            var cartItem = new CartItem { ProductId = productId, Quantity = 1 };
            var cart = new Cart { UserId = userId, CartItems = new List<CartItem> { cartItem } };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Empty(cart.CartItems); // Verifica se a lista de itens do carrinho está vazia
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once);
        }

        [Fact]
        // Deveria retornar falso quando o carrinho não for encontrado
        public async Task Handle_ShouldReturnFalse_WhenCartIsNotFound()
        {
            // Arrange
            var command = new DecreaseCartItemQuantityCommand { UserId = Guid.NewGuid(), ProductId = Guid.NewGuid() };

            // "Ensina" o mock a retornar nulo, simulando que o carrinho não foi encontrado
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Cart)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        // Deveria retornar falso quando o item não for encontrado no carrinho
        public async Task Handle_ShouldReturnFalse_WhenItemIsNotFoundInCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new DecreaseCartItemQuantityCommand { UserId = userId, ProductId = Guid.NewGuid() }; // Produto não está no carrinho
            var cart = new Cart { UserId = userId, CartItems = new List<CartItem>() };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}