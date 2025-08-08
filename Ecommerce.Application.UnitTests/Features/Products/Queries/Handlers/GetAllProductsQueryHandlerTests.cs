using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Dtos;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Application.Products.Queries.Handlers;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Products.Queries.Handlers
{
    public class GetAllProductsQueryHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly GetAllProductsQueryHandler _handler;

        public GetAllProductsQueryHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new GetAllProductsQueryHandler(_mockProductRepository.Object);
        }

        [Fact]
        // Deveria retornar uma lista de ProductDtos quando produtos existirem
        public async Task Handle_ShouldReturnListOfProductDtos_WhenProductsExist()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Produto A", Category = new Category { Name = "Categoria 1" } },
                new Product { Id = Guid.NewGuid(), Name = "Produto B", Category = new Category { Name = "Categoria 1" } }
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(productsFromRepo);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ProductDto>>(result);
            Assert.Equal(2, result.Count);
        }
    }
}