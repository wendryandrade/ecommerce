using Ecommerce.API.Resources.Addresses.DTOs.Responses;
using Ecommerce.API.Resources.Payments.DTOs.Responses;

namespace Ecommerce.API.Resources.Orders.DTOs.Responses
{
    public class ShippingResponse
    {
        public decimal ShippingCost { get; set; }
        public int DeliveryDays { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
    }
}
