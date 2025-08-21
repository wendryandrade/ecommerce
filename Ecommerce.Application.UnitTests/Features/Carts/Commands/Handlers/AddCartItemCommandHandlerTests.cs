using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Carts.Commands.Handlers
{
    public class AddCartItemCommandHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly AddCartItemCommandHandler _handler;

        public AddCartItemCommandHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new AddCartItemCommandHandler(_mockCartRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        // Deveria adicionar um item ao novo carrinho quando o carrinho não existe
        public async Task Handle_ShouldAddItemToNewCart_WhenCartDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new AddCartItemCommand { UserId = userId, ProductId = productId, Quantity = 1 };
            var product = new Product { Id = productId, StockQuantity = 10 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.AddAsync(It.Is<Cart>(c => c.UserId == userId)), Times.Once);
        }

        [Fact]
        // Deveria retornar falso quando o produto não tem estoque suficiente
        public async Task Handle_ShouldReturnFalse_WhenProductIsOutOfStock()
        {
            // Arrange
            var command = new AddCartItemCommand { ProductId = Guid.NewGuid(), Quantity = 5 };
            var product = new Product { Id = command.ProductId, StockQuantity = 4 }; // Estoque insuficiente

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync(product);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            var command = new AddCartItemCommand { ProductId = Guid.NewGuid(), Quantity = 1 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockCartRepository.Verify(repo => repo.GetByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldAddItemToExistingCart_WhenCartExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var command = new AddCartItemCommand { UserId = userId, ProductId = productId, Quantity = 2 };
            var product = new Product { Id = productId, StockQuantity = 10 };
            var existingCart = new Cart { Id = Guid.NewGuid(), UserId = userId, CartItems = new List<CartItem>() };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(existingCart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.UpdateAsync(existingCart), Times.Once);
            _mockCartRepository.Verify(repo => repo.AddAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenQuantityIsZero()
        {
            // Arrange
            var command = new AddCartItemCommand { ProductId = Guid.NewGuid(), Quantity = 0 };
            var product = new Product { Id = command.ProductId, StockQuantity = 10 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenQuantityIsNegative()
        {
            // Arrange
            var command = new AddCartItemCommand { ProductId = Guid.NewGuid(), Quantity = -1 };
            var product = new Product { Id = command.ProductId, StockQuantity = 10 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync(product);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenQuantityEqualsStock()
        {
            // Arrange
            var command = new AddCartItemCommand { ProductId = Guid.NewGuid(), Quantity = 5 };
            var product = new Product { Id = command.ProductId, StockQuantity = 5 }; // Quantidade igual ao estoque

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync(product);
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(command.UserId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.AddAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldVerifyProductRepositoryCalled_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new AddCartItemCommand { ProductId = productId, Quantity = 1 };
            var product = new Product { Id = productId, StockQuantity = 10 };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(product);
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(command.UserId)).ReturnsAsync((Cart?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldVerifyCartRepositoryCalled_WhenCartExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new AddCartItemCommand { UserId = userId, ProductId = Guid.NewGuid(), Quantity = 1 };
            var product = new Product { Id = command.ProductId, StockQuantity = 10 };
            var existingCart = new Cart { Id = Guid.NewGuid(), UserId = userId };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(command.ProductId)).ReturnsAsync(product);
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(existingCart);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCartRepository.Verify(repo => repo.GetByUserIdAsync(userId), Times.Once);
        }
    }
}