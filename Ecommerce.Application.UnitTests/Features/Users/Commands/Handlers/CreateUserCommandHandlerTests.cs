using Ecommerce.Application.Features.Users.Commands;
using Ecommerce.Application.Features.Users.Handlers;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Moq;

namespace Ecommerce.Application.UnitTests.Features.Users.Commands
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _handler = new CreateUserCommandHandler(_mockUserRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateUserAndReturnUserId_WhenEmailIsUnique()
        {
            // Arrange (Preparação)
            var command = new CreateUserCommand
            {
                FirstName = "Novo",
                LastName = "Utilizador",
                Email = "unico@email.com",
                Password = "Password123!"
            };

            // "Ensina" o mock a retornar null, simulando que o e-mail não foi encontrado.
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email))
                               .ReturnsAsync((User?)null);

            // Queremos verificar que o método AddAsync é chamado.
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

            // Act (Ação)
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert (Verificação)
            Assert.NotEqual(Guid.Empty, result); // O ID retornado não deve ser vazio.
            // Verifica se o método AddAsync foi chamado exatamente uma vez com qualquer objeto User.
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                FirstName = "Utilizador",
                LastName = "Existente",
                Email = "existente@email.com",
                Password = "Password123!"
            };

            var existingUser = new User { Id = Guid.NewGuid(), Email = command.Email };

            // "Ensina" o mock a retornar um utilizador, simulando que o e-mail já existe.
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(command.Email))
                               .ReturnsAsync(existingUser);

            // Act & Assert
            // Verificamos se o handler lança a exceção esperada.
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(command, CancellationToken.None));

            Assert.Equal("Um cliente com este e-mail já existe.", exception.Message);
        }
    }
}