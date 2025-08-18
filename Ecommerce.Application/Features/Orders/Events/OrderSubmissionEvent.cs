using Ecommerce.Application.Features.Orders.DTOs;

namespace Ecommerce.Application.Features.Orders.Events
{
    public class OrderSubmissionEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public OrderAddressDto ShippingAddress { get; set; }
        public OrderPaymentDto PaymentDetails { get; set; }
        public List<EventCartItem> CartItems { get; set; }
    }

    public class EventCartItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}