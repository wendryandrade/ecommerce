using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Features.Payments.DTOs
{
    public class PaymentDto
    {
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}