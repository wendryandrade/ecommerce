using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Dtos;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Application.Products.Queries.Handlers;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Products.Queries.Handlers
{
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly GetProductByIdQueryHandler _handler;

        public GetProductByIdQueryHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new GetProductByIdQueryHandler(_mockProductRepository.Object);
        }

        [Fact]
        // Deveria retornar um ProductDto quando o produto existe
        public async Task Handle_ShouldReturnProductDto_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);
            var productFromRepo = new Product { Id = productId, Name = "Produto Teste", Category = new Category { Name = "Categoria Teste" } };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(productFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductDto>(result);
            Assert.Equal(productId, result.Id);
        }

        [Fact]
        // Deveria retornar null quando o produto não existe
        public async Task Handle_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var query = new GetProductByIdQuery(productId);

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}