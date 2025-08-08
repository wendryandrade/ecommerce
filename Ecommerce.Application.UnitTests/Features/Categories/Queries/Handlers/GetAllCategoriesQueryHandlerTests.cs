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
    }
}