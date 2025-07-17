using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; } 
        public PaymentStatus Status { get; set; }
    }
}
