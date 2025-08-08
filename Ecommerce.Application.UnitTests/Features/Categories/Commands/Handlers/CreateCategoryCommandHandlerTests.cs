using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Categories.Commands.Handlers
{
    public class CreateCategoryCommandHandlerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CreateCategoryCommandHandler _handler;

        public CreateCategoryCommandHandlerTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _handler = new CreateCategoryCommandHandler(_mockCategoryRepository.Object);
        }

        [Fact]
        // Deveria criar uma categoria e retornar o ID da categoria criada
        public async Task Handle_ShouldCreateCategoryAndReturnCategoryId()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Nova Categoria",
                Description = "Descrição da Categoria"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}