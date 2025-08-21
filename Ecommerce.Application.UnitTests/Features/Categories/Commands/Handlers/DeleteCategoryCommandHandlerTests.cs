using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;
using Xunit;

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
        public async Task Handle_ShouldReturnTrue_WhenCategoryExistsAndIsDeleted()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };
            var categoryFromRepo = new Category { Id = categoryId, Name = "Categoria Teste" };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(categoryFromRepo);
            _mockCategoryRepository.Setup(repo => repo.DeleteAsync(categoryFromRepo)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryFromRepo), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenDeleteSucceeds()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new DeleteCategoryCommand { Id = categoryId };
            var categoryFromRepo = new Category { Id = categoryId, Name = "Categoria Teste" };

            _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(categoryFromRepo);
            _mockCategoryRepository.Setup(repo => repo.DeleteAsync(categoryFromRepo)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockCategoryRepository.Verify(repo => repo.DeleteAsync(categoryFromRepo), Times.Once);
        }
    }
}