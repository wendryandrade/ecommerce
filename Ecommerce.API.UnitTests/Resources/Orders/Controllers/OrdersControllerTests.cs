using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Orders.Controllers;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Responses;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Orders.Controllers
{
	public class OrdersControllerTests
	{
		private readonly Mock<IMediator> _mockMediator = new();
		private readonly OrdersController _controller;

		public OrdersControllerTests()
		{
			_controller = new OrdersController(_mockMediator.Object);
		}

		[Fact]
		public async Task GetOrderById_ShouldReturnNotFound_WhenNull()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderDto?)null);
			var result = await _controller.GetOrderById(Guid.NewGuid());
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task GetOrderById_ShouldReturnOk_WhenFound()
		{
			var dto = new OrderDto { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = OrderStatus.Pending };
			_mockMediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
			var result = await _controller.GetOrderById(dto.Id);
			var ok = Assert.IsType<OkObjectResult>(result);
			var payload = Assert.IsType<OrderResponse>(ok.Value);
			Assert.Equal(dto.Id, payload.Id);
		}

		[Fact]
		public async Task GetOrdersByUserId_ShouldReturnOk_WithOrders()
		{
			var dtos = new List<OrderDto> { new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Status = OrderStatus.Pending } };
			_mockMediator.Setup(m => m.Send(It.IsAny<GetOrdersByUserIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dtos);
			var result = await _controller.GetOrdersByUserId(Guid.NewGuid());
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var payload = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(ok.Value);
			Assert.Single(payload);
		}

		[Fact]
		public async Task GetOrdersByUserId_ShouldReturnEmpty_WhenNoOrders()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetOrdersByUserIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<OrderDto>());
			var result = await _controller.GetOrdersByUserId(Guid.NewGuid());
			var ok = Assert.IsType<OkObjectResult>(result.Result);
			var payload = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(ok.Value);
			Assert.Empty(payload);
		}

		[Fact]
		public async Task UpdateStatus_ShouldReturnNoContent_WhenTrue()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			var req = new UpdateOrderStatusRequest { NewStatus = OrderStatus.Paid };
			var result = await _controller.UpdateStatus(Guid.NewGuid(), req);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task UpdateStatus_ShouldReturnNotFound_WhenFalse()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
			var req = new UpdateOrderStatusRequest { NewStatus = OrderStatus.Paid };
			var result = await _controller.UpdateStatus(Guid.NewGuid(), req);
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task CreateOrder_ShouldReturnCreated_WhenOrderCreated()
		{
			var id = Guid.NewGuid();
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(id);
			var req = new CreateOrderRequest
			{
				UserId = Guid.NewGuid(),
				ShippingAddress = new CreateOrderAddressRequest { Street = "123 Main St", City = "City", State = "State", PostalCode = "12345" },
				PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
			};
			var result = await _controller.CreateOrder(req);
			var created = Assert.IsType<CreatedAtActionResult>(result);
			Assert.Equal(nameof(OrdersController.GetOrderById), created.ActionName);
		}

		[Fact]
		public async Task CreateOrder_ShouldReturnBadRequest_OnInvalidOperation()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("err"));
			var req = new CreateOrderRequest
			{
				UserId = Guid.NewGuid(),
				ShippingAddress = new CreateOrderAddressRequest(),
				PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
			};
			var result = await _controller.CreateOrder(req);
			var bad = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(bad.Value);
		}

		[Fact]
		public async Task CreateOrder_ShouldThrowArgumentException_WhenArgumentExceptionIsThrown()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("Invalid request"));
			var req = new CreateOrderRequest
			{
				UserId = Guid.NewGuid(),
				ShippingAddress = new CreateOrderAddressRequest(),
				PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
			};
			
			// Act & Assert
			await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateOrder(req));
		}
	}
}


