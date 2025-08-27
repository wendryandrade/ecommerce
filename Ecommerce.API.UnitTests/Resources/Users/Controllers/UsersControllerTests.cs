using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Users.Controllers;
using Ecommerce.API.Resources.Users.DTOs.Responses;
using Ecommerce.Application.Features.Users.DTOs;
using Ecommerce.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Users.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new();
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _controller = new UsersController(_mockMediator.Object);
            var http = new DefaultHttpContext();
            http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = http };
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((UserDto?)null);
            var result = await _controller.GetById(Guid.NewGuid());
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            var dto = new UserDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", Email = "a@b.com" };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
            var result = await _controller.GetById(dto.Id);
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<UserResponse>(ok.Value);
            Assert.Equal(dto.Email, payload.Email);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserDto> { new() { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", Email = "a@b.com" } });
            var res = await _controller.GetAll();
            var ok = Assert.IsType<OkObjectResult>(res);
            var list = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(ok.Value);
            Assert.Single(list);
        }
    }
}


