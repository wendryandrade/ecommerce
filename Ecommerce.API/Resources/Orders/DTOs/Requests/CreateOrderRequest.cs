namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    public class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public CreateOrderAddressRequest ShippingAddress { get; set; } = new();
        public CreateOrderPaymentRequest PaymentDetails { get; set; } = new();
    }
}
