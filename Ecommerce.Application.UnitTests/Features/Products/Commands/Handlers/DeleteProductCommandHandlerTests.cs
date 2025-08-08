using Ecommerce.Application.Features.Products.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Domain.Entities;
using Moq;

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
        // Deveria deletar um produto e retornar verdadeiro quando o produto existe
        public async Task Handle_ShouldDeleteProductAndReturnTrue_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new DeleteProductCommand(productId);
            var existingProduct = new Product { Id = productId };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(existingProduct), Times.Once);
        }
    }
}