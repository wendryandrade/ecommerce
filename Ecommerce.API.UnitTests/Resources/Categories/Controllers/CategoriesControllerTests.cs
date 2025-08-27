using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.API.Resources.Categories.Controllers;
using Ecommerce.API.Resources.Categories.DTOs.Requests;
using Ecommerce.API.Resources.Categories.DTOs.Responses;
using Ecommerce.Application.Features.Categories.DTOs;
using Ecommerce.Application.Features.Categories.Queries;
using Ecommerce.Application.Features.Categories.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Ecommerce.API.UnitTests.Resources.Categories.Controllers
{
	public class CategoriesControllerTests
	{
		private readonly Mock<IMediator> _mockMediator = new();
		private readonly CategoriesController _controller;

		public CategoriesControllerTests()
		{
			_controller = new CategoriesController(_mockMediator.Object);
		}

		[Fact]
		public async Task GetAll_ShouldReturnOk_WithMappedResponses()
		{
			var dtos = new List<CategoryDto>
			{
				new() { Id = Guid.NewGuid(), Name = "Cat", Description = "Desc", Products = new List<CategoryProductDto>() }
			};
			_mockMediator.Setup(m => m.Send(It.IsAny<GetAllCategoriesQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dtos);

			var result = await _controller.GetAll();
			var ok = Assert.IsType<OkObjectResult>(result);
			var payload = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);
			Assert.Single(payload);
		}

		[Fact]
		public async Task GetById_ShouldReturnNotFound_WhenNull()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync((CategoryDto?)null);
			var result = await _controller.GetById(Guid.NewGuid());
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task GetById_ShouldReturnOk_WhenFound()
		{
			var dto = new CategoryDto { Id = Guid.NewGuid(), Name = "Cat" };
			_mockMediator.Setup(m => m.Send(It.IsAny<GetCategoryByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);
			var result = await _controller.GetById(dto.Id);
			Assert.IsType<OkObjectResult>(result);
		}

		[Fact]
		public async Task Create_ShouldReturnCreated()
		{
			var id = Guid.NewGuid();
			_mockMediator.Setup(m => m.Send(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(id);
			var req = new CreateCategoryRequest { Name = "Cat", Description = "Desc" };
			var result = await _controller.Create(req);
			Assert.IsType<CreatedAtActionResult>(result);
		}

		[Fact]
		public async Task Update_ShouldReturnNoContent_WhenTrue()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			var req = new CategoryRequest { Name = "N", Description = "D" };
			var result = await _controller.Update(Guid.NewGuid(), req);
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task Update_ShouldReturnNotFound_WhenFalse()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
			var req = new CategoryRequest { Name = "N", Description = "D" };
			var result = await _controller.Update(Guid.NewGuid(), req);
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task Delete_ShouldReturnNoContent_WhenDeleted()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
			var result = await _controller.Delete(Guid.NewGuid());
			Assert.IsType<NoContentResult>(result);
		}

		[Fact]
		public async Task Delete_ShouldReturnNotFound_WhenNotDeleted()
		{
			_mockMediator.Setup(m => m.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
			var result = await _controller.Delete(Guid.NewGuid());
			Assert.IsType<NotFoundObjectResult>(result);
		}

		[Fact]
		public async Task Delete_ShouldReturnBadRequest_OnInvalidOperation()
		{
			_mockMediator
				.Setup(m => m.Send(It.IsAny<DeleteCategoryCommand>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new InvalidOperationException("err"));
			var result = await _controller.Delete(Guid.NewGuid());
			var bad = Assert.IsType<BadRequestObjectResult>(result);
			Assert.NotNull(bad.Value);
		}
	}
}






