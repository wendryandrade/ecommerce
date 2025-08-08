using Ecommerce.Application.Features.Products.Commands;
using Ecommerce.Application.Features.Products.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.Dtos;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Products.Commands.Handlers
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly UpdateProductCommandHandler _handler;

        public UpdateProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new UpdateProductCommandHandler(_mockProductRepository.Object);
        }

        [Fact]
        // Deveria atualizar um produto e retornar o DTO do produto atualizado quando o produto existe
        public async Task Handle_ShouldUpdateProductAndReturnDto_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Nome Atualizado" };
            var existingProduct = new Product { Id = productId, Name = "Nome Antigo" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductDto>(result);
            Assert.Equal("Nome Atualizado", result.Name);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }
    }
}