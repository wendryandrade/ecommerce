using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Carts.Controllers;
using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Carts.DTOs.Responses;
using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.DTOs;
using Ecommerce.Application.Features.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Carts.Controllers
{
	public class CartControllerTests
	{
		private readonly Mock<IMediator> _mockMediator = new();
		private readonly CartController _controller;
		private readonly Guid _userId = Guid.NewGuid();

		public CartControllerTests()
		{
			_controller = new CartController(_mockMediator.Object);
			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = new ClaimsPrincipal(new ClaimsIdentity(new[]
					{
						new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
					}, "mock"))
				}
			};
		}

		[Fact]
		public async Task AddItem_ShouldReturnOk_WhenMediatorReturnsTrue()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<AddCartItemCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			var result = await _controller.AddItem(new AddCartItemRequest { ProductId = Guid.NewGuid(), Quantity = 1 });
			Assert.IsType<OkResult>(result);
		}

		[Fact]
		public async Task AddItem_ShouldReturnBadRequest_WhenMediatorReturnsFalse()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<AddCartItemCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);

			var result = await _controller.AddItem(new AddCartItemRequest { ProductId = Guid.NewGuid(), Quantity = 0 });
			Assert.IsType<BadRequestObjectResult>(result);
		}

		[Fact]
		public async Task GetMyCart_ShouldReturnEmpty_WhenNull()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetCartByUserIdQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((CartDto?)null);

			var result = await _controller.GetMyCart();
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var payload = Assert.IsType<CartResponse>(ok.Value);
			Assert.Empty(payload.Items);
			Assert.Equal(_userId, payload.UserId);
		}

		[Fact]
		public async Task GetMyCart_ShouldMapItems_WhenExists()
		{
			var dto = new CartDto
			{
				UserId = _userId,
				Items = new List<CartItemDto>
				{
					new CartItemDto { ProductId = Guid.NewGuid(), ProductName = "P", Quantity = 2, UnitPrice = 3m }
				}
			};
			_mockMediator.Setup(m => m.Send(It.IsAny<GetCartByUserIdQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(dto);

			var result = await _controller.GetMyCart();
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var payload = Assert.IsType<CartResponse>(ok.Value);
			Assert.Single(payload.Items);
		}

		[Fact]
		public async Task RemoveItem_ShouldReturnNoContent_WhenRemoved()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<RemoveCartItemCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			var result = await _controller.RemoveItem(Guid.NewGuid());
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task RemoveItem_ShouldReturnNotFound_WhenNotRemoved()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<RemoveCartItemCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);

			var result = await _controller.RemoveItem(Guid.NewGuid());
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task DecreaseQuantity_ShouldReturnNoContent_WhenOk()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DecreaseCartItemQuantityCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			var result = await _controller.DecreaseQuantity(Guid.NewGuid());
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task DecreaseQuantity_ShouldReturnNotFound_WhenFail()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DecreaseCartItemQuantityCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);

			var result = await _controller.DecreaseQuantity(Guid.NewGuid());
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task GetMyCart_ShouldThrowUnauthorized_WhenNoUserClaim()
		{
			// remove claims
			_controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
			await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMyCart());
		}
	}
}


