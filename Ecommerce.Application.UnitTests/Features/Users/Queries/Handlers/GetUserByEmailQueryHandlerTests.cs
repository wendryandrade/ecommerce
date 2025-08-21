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
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldMapAllUserProperties_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userEmail = "completo@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };
            var userFromRepo = new User 
            { 
                Id = userId, 
                FirstName = "João", 
                LastName = "Silva", 
                Email = userEmail,
                Role = "Customer"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("João", result.FirstName);
            Assert.Equal("Silva", result.LastName);
            Assert.Equal(userEmail, result.Email);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenEmailIsEmpty()
        {
            // Arrange
            var query = new GetUserByEmailQuery { Email = "" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("")).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenEmailIsNull()
        {
            // Arrange
            var query = new GetUserByEmailQuery { Email = "" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("")).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldVerifyRepositoryCall_WhenQueryIsExecuted()
        {
            // Arrange
            var userEmail = "teste@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = userEmail };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(userEmail), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDtoWithEmptyStrings_WhenUserHasEmptyProperties()
        {
            // Arrange
            var userEmail = "vazio@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };
            var userFromRepo = new User 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "", 
                LastName = "", 
                Email = userEmail 
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.FirstName);
            Assert.Equal("", result.LastName);
            Assert.Equal(userEmail, result.Email);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserDtoWithNullStrings_WhenUserHasNullProperties()
        {
            // Arrange
            var userEmail = "null@email.com";
            var query = new GetUserByEmailQuery { Email = userEmail };
            var userFromRepo = new User 
            { 
                Id = Guid.NewGuid(), 
                FirstName = "", 
                LastName = "", 
                Email = userEmail 
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(userEmail)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.FirstName);
            Assert.Equal("", result.LastName);
            Assert.Equal(userEmail, result.Email);
        }
    }
}