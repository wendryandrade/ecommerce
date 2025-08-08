using Ecommerce.Application.Features.Users.DTOs;
using Ecommerce.Application.Features.Users.Queries;
using Ecommerce.Application.Features.Users.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Users.Queries
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly GetUserByIdQueryHandler _handler;

        public GetUserByIdQueryHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new GetUserByIdQueryHandler(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Teste", Email = "teste@email.com" };
            var query = new GetUserByIdQuery { Id = userId };

            // "Ensina" o mock a retornar o utilizador falso QUANDO o método GetByIdAsync
            // for chamado com ESTE ID específico.
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserDto>(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new GetUserByIdQuery { Id = userId };

            // "Ensina" o mock a retornar nulo quando o método GetByIdAsync for chamado
            // com qualquer Guid. It.IsAny<Guid>() é um "coringa".
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}