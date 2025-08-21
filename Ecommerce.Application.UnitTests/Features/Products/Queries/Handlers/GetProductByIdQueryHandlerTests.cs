using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Application.Products.Queries.Handlers;
using Ecommerce.Domain.Entities;
using Moq;
using Xunit;

namespace Ecommerce.Application.UnitTests.Features.Products.Queries.Handlers
{
	public class GetProductByIdQueryHandlerTests
	{
		private readonly Mock<IProductRepository> _mockProductRepository = new();
		private readonly GetProductByIdQueryHandler _handler;

		public GetProductByIdQueryHandlerTests()
		{
			_handler = new GetProductByIdQueryHandler(_mockProductRepository.Object);
		}

		[Fact]
		public async Task Handle_ShouldReturnNull_WhenNotFound()
		{
			_mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
				.ReturnsAsync((Product?)null);

			var result = await _handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);
			Assert.Null(result);
		}

		[Fact]
		public async Task Handle_ShouldMapFields_WhenFound()
		{
			var id = Guid.NewGuid();
			var product = new Product { Id = id, Name = "N", Description = "D", Price = 1m, StockQuantity = 2, Category = new Category { Name = "C" } };
			_mockProductRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

			var result = await _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);
			Assert.NotNull(result);
			Assert.Equal(product.Name, result!.Name);
			Assert.Equal("C", result.CategoryName);
		}
	}
}


