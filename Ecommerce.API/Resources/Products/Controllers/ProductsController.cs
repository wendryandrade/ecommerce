using Ecommerce.API.Resources.Products.DTOs.Requests;
using Ecommerce.API.Resources.Products.DTOs.Responses;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/products
        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> Get()
        {
            var products = await _mediator.Send(new GetAllProductsQuery());

            var response = products.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.CategoryName
            }).ToList();

            return Ok(response);
        }

        // GET api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            if (product == null) return NotFound();

            var response = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryName = product.CategoryName
            };

            return Ok(response);
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var command = new CreateProductCommand
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId
            };

            var productId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = productId }, null);
        }

        // PUT api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            if (id != request.Id)
                return BadRequest("ID do corpo e da URL não coincidem");

            var command = new UpdateProductCommand
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId
            };

            var result = await _mediator.Send(command);
            if (result == null) return NotFound();

            var response = new ProductResponse
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                Price = result.Price,
                StockQuantity = result.StockQuantity,
                CategoryName = result.CategoryName
            };

            return Ok(response);
        }

        // DELETE api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeleteProductCommand(id));
            if (!success) return NotFound();

            return NoContent();
        }
    }
}
