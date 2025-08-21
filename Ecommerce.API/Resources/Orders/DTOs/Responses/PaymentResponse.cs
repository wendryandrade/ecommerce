namespace Ecommerce.API.Resources.Payments.DTOs.Responses
{
    public class PaymentResponse
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }
}