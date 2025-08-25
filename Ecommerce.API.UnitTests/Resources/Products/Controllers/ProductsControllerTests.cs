using Ecommerce.API.Resources.Products.DTOs.Requests;
using Ecommerce.Application.Products.Dtos;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Ecommerce.API.Resources.Products.Controllers;

namespace Ecommerce.API.UnitTests.Resources.Products.Controllers
{
	public class ProductsControllerTests
	{
		private readonly Mock<IMediator> _mockMediator = new();
		private readonly ProductsController _controller;

		public ProductsControllerTests()
		{
			_controller = new ProductsController(_mockMediator.Object);
		}

		[Fact]
		public async Task Get_ShouldReturnOk_WithMappedResponses()
		{
			var dtos = new List<ProductDto>
			{
				new() { Id = Guid.NewGuid(), Name = "P1", Description = "D1", Price = 10m, StockQuantity = 1, CategoryName = "C" }
			};
			_mockMediator.Setup(m => m.Send(It.IsAny<GetAllProductsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dtos);

			var result = await _controller.GetAll();
			var ok = Assert.IsType<OkObjectResult>(result);
			var payloadEnumerable = Assert.IsAssignableFrom<IEnumerable<Ecommerce.API.Resources.Products.DTOs.Responses.ProductResponse>>(ok.Value);
			var payload = payloadEnumerable.ToList();
			Assert.Single(payload);
			Assert.Equal(dtos[0].Name, payload[0].Name);
		}

		[Fact]
		public async Task GetById_ShouldReturnNotFound_WhenNull()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);
			var result = await _controller.GetById(Guid.NewGuid());
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task GetById_ShouldReturnOk_WhenFound()
		{
			var dto = new ProductDto { Id = Guid.NewGuid(), Name = "X", Description = "D", Price = 1m, StockQuantity = 2, CategoryName = "C" };
			_mockMediator.Setup(m => m.Send(It.IsAny<GetProductByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
			var result = await _controller.GetById(dto.Id);
			var ok = Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task Create_ShouldSendCommand_AndReturnCreated()
		{
			var id = Guid.NewGuid();
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(id);
			var req = new CreateProductRequest { Name = "N", Description = "D", Price = 1m, StockQuantity = 1, CategoryId = Guid.NewGuid() };
			var result = await _controller.Create(req);
			var created = Assert.IsType<CreatedAtActionResult>(result);
			Assert.Equal(nameof(ProductsController.GetById), created.ActionName);
		}

		[Fact]
		public async Task Update_ShouldReturnBadRequest_WhenRouteBodyMismatch()
		{
			var req = new UpdateProductRequest { Id = Guid.NewGuid() };
			var result = await _controller.Update(Guid.NewGuid(), req);
			var bad = Assert.IsType<BadRequestObjectResult>(result);
		}

		[Fact]
		public async Task Update_ShouldReturnNotFound_WhenMediatorReturnsNull()
		{
			var id = Guid.NewGuid();
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);
			var req = new UpdateProductRequest { Id = id, Name = "N", Description = "D", Price = 1m, StockQuantity = 1, CategoryId = Guid.NewGuid() };
			var result = await _controller.Update(id, req);
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task Update_ShouldReturnNoContent_WhenUpdated()
		{
			var dto = new ProductDto { Id = Guid.NewGuid(), Name = "N" };
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
			var req = new UpdateProductRequest { Id = dto.Id, Name = "N" };
			var result = await _controller.Update(dto.Id, req);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task Delete_ShouldReturnNoContent_WhenDeleted()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			var result = await _controller.Delete(Guid.NewGuid());
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task Delete_ShouldReturnNotFound_WhenNotDeleted()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
			var result = await _controller.Delete(Guid.NewGuid());
			Assert.IsType<NotFoundObjectResult>(result);
		}
	}
}


