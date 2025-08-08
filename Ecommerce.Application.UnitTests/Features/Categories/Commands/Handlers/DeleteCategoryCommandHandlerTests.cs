using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Categories.Commands.Handlers
{
    public class DeleteCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly DeleteCategoryCommandHandler _handler;

        public DeleteCategoryCommandHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object);
        }

        [Fact]
        // Deveria excluir uma categoria quando ela existe e não tem produtos associados
        public async Task Handle_ShouldDeleteCategory_WhenCategoryExistsAndHasNoProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };
            var category = new Category { Id = categoryId, Name = "Categoria Vazia", Products = new List<Product>() };

            // "Ensina" o mock a encontrar a categoria
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            // Verifica se o método DeleteAsync foi chamado exatamente uma vez.
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(category), Times.Once);
        }

        [Fact]
        // Deveria lançar uma exceção quando a categoria tem produtos associados
        public async Task Handle_ShouldThrowInvalidOperationException_WhenCategoryHasProducts()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };
            // Categoria com um produto associado
            var categoryWithProducts = new Category
            {
                Id = categoryId,
                Name = "Categoria Com Produtos",
                Products = new List<Product> { new Product() }
            };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(categoryWithProducts);

            // Act & Assert
            // Verifica se a exceção correta é lançada
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Não é possível excluir uma categoria com produtos vinculados.", exception.Message);
        }

        [Fact]
        // Deveria retornar falso quando a categoria não existe
        public async Task Handle_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };

            // "Ensina" o mock a não encontrar a categoria
            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync((Category)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}