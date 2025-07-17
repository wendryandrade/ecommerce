using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Carts.DTOs.Responses;
using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Carts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/carts/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            var command = new AddCartItemCommand
            {
                CustomerId = request.CustomerId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var success = await _mediator.Send(command);
            if (!success) return BadRequest("Produto não encontrado ou quantidade inválida.");

            return Ok();
        }

        // GET: api/carts/{customerId}
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CartResponse>> GetCart(Guid customerId)
        {
            var cart = await _mediator.Send(new GetCartByCustomerIdQuery(customerId));
            if (cart == null) return NotFound();

            var response = new CartResponse
            {
                CustomerId = cart.CustomerId,
                Items = cart.Items.Select(ci => new CartItemResponse
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.ProductName,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice
                }).ToList()
            };

            return Ok(response);
        }

        // DELETE: api/carts/{customerId}/items/{productId}
        [HttpDelete("{customerId}/items/{productId}")]
        public async Task<IActionResult> RemoveItem(Guid customerId, Guid productId)
        {
            var command = new RemoveCartItemCommand(customerId, productId);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // PATCH: api/carts/{customerId}/items/{productId}/decrease-quantity
        [HttpPatch("carts/{customerId}/items/{productId}/decrease-quantity")]
        public async Task<IActionResult> DecreaseQuantity(Guid customerId, Guid productId)
        {
            var result = await _mediator.Send(new DecreaseCartItemQuantityCommand
            {
                CustomerId = customerId,
                ProductId = productId
            });

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
