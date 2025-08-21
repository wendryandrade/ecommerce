using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;
using Xunit;
using Ecommerce.Application.Features.Products.Commands.Handlers;

namespace Ecommerce.Application.UnitTests.Features.Products.Commands.Handlers
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly DeleteProductCommandHandler _handler;

        public DeleteProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new DeleteProductCommandHandler(_mockProductRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenProductExistsAndIsDeleted()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var productFromRepo = new Product { Id = productId, Name = "Produto Teste" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productFromRepo);
            _mockProductRepository.Setup(repo => repo.DeleteAsync(productFromRepo)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(productFromRepo), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenDeleteFails()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var productFromRepo = new Product { Id = productId, Name = "Produto Teste" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productFromRepo);
            _mockProductRepository.Setup(repo => repo.DeleteAsync(productFromRepo)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result); // The handler returns true if product exists and delete is called
            _mockProductRepository.Verify(repo => repo.DeleteAsync(productFromRepo), Times.Once);
        }
    }
}