using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Carts.Commands.Handlers
{
    public class RemoveCartItemCommandHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly RemoveCartItemCommandHandler _handler;

        public RemoveCartItemCommandHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _handler = new RemoveCartItemCommandHandler(_mockCartRepository.Object);
        }

        [Fact]
        // Deveria remover um item e atualizar o carrinho quando o carrinho existe
        public async Task Handle_ShouldRemoveItemAndUpdateCart_WhenCartExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1 } }
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.Is<Cart>(c => c.CartItems.Count == 0)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenCartIsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenItemIsNotFoundInCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = Guid.NewGuid(), Quantity = 1 } } // Different product
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRemoveSpecificItem_WhenMultipleItemsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId1);
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> 
                { 
                    new CartItem { ProductId = productId1, Quantity = 2 },
                    new CartItem { ProductId = productId2, Quantity = 1 }
                }
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Single(cart.CartItems);
            Assert.Equal(productId2, cart.CartItems.First().ProductId);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenCartIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>() // Empty cart
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCalls_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem> { new CartItem { ProductId = productId, Quantity = 1 } }
            };

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(cart), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCallUpdate_WhenCartIsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new RemoveCartItemCommand(userId, productId);

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Cart>()), Times.Never);
        }
    }
}