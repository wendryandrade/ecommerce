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

        [Fact]
        public async Task Handle_ShouldCreateCategoryWithAllProperties_WhenCommandIsComplete()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Eletrônicos",
                Description = "Produtos eletrônicos e tecnologia"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => 
                c.Name == command.Name && 
                c.Description == command.Description), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateCategoryWithEmptyDescription_WhenDescriptionIsEmpty()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Categoria Sem Descrição",
                Description = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => 
                c.Name == command.Name && 
                c.Description == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateCategoryWithEmptyName_WhenNameIsEmpty()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "",
                Description = "Descrição da categoria"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => 
                c.Name == "" && 
                c.Description == command.Description), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateCategoryWithNullDescription_WhenDescriptionIsNull()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Categoria Com Descrição Nula",
                Description = ""
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => 
                c.Name == command.Name && 
                c.Description == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateCategoryWithNullName_WhenNameIsNull()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "",
                Description = "Descrição da categoria"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.Is<Category>(c => 
                c.Name == "" && 
                c.Description == command.Description), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCall_WhenCategoryIsCreated()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Categoria Teste",
                Description = "Descrição Teste"
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockCategoryRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnUniqueGuid_WhenCategoryIsCreated()
        {
            // Arrange
            var command = new CreateCategoryCommand
            {
                Name = "Categoria Única",
                Description = "Descrição Única"
            };

            // Act
            var result1 = await _handler.Handle(command, CancellationToken.None);
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result1);
            Assert.NotEqual(Guid.Empty, result2);
            Assert.NotEqual(result1, result2); // Each call should generate a unique GUID
        }
    }
}