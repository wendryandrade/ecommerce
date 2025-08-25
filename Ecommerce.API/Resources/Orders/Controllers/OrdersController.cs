using Ecommerce.API.Resources.Addresses.DTOs.Responses;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Responses;
using Ecommerce.API.Resources.Payments.DTOs.Responses;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using Ecommerce.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce.API.Resources.Orders.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;
        private readonly IShippingService _shippingService;

        public OrdersController(IMediator mediator, ILogger<OrdersController> logger, IShippingService shippingService)
        {
            _mediator = mediator;
            _logger = logger;
            _shippingService = shippingService;
        }

        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido.");

            return userId;
        }

        [HttpPost("checkout")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, [FromServices] ICartRepository? cartRepository = null)
        {
            try
            {
                var userId = GetUserIdFromToken();

                if (cartRepository != null)
                {
                    var cart = await cartRepository.GetByUserIdAsync(userId);
                    if (cart == null || cart.CartItems == null || cart.CartItems.Count == 0)
                    {
                        return BadRequest(new ProblemDetails
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "Invalid request",
                            Detail = "O carrinho está vazio."
                        });
                    }
                }

                var command = new CreateOrderCommand
                {
                    UserId = userId,
                    ShippingAddress = new OrderAddressDto
                    {
                        Street = request.ShippingAddress.Street,
                        City = request.ShippingAddress.City,
                        State = request.ShippingAddress.State,
                        PostalCode = request.ShippingAddress.PostalCode
                    },
                    PaymentDetails = new OrderPaymentDto
                    {
                        PaymentMethod = request.PaymentDetails.PaymentMethod
                    }
                };
                var orderId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { id = orderId });
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

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        public async Task<ActionResult<List<OrderResponse>>> GetOrdersByUserId(Guid userId)
        {
            var query = new GetOrdersByUserIdQuery(userId);
            var orderDtos = await _mediator.Send(query);
            var response = orderDtos.Select(dto => MapToOrderResponse(dto)).ToList();
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var query = new GetOrderByIdQuery { Id = id };
            var orderDto = await _mediator.Send(query);

            if (orderDto == null)
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not found",
                    Detail = "Pedido não encontrado."
                });

            var response = MapToOrderResponse(orderDto);

            return Ok(response);
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = id,
                NewStatus = request.NewStatus
            };
            var result = await _mediator.Send(command);
            if (!result) return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = "Pedido não encontrado."
            });
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("calculate-shipping")]
        public async Task<IActionResult> CalculateShipping([FromBody] CalculateShippingRequest request)
        {
            try
            {
                var (shippingCost, deliveryDays, isRealApi, originAddress, destinationAddress) = await _shippingService.CalculateShippingWithDetailsFromStoreAsync(
                    request.DestinationZipCode);

                var response = new
                {
                    OriginZipCode = _shippingService.GetStoreZipCode(),
                    DestinationZipCode = request.DestinationZipCode,
                    ShippingCost = shippingCost,
                    Currency = "BRL",
                    DeliveryDays = deliveryDays,
                    IsRealApi = isRealApi,
                    OriginAddress = new
                    {
                        originAddress.ZipCode,
                        originAddress.Street,
                        originAddress.Neighborhood,
                        originAddress.City,
                        originAddress.State,
                        originAddress.FullAddress
                    },
                    DestinationAddress = new
                    {
                        destinationAddress.ZipCode,
                        destinationAddress.Street,
                        destinationAddress.Neighborhood,
                        destinationAddress.City,
                        destinationAddress.State,
                        destinationAddress.FullAddress
                    }
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid request",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete");
                return StatusCode(500, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal server error",
                    Detail = "Erro interno do servidor"
                });
            }
        }

        private static OrderResponse MapToOrderResponse(OrderDto dto)
        {
            return new OrderResponse
            {
                Id = dto.Id,
                UserId = dto.UserId,
                OrderDate = dto.OrderDate,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status.ToString(),
                Items = dto.OrderItems.Select(itemDto => new OrderItemResponse
                {
                    ProductId = itemDto.ProductId,
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice
                }).ToList(),
                Payment = dto.Payment != null ? new PaymentResponse
                {
                    Amount = dto.Payment.Amount,
                    PaymentMethod = dto.Payment.PaymentMethod.ToString(),
                    Status = dto.Payment.Status.ToString(),
                    TransactionId = dto.Payment.TransactionId
                } : null,
                ShippingAddress = dto.ShippingAddress != null ? new AddressResponse
                {
                    Street = dto.ShippingAddress.Street,
                    City = dto.ShippingAddress.City,
                    State = dto.ShippingAddress.State,
                    PostalCode = dto.ShippingAddress.PostalCode
                } : null,
                Shipping = dto.Shipping != null ? new ShippingResponse
                {
                    ShippingCost = dto.Shipping.ShippingCost,
                    DeliveryDays = dto.Shipping.DeliveryDays,
                    EstimatedDeliveryDate = dto.Shipping.EstimatedDeliveryDate
                } : null
            };
        }
    }
}