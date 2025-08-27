using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Models;
using Ecommerce.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _loggerMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly EmailSettings _emailSettings;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _loggerMock = new Mock<ILogger<EmailService>>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailSenderMock = new Mock<IEmailSender>();
            
            _emailSettings = new EmailSettings
            {
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                SmtpUsername = "test@example.com",
                SmtpPassword = "test-password",
                FromEmail = "noreply@ecommerce.com",
                FromName = "E-commerce Store",
                EnableSsl = true
            };

            var optionsMock = new Mock<IOptions<EmailSettings>>();
            optionsMock.Setup(x => x.Value).Returns(_emailSettings);

            _emailService = new EmailService(_loggerMock.Object, optionsMock.Object, _userRepositoryMock.Object, _emailSenderMock.Object);
        }

        [Fact]
        public async Task SendOrderConfirmationAsync_ShouldLogWarning_WhenEmailSettingsNotConfigured()
        {
            // Arrange
            var emptyEmailSettings = new EmailSettings();
            var optionsMock = new Mock<IOptions<EmailSettings>>();
            optionsMock.Setup(x => x.Value).Returns(emptyEmailSettings);

            var emailService = new EmailService(_loggerMock.Object, optionsMock.Object, _userRepositoryMock.Object, _emailSenderMock.Object);

            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var totalAmount = 100.00m;

            // Act
            await emailService.SendOrderConfirmationAsync(userId, orderId, totalAmount);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Configurações de email não definidas")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOrderConfirmationAsync_ShouldLogError_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var totalAmount = 100.00m;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            await _emailService.SendOrderConfirmationAsync(userId, orderId, totalAmount);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Usuário não encontrado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOrderConfirmationAsync_ShouldLogError_WhenUserEmailIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var totalAmount = 100.00m;

            var user = new User
            {
                Id = userId,
                FirstName = "João",
                LastName = "Silva",
                Email = string.Empty
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _emailService.SendOrderConfirmationAsync(userId, orderId, totalAmount);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email do usuário não encontrado")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOrderConfirmationAsync_ShouldCallUserRepository_WithCorrectUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var totalAmount = 100.00m;

            var user = new User
            {
                Id = userId,
                FirstName = "João",
                LastName = "Silva",
                Email = "joao@example.com"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _emailService.SendOrderConfirmationAsync(userId, orderId, totalAmount);

            // Assert
            _userRepositoryMock.Verify(x => x.GetByIdAsync(userId), Times.Once);
            _emailSenderMock.Verify(s => s.SendAsync(It.IsAny<EmailSettings>(), user.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendOrderConfirmationAsync_ShouldLogSuccess_WhenEmailSent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var totalAmount = 100.00m;

            var user = new User
            {
                Id = userId,
                FirstName = "João",
                LastName = "Silva",
                Email = "joao@example.com"
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _emailService.SendOrderConfirmationAsync(userId, orderId, totalAmount);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email de confirmação enviado com sucesso")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
