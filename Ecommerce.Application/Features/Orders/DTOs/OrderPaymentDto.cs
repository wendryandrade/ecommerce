using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Features.Orders.DTOs
{
    public class OrderPaymentDto
    {
        public PaymentMethod PaymentMethod { get; set; }
    }
}