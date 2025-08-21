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

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoProductsExist()
        {
            // Arrange
            var emptyProductsList = new List<Product>();

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emptyProductsList);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<ProductDto>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldMapAllProductProperties_WhenProductsExist()
        {
            // Arrange
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var productsFromRepo = new List<Product>
            {
                new Product 
                { 
                    Id = productId1, 
                    Name = "Produto A", 
                    Description = "Descrição A",
                    Price = 100.50m,
                    StockQuantity = 10,
                    Category = new Category { Id = categoryId, Name = "Categoria 1" }
                },
                new Product 
                { 
                    Id = productId2, 
                    Name = "Produto B", 
                    Description = "Descrição B",
                    Price = 200.75m,
                    StockQuantity = 5,
                    Category = new Category { Id = categoryId, Name = "Categoria 1" }
                }
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(productsFromRepo);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            // Verify first product mapping
            var firstProduct = result[0];
            Assert.Equal(productId1, firstProduct.Id);
            Assert.Equal("Produto A", firstProduct.Name);
            Assert.Equal("Descrição A", firstProduct.Description);
            Assert.Equal(100.50m, firstProduct.Price);
            Assert.Equal(10, firstProduct.StockQuantity);
            Assert.Equal("Categoria 1", firstProduct.CategoryName);

            // Verify second product mapping
            var secondProduct = result[1];
            Assert.Equal(productId2, secondProduct.Id);
            Assert.Equal("Produto B", secondProduct.Name);
            Assert.Equal("Descrição B", secondProduct.Description);
            Assert.Equal(200.75m, secondProduct.Price);
            Assert.Equal(5, secondProduct.StockQuantity);
            Assert.Equal("Categoria 1", secondProduct.CategoryName);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyCategoryName_WhenProductHasEmptyCategory()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Produto Sem Categoria", 
                    Description = "Descrição",
                    Price = 50.00m,
                    StockQuantity = 1,
                    Category = new Category { Id = Guid.NewGuid(), Name = "", Description = "" }
                }
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(productsFromRepo);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("", result[0].CategoryName);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCall_WhenQueryIsExecuted()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Produto Teste", Category = new Category { Name = "Categoria" } }
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(productsFromRepo);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _mockProductRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldHandleMixedProducts_WithAndWithoutCategoryNames()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Produto Com Categoria", 
                    Category = new Category { Name = "Categoria 1" }
                },
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Produto Sem Nome de Categoria", 
                    Category = new Category { Id = Guid.NewGuid(), Name = "", Description = "" }
                },
                new Product 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Outro Produto Com Categoria", 
                    Category = new Category { Name = "Categoria 2" }
                }
            };

            _mockProductRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(productsFromRepo);

            var query = new GetAllProductsQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Categoria 1", result[0].CategoryName);
            Assert.Equal("", result[1].CategoryName);
            Assert.Equal("Categoria 2", result[2].CategoryName);
        }
    }
}