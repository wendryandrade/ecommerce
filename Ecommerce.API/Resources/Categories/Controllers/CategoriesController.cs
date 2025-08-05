using Ecommerce.API.Resources.Categories.DTOs.Requests;
using Ecommerce.API.Resources.Categories.DTOs.Responses;
using Ecommerce.Application.Features.Categories.Commands;
using Ecommerce.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Categories.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            return Ok(response);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
            if (category == null) return NotFound();
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
            return CreatedAtAction(nameof(GetById), new { id = id }, new { id = id });
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
            return result ? NoContent() : NotFound();
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
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
