using Ecommerce.API.Resources.Addresses.DTOs.Responses;
using Ecommerce.API.Resources.Orders.DTOs.Requests;
using Ecommerce.API.Resources.Orders.DTOs.Responses;
using Ecommerce.API.Resources.Payments.DTOs.Responses;
using Ecommerce.Application.Features.Orders.Commands;
using Ecommerce.Application.Features.Orders.DTOs;
using Ecommerce.Application.Features.Orders.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Orders.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Endpoint para finalizar a compra e criar um pedido
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var command = new CreateOrderCommand
                {
                    CustomerId = request.CustomerId,
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
                return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { OrderId = orderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Endpoint para buscar todos os pedidos de um cliente
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        public async Task<ActionResult<List<OrderResponse>>> GetOrdersByCustomerId(Guid customerId)
        {
            var query = new GetOrdersByCustomerIdQuery(customerId);
            var orderDtos = await _mediator.Send(query);
            var response = orderDtos.Select(dto => MapToOrderResponse(dto)).ToList();
            return Ok(response);
        }

        // Endpoint para buscar um único pedido pelo seu ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var query = new GetOrderByIdQuery { Id = id };
            var orderDto = await _mediator.Send(query);

            if (orderDto == null) return NotFound();

            var response = MapToOrderResponse(orderDto);

            return Ok(response);
        }

        // Endpoint para atualizar o status de um pedido
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
            if (!result) return NotFound();
            return NoContent();
        }

        private OrderResponse MapToOrderResponse(OrderDto dto)
        {
            return new OrderResponse
            {
                Id = dto.Id,
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
                } : null
            };
        }
    }
}