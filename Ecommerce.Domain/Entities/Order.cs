using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }  
        public OrderStatus Status { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }
        public Shipping Shipping { get; set; }
    }
}
