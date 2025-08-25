using Ecommerce.Application.Features.Addresses.DTOs;
using Ecommerce.Application.Features.Payments.DTOs;

namespace Ecommerce.Application.Features.Orders.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Ecommerce.Domain.Enums.OrderStatus Status { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public PaymentDto? Payment { get; set; }
        public AddressDto? ShippingAddress { get; set; }
        public ShippingDto? Shipping { get; set; }
    }
}