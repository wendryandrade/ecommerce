using Ecommerce.Application.Features.Users.DTOs;
using Ecommerce.Application.Features.Users.Queries;
using Ecommerce.Application.Features.Users.Queries.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Users.Queries.Handlers
{
    public class GetUserByEmailQueryHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly GetUserByEmailQueryHandler _handler;

        public GetUserByEmailQueryHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new GetUserByEmailQueryHandler(_mockUserRepository.Object);
        }

        [Fact]
        // Deveria retornar um UserDto quando o usuário existe
        public async Task Handle_ShouldReturnUserDto_WhenUserExists()
        {
            // Arrange
            var userEmail = "teste@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };
            var userFromRepo = new User { Id = Guid.NewGuid(), FirstName = "Utilizador", Email = userEmail };

            // "Ensina" o mock a retornar o nosso utilizador falso quando o método GetByEmailAsync
            // for chamado com este e-mail específico.
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UserDto>(result);
            Assert.Equal(userEmail, result.Email);
        }

        [Fact]
        // Deveria retornar nulo quando o usuário não existe
        public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userEmail = "naoexiste@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };

            // "Ensina" o mock a retornar nulo quando o método GetByEmailAsync for chamado.
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}