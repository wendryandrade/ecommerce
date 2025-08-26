using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Auth.Commands.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;
using System.Security.Cryptography;

namespace Ecommerce.Application.UnitTests.Features.Auth.Commands.Handlers
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuthService = new Mock<IAuthService>();
            _handler = new LoginCommandHandler(_mockUserRepository.Object, _mockAuthService.Object);
        }

        // Método auxiliar para gerar um hash de senha válido para os testes
        private static string GenerateValidPasswordHash(string password, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[52];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 20);
            return Convert.ToBase64String(hashBytes);
        }

        [Fact]
        public async Task Handle_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "Password123!";
            // Usamos um salt determinístico mas não totalmente previsível para o teste
            var salt = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            var command = new LoginCommand { Email = "teste@email.com", Password = password };
            var passwordHash = GenerateValidPasswordHash(password, salt);
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = passwordHash, Role = "Customer" };
            var expectedToken = "token_jwt_gerado_aqui";

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(userFromRepo);
            _mockAuthService.Setup(auth => auth.GenerateJwtToken(It.IsAny<User>())).Returns(expectedToken);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new LoginCommand { Email = "naoexiste@email.com", Password = "123" };
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var correctPassword = "Password123!";
            var incorrectPassword = "senha_errada";
            // Usamos um salt determinístico mas não totalmente previsível para o teste
            var salt = new byte[32] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
            var command = new LoginCommand { Email = "teste@email.com", Password = incorrectPassword };

            // Geramos um hash com a senha CORRETA para simular o que está no "banco"
            var validPasswordHash = GenerateValidPasswordHash(correctPassword, salt);
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = validPasswordHash, Role = "Customer" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(userFromRepo);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result); // Esperamos nulo porque a senha enviada ("senha_errada") está incorreta
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordHashIsInvalid()
        {
            // Arrange
            var command = new LoginCommand { Email = "teste@email.com", Password = "Password123!" };
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "invalid_hash_format", Role = "Customer" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(userFromRepo);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordHashIsEmpty()
        {
            // Arrange
            var command = new LoginCommand { Email = "teste@email.com", Password = "Password123!" };
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = "", Role = "Customer" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(userFromRepo);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordHashIsNull()
        {
            // Arrange
            var command = new LoginCommand { Email = "teste@email.com", Password = "Password123!" };
            var userFromRepo = new User { Id = Guid.NewGuid(), Email = command.Email, PasswordHash = null!, Role = "Customer" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(userFromRepo);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenEmailIsEmpty()
        {
            // Arrange
            var command = new LoginCommand { Email = "", Password = "Password123!" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordIsEmpty()
        {
            // Arrange
            var command = new LoginCommand { Email = "teste@email.com", Password = "" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}