using Ecommerce.Application.Features.Categories.DTOs;
using Ecommerce.Application.Features.Categories.Queries;
using Ecommerce.Application.Features.Categories.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Categories.Queries.Handlers
{
    public class GetCategoryByIdQueryHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly GetCategoryByIdQueryHandler _handler;

        public GetCategoryByIdQueryHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new GetCategoryByIdQueryHandler(_mockCategoryRepository.Object);
        }

        [Fact]
        // Deveria retornar um CategoryDto quando a categoria existe
        public async Task Handle_ShouldReturnCategoryDto_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var query = new GetCategoryByIdQuery { Id = categoryId };
            var categoryFromRepo = new Category { Id = categoryId, Name = "Eletrônicos", Products = new List<Product>() };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(categoryFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CategoryDto>(result);
            Assert.Equal(categoryId, result.Id);
        }

        [Fact]
        // Deveria retornar nulo quando a categoria não existe
        public async Task Handle_ShouldReturnNull_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var query = new GetCategoryByIdQuery { Id = categoryId };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}