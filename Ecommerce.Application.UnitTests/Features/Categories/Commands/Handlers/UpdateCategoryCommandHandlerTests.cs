using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Categories.Commands.Handlers
{
    public class UpdateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly UpdateCategoryCommandHandler _handler;

        public UpdateCategoryCommandHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new UpdateCategoryCommandHandler(_mockCategoryRepository.Object);
        }

        [Fact]
        // Deveria atualizar uma categoria e retornar verdadeiro quando a categoria existe
        public async Task Handle_ShouldUpdateCategoryAndReturnTrue_WhenCategoryExists()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new UpdateCategoryCommand { Id = categoryId, Name = "Nome Atualizado" };
            var existingCategory = new Category { Id = categoryId, Name = "Nome Antigo" };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCategoryRepository.Verify(repo => repo.UpdateAsync(It.Is<Category>(c => c.Name == "Nome Atualizado")), Times.Once);
        }

        [Fact]
        // Deveria retornar falso quando a categoria não existe
        public async Task Handle_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var command = new UpdateCategoryCommand { Id = Guid.NewGuid() };
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}