using Ecommerce.Application.Features.Products.Commands;
using Ecommerce.Application.Features.Products.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.Dtos;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Products.Commands.Handlers
{
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly UpdateProductCommandHandler _handler;

        public UpdateProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new UpdateProductCommandHandler(_mockProductRepository.Object);
        }

        [Fact]
        // Deveria atualizar um produto e retornar o DTO do produto atualizado quando o produto existe
        public async Task Handle_ShouldUpdateProductAndReturnDto_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Nome Atualizado" };
            var existingProduct = new Product { Id = productId, Name = "Nome Antigo" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ProductDto>(result);
            Assert.Equal("Nome Atualizado", result.Name);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Nome Atualizado" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldUpdateAllProductProperties_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var command = new UpdateProductCommand 
            { 
                Id = productId, 
                Name = "Nome Atualizado",
                Description = "Descrição Atualizada",
                Price = 150.50m,
                StockQuantity = 25,
                CategoryId = categoryId
            };
            var existingProduct = new Product 
            { 
                Id = productId, 
                Name = "Nome Antigo",
                Description = "Descrição Antiga",
                Price = 100.00m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Name, result.Name);
            Assert.Equal(command.Description, result.Description);
            Assert.Equal(command.Price, result.Price);
            Assert.Equal(command.StockQuantity, result.StockQuantity);
            Assert.Equal(command.CategoryId, existingProduct.CategoryId);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => 
                p.Name == command.Name && 
                p.Description == command.Description && 
                p.Price == command.Price && 
                p.StockQuantity == command.StockQuantity && 
                p.CategoryId == command.CategoryId)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnProductDtoWithCorrectMapping_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var command = new UpdateProductCommand 
            { 
                Id = productId, 
                Name = "Produto Teste",
                Description = "Descrição Teste",
                Price = 99.99m,
                StockQuantity = 50,
                CategoryId = categoryId
            };
            var existingProduct = new Product 
            { 
                Id = productId, 
                Name = "Nome Antigo",
                Description = "Descrição Antiga",
                Price = 50.00m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid(),
                Category = new Category { Name = "Categoria Teste" }
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal(command.Name, result.Name);
            Assert.Equal(command.Description, result.Description);
            Assert.Equal(command.Price, result.Price);
            Assert.Equal(command.StockQuantity, result.StockQuantity);
            Assert.Equal("Categoria Teste", result.CategoryName);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyCategoryName_WhenProductHasEmptyCategory()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Produto Teste" };
            var existingProduct = new Product 
            { 
                Id = productId, 
                Name = "Nome Antigo",
                Category = new Category { Id = Guid.NewGuid(), Name = "", Description = "" }
            };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.CategoryName);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCalls_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Nome Atualizado" };
            var existingProduct = new Product { Id = productId, Name = "Nome Antigo" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(existingProduct);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldNotCallUpdate_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var command = new UpdateProductCommand { Id = productId, Name = "Nome Atualizado" };

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}