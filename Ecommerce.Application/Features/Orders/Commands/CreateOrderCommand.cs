using Ecommerce.Application.Features.Orders.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Commands
{
    public class CreateOrderCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public required OrderAddressDto ShippingAddress { get; set; }
        public required OrderPaymentDto PaymentDetails { get; set; }
    }
}