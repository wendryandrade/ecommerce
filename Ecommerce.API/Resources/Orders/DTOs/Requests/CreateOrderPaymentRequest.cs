using Ecommerce.Domain.Enums;

namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    public class CreateOrderPaymentRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
    }
}
