using Ecommerce.Application.Features.Products.Commands;
using Ecommerce.Application.Features.Products.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Domain.Entities;
using Moq;

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
    }
}