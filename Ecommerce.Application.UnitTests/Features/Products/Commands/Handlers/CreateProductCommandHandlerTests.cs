using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Features.Products.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;
using Xunit;

namespace Ecommerce.Application.UnitTests.Features.Products.Commands.Handlers
{
    public class CreateProductCommandHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly CreateProductCommandHandler _handler;

        public CreateProductCommandHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _handler = new CreateProductCommandHandler(_mockProductRepository.Object);
        }

        [Fact]
        // Deveria criar um produto e retornar o ID do produto criado
        public async Task Handle_ShouldCreateProductAndReturnProductId()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Novo Produto",
                Description = "Descrição do Produto",
                Price = 100,
                StockQuantity = 50,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithAllProperties_WhenCommandIsComplete()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Smartphone",
                Description = "Smartphone de última geração",
                Price = 999.99m,
                StockQuantity = 25,
                CategoryId = categoryId
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => 
                p.Name == command.Name && 
                p.Description == command.Description &&
                p.Price == command.Price &&
                p.StockQuantity == command.StockQuantity &&
                p.CategoryId == command.CategoryId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithZeroPrice_WhenPriceIsZero()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Grátis",
                Description = "Produto gratuito",
                Price = 0m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Price == 0m), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithZeroStock_WhenStockQuantityIsZero()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Sem Estoque",
                Description = "Produto sem estoque",
                Price = 50m,
                StockQuantity = 0,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.StockQuantity == 0), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithNegativeStock_WhenStockQuantityIsNegative()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Com Estoque Negativo",
                Description = "Produto com estoque negativo",
                Price = 100m,
                StockQuantity = -5,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.StockQuantity == -5), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithEmptyName_WhenNameIsEmpty()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "",
                Description = "Produto sem nome",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Name == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithNullName_WhenNameIsNull()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "",
                Description = "Produto com nome nulo",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Name == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithEmptyDescription_WhenDescriptionIsEmpty()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Sem Descrição",
                Description = "",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Description == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProductWithNullDescription_WhenDescriptionIsNull()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Com Descrição Nula",
                Description = "",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p => p.Description == ""), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCall_WhenProductIsCreated()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Teste",
                Description = "Descrição Teste",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _mockProductRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnUniqueGuid_WhenProductIsCreated()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Produto Único",
                Description = "Descrição Única",
                Price = 100m,
                StockQuantity = 10,
                CategoryId = Guid.NewGuid()
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