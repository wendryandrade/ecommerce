using Ecommerce.API.Resources.Carts.DTOs.Requests;
using Ecommerce.API.Resources.Carts.DTOs.Responses;
using Ecommerce.Application.Features.Carts.Commands;
using Ecommerce.Application.Features.Carts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.API.Resources.Carts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Método privado para obter o ID do usuário autenticado
        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido.");

            return userId;
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            var userId = GetUserIdFromToken();
            var command = new AddCartItemCommand
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var success = await _mediator.Send(command);
            if (!success) return BadRequest("Produto não encontrado ou quantidade inválida.");

            return Ok();
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetMyCart()
        {
            var userId = GetUserIdFromToken();
            var cart = await _mediator.Send(new GetCartByUserIdQuery(userId));
            if (cart == null)
                return Ok(new CartResponse { UserId = userId, Items = new List<CartItemResponse>() });

            var response = new CartResponse
            {
                UserId = cart.UserId,
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

        // DELETE: api/cart/items/{productId}
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(Guid productId)
        {
            var userId = GetUserIdFromToken();
            var command = new RemoveCartItemCommand(userId, productId);
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        // PATCH: api/cart/items/{productId}/decrease-quantity
        [HttpPatch("items/{productId}/decrease-quantity")]
        public async Task<IActionResult> DecreaseQuantity(Guid productId)
        {
            var userId = GetUserIdFromToken();
            var result = await _mediator.Send(new DecreaseCartItemQuantityCommand
            {
                UserId = userId,
                ProductId = productId
            });

            if (!result)
                return NotFound();

            return NoContent();
        }
    }                       
}
