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
    }
}