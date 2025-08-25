namespace Ecommerce.Application.Features.Orders.DTOs
{
    public class ShippingDto
    {
        public decimal ShippingCost { get; set; }
        public int DeliveryDays { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
