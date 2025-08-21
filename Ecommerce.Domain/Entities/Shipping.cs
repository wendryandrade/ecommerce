namespace Ecommerce.Domain.Entities
{
    public class Shipping
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string TrackingCode { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }  
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
