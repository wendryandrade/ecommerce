using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Orders.Controllers;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Responses;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Orders.Controllers
{
	public class OrdersControllerTests
	{
		private readonly Mock<IMediator> _mockMediator = new();
		private readonly Mock<ILogger<OrdersController>> _mockLogger = new();
		private readonly Mock<IShippingService> _mockShippingService = new();
		private readonly OrdersController _controller;
		private readonly Guid _userId = Guid.NewGuid();

		public OrdersControllerTests()
		{
			_controller = new OrdersController(_mockMediator.Object, _mockLogger.Object, _mockShippingService.Object);

			// Configure authenticated user in HttpContext because controller reads userId from JWT claims
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
		public async Task GetOrderById_ShouldReturnNotFound_WhenNull()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderDto?)null);
			var result = await _controller.GetOrderById(Guid.NewGuid());
			Assert.IsType<NotFoundObjectResult>(result);
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
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task CreateOrder_ShouldReturnCreated_WhenOrderCreated()
		{
			var id = Guid.NewGuid();
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(id);
			var req = new CreateOrderRequest
			{
				// UserId is ignored by controller; it uses JWT claim
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
				ShippingAddress = new CreateOrderAddressRequest(),
				PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
			};
			
			// Act & Assert
			await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateOrder(req));
		}

		[Fact]
		public async Task CalculateShipping_ShouldReturnOk_WithShippingCost()
		{
			// Arrange
			var request = new CalculateShippingRequest
			{
				DestinationZipCode = "20040-000"
			};
			var expectedCost = 25.50m;
			var expectedDeliveryDays = 1;
			var expectedIsRealApi = true;

			_mockShippingService.Setup(s => s.CalculateShippingWithDetailsFromStoreAsync(request.DestinationZipCode))
				.ReturnsAsync((expectedCost, expectedDeliveryDays, expectedIsRealApi, new AddressInfo(), new AddressInfo()));
			
			_mockShippingService.Setup(s => s.GetStoreZipCode())
				.Returns("01310-100");

			// Act
			var result = await _controller.CalculateShipping(request);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			// Do basic shape assertions without depending on anonymous type
			var response = okResult.Value;
			Assert.NotNull(response);
			var shippingCostProp = response.GetType().GetProperty("ShippingCost");
			var currencyProp = response.GetType().GetProperty("Currency");
			var deliveryDaysProp = response.GetType().GetProperty("DeliveryDays");
			var isRealApiProp = response.GetType().GetProperty("IsRealApi");
			Assert.NotNull(shippingCostProp);
			Assert.NotNull(currencyProp);
			Assert.NotNull(deliveryDaysProp);
			Assert.NotNull(isRealApiProp);
			Assert.Equal(expectedCost, (decimal)shippingCostProp!.GetValue(response)!);
			Assert.Equal("BRL", (string)currencyProp!.GetValue(response)!);
			Assert.Equal(expectedDeliveryDays, (int)deliveryDaysProp!.GetValue(response)!);
			Assert.Equal(expectedIsRealApi, (bool)isRealApiProp!.GetValue(response)!);
		}

		[Fact]
		public async Task CalculateShipping_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
		{
			// Arrange
			var request = new CalculateShippingRequest
			{
				DestinationZipCode = ""
			};

			_mockShippingService.Setup(s => s.CalculateShippingWithDetailsFromStoreAsync(It.IsAny<string>()))
				.ThrowsAsync(new ArgumentException("Invalid zip code"));

			// Act
			var result = await _controller.CalculateShipping(request);

			// Assert
			Assert.IsType<BadRequestObjectResult>(result);
		}

		[Fact]
		public async Task CreateOrder_ShouldThrowUnauthorized_WhenNoClaims()
		{
			// Arrange
			var req = new CreateOrderRequest
			{
				ShippingAddress = new CreateOrderAddressRequest { Street = "S", City = "C", State = "ST", PostalCode = "123" },
				PaymentDetails = new CreateOrderPaymentRequest { PaymentMethod = PaymentMethod.CreditCard }
			};

			// Reset HttpContext without user and add a fake empty cart repo to trigger path
			var context = new DefaultHttpContext();
			_controller.ControllerContext = new ControllerContext { HttpContext = context };
			var services = new ServiceCollection();
			services.AddSingleton<ICartRepository>(new FakeEmptyCartRepository());
			_controller.ControllerContext.HttpContext.RequestServices = services.BuildServiceProvider();

			// Act & Assert
			await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.CreateOrder(req));
		}

		private class FakeEmptyCartRepository : ICartRepository
		{
			public Task AddAsync(Ecommerce.Domain.Entities.Cart cart) => Task.CompletedTask;
			public Task DeleteAsync(Guid userId, Guid productId) => Task.CompletedTask;
			public Task<Ecommerce.Domain.Entities.Cart?> GetByUserIdAsync(Guid userId) => Task.FromResult<Ecommerce.Domain.Entities.Cart?>(new Ecommerce.Domain.Entities.Cart { UserId = userId });
			public Task UpdateAsync(Ecommerce.Domain.Entities.Cart cart) => Task.CompletedTask;
		}

		[Fact]
		public async Task CalculateShipping_ShouldReturn500_OnUnhandledError()
		{
			_mockShippingService.Setup(s => s.CalculateShippingWithDetailsFromStoreAsync(It.IsAny<string>()))
				.ThrowsAsync(new Exception("unexpected"));
			var res = await _controller.CalculateShipping(new CalculateShippingRequest { DestinationZipCode = "01001000" });
			var obj = Assert.IsType<ObjectResult>(res);
			Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
		}
	}
}


