using Ecommerce.API.Resources.Products.DTOs.Requests;
using Ecommerce.API.Resources.Products.DTOs.Responses;
using Ecommerce.Application.Products.Commands;
using Ecommerce.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.API.Common;

namespace Ecommerce.API.Resources.Products.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
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
            });
            Response.AddSuccessMessage("Produtos recuperados com sucesso.");
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            if (product == null)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Produto não encontrado."
                });
            Response.AddSuccessMessage("Produto recuperado com sucesso.");
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
            Response.AddSuccessMessage("Produto criado com sucesso.");
            return CreatedAtAction(nameof(GetById), new { id = productId }, new { id = productId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            if (id != request.Id)
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = "ID do corpo e da URL não coincidem."
                });

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
            if (result == null)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Produto não encontrado."
                });

            Response.AddSuccessMessage("Produto atualizado com sucesso.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeleteProductCommand(id));
            if (!success)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Produto não encontrado."
                });

            Response.AddSuccessMessage("Produto deletado com sucesso.");
            return NoContent();
        }
    }
}
