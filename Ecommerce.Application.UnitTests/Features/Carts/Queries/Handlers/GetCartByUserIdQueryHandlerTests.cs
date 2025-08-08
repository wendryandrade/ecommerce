using Ecommerce.Application.Features.Carts.DTOs;
using Ecommerce.Application.Features.Carts.Queries;
using Ecommerce.Application.Features.Carts.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Carts.Queries.Handlers
{
    public class GetCartByUserIdQueryHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly GetCartByUserIdQueryHandler _handler;

        public GetCartByUserIdQueryHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _handler = new GetCartByUserIdQueryHandler(_mockCartRepository.Object);
        }

        [Fact]
        // Deveria retornar um CartDto quando o carrinho existe
        public async Task Handle_ShouldReturnCartDto_WhenCartExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCartByUserIdQuery(userId);

            // Criar produtos falsos para o nosso teste
            var productA = new Product { Id = Guid.NewGuid(), Name = "Produto A" };
            var productB = new Product { Id = Guid.NewGuid(), Name = "Produto B" };

            // Criar um carrinho falso com itens e associar os produtos falsos
            var cartFromRepo = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>
                {
                    // Associamos o objeto Product inteiro
                    new CartItem { ProductId = productA.Id, Product = productA, Quantity = 2, UnitPrice = 10 },
                    // E aqui também
                    new CartItem { ProductId = productB.Id, Product = productB, Quantity = 1, UnitPrice = 25 }
                }
            };

            // "Ensinar" o mock a retornar o nosso carrinho falso
            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(cartFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CartDto>(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(2, result.Items.Count); // Verifica se os 2 itens foram mapeados
            Assert.Equal("Produto A", result.Items[0].ProductName); // Agora podemos verificar o nome
        }

        [Fact]
        // Deveria retornar null quando o carrinho não existe
        public async Task Handle_ShouldReturnNull_WhenCartDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetCartByUserIdQuery(userId);

            _mockCartRepository.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((Cart)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}