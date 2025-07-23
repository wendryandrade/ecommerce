using Ecommerce.Application.Features.Orders.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Commands
{
    public class CreateOrderCommand : IRequest<Guid>
    {
        public Guid CustomerId { get; set; }
        public OrderAddressDto ShippingAddress { get; set; }
        public OrderPaymentDto PaymentDetails { get; set; }
    }
}