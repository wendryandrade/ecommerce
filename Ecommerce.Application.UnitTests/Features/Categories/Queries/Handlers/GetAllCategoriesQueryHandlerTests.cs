using Ecommerce.Application.Features.Categories.DTOs;
using Ecommerce.Application.Features.Categories.Queries;
using Ecommerce.Application.Features.Categories.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Categories.Queries.Handlers
{
    public class GetAllCategoriesQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly GetAllCategoriesQueryHandler _handler;

        public GetAllCategoriesQueryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new GetAllCategoriesQueryHandler(_mockCategoryRepository.Object);
        }

        [Fact]
        // Deveria retornar uma lista de CategoryDtos quando categorias existirem
        public async Task Handle_ShouldReturnListOfCategoryDtos_WhenCategoriesExist()
        {
            // Arrange
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Eletrônicos", Products = new List<Product> { new Product { Name = "Smartphone" } } },
                new Category { Id = Guid.NewGuid(), Name = "Roupas", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CategoryDto>>(result);
            Assert.Equal(2, result.Count);
            Assert.Single(result[0].Products); // A primeira categoria deve ter 1 produto
            Assert.Empty(result[1].Products);    // A segunda categoria deve ter 0 produtos
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoCategoriesExist()
        {
            // Arrange
            var emptyCategoriesList = new List<Category>();
            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(emptyCategoriesList);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CategoryDto>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldMapAllCategoryProperties_WhenCategoriesExist()
        {
            // Arrange
            var categoryId1 = Guid.NewGuid();
            var categoryId2 = Guid.NewGuid();
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = categoryId1, Name = "Eletrônicos", Description = "Produtos eletrônicos", Products = new List<Product>() },
                new Category { Id = categoryId2, Name = "Roupas", Description = "Vestuário", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(categoryId1, result[0].Id);
            Assert.Equal("Eletrônicos", result[0].Name);
            Assert.Equal("Produtos eletrônicos", result[0].Description);
            Assert.Equal(categoryId2, result[1].Id);
            Assert.Equal("Roupas", result[1].Name);
            Assert.Equal("Vestuário", result[1].Description);
        }

        [Fact]
        public async Task Handle_ShouldHandleCategoriesWithEmptyProperties_WhenCategoriesExist()
        {
            // Arrange
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "", Description = "", Products = new List<Product>() },
                new Category { Id = Guid.NewGuid(), Name = "", Description = "", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("", result[0].Name);
            Assert.Equal("", result[0].Description);
            Assert.Equal("", result[1].Name);
            Assert.Equal("", result[1].Description);
        }

        [Fact]
        public async Task Handle_ShouldHandleCategoriesWithNullProducts_WhenCategoriesExist()
        {
            // Arrange
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Categoria 1", Products = new List<Product>() },
                new Category { Id = Guid.NewGuid(), Name = "Categoria 2", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Empty(result[0].Products); // Should handle null products gracefully
            Assert.Empty(result[1].Products);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCall_WhenCategoriesExist()
        {
            // Arrange
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Categoria Teste", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _mockCategoryRepository.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectType_WhenCategoriesExist()
        {
            // Arrange
            var categoriesFromRepo = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Categoria Teste", Products = new List<Product>() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categoriesFromRepo);

            var query = new GetAllCategoriesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<CategoryDto>>(result);
            Assert.IsAssignableFrom<List<CategoryDto>>(result);
        }
    }
}