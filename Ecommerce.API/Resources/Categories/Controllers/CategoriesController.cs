using Ecommerce.API.Resources.Categories.DTOs.Requests;
using Ecommerce.API.Resources.Categories.DTOs.Responses;
using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.API.Common;

namespace Ecommerce.API.Resources.Categories.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categoryDtos = await _mediator.Send(new GetAllCategoriesQuery());

            var response = categoryDtos.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Products = c.Products.Select(p => new CategoryProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity
                }).ToList()
            });

            Response.AddSuccessMessage("Categorias recuperadas com sucesso.");
            return Ok(response);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (category == null)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Category not found."
                });
            Response.AddSuccessMessage("Categoria recuperada com sucesso.");
            return Ok(category);
        }

        // POST: api/categories - Protegido para Admin
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var command = new CreateCategoryCommand
            {
                Name = request.Name,
                Description = request.Description
            };

            var id = await _mediator.Send(command);
            Response.AddSuccessMessage("Categoria criada com sucesso.");
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        
        // PUT: api/categories/{id} - Protegido para Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request)
        {
            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await _mediator.Send(command);
            if (!result)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Category not found."
                });
            Response.AddSuccessMessage("Categoria atualizada com sucesso.");
            return NoContent();
        }

        // DELETE: api/categories/{id} - Protegido para Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _mediator.Send(new DeleteCategoryCommand { Id = id });
                if (!result)
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Not found",
                        Detail = "Category not found."
                    });

                Response.AddSuccessMessage("Categoria deletada com sucesso.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid operation",
                    Detail = ex.Message
                });
            }
        }
    }
}
