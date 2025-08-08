using Ecommerce.Application.Features.Users.DTOs;
using Ecommerce.Application.Features.Users.Queries;
using Ecommerce.Application.Features.Users.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Users.Queries
{
    public class GetAllUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly GetAllUsersQueryHandler _handler;

        public GetAllUsersQueryHandlerTests()
        {
            // "Arrange" - Configuração partilhada para todos os testes
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new GetAllUsersQueryHandler(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnListOfUserDtos_WhenUsersExist()
        {
            // Arrange (Preparação)
            // 1. Criar uma lista falsa de utilizadores que o nosso repositório falso irá retornar.
            var usersFromRepo = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Ana", LastName = "Silva", Email = "ana@email.com", Role = "Customer" },
                new User { Id = Guid.NewGuid(), FirstName = "Carlos", LastName = "Mendes", Email = "carlos@email.com", Role = "Admin" }
            };

            // 2. "Ensinar" o nosso mock a comportar-se como esperamos.
            // Quando o método GetAllAsync for chamado, ele deve retornar a nossa lista falsa.
            _mockUserRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(usersFromRepo);

            var query = new GetAllUsersQuery();

            // Act (Ação)
            // Executar o método que queremos testar.
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert (Verificação)
            // Verificar se o resultado é o que esperávamos.
            Assert.NotNull(result);
            Assert.IsType<List<UserDto>>(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Ana", result[0].FirstName);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            // Desta vez, o repositório falso retorna uma lista vazia.
            _mockUserRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<User>());

            var query = new GetAllUsersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // A lista deve estar vazia.
        }
    }
}