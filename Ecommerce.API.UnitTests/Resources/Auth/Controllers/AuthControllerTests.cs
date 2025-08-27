using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Auth.Controllers;
using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Users.DTOs.Requests;
using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Auth.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediator = new();
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _controller = new AuthController(_mediator.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_OnInvalidOperation()
        {
            _mediator.Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("duplicate"));

            var req = new CreateUserRequest { FirstName = "A", LastName = "B", Email = "a@b.com", Password = "P@ssw0rd" };
            var res = await _controller.Register(req);
            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenTokenIsNull()
        {
            _mediator.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var res = await _controller.Login(new LoginRequest { Email = "x@y.com", Password = "z" });
            Assert.IsType<UnauthorizedObjectResult>(res);
        }
    }
}
