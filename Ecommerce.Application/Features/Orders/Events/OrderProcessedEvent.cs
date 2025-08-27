namespace Ecommerce.Application.Features.Orders.Events
{
    public class OrderProcessedEvent
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
