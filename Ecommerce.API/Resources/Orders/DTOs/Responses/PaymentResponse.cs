namespace Ecommerce.API.Resources.Payments.DTOs.Responses
{
    public class PaymentResponse
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
    }
}