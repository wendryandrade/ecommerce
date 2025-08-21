using Ecommerce.Application.Features.Orders.DTOs;

namespace Ecommerce.Application.Features.Orders.Events
{
    public class OrderSubmissionEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public OrderAddressDto ShippingAddress { get; set; } = new();
        public OrderPaymentDto PaymentDetails { get; set; } = new();
        public List<EventCartItem> CartItems { get; set; } = new();
    }

    public class EventCartItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}