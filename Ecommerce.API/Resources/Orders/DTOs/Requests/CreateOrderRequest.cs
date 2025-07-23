namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public CreateOrderAddressRequest ShippingAddress { get; set; }
        public CreateOrderPaymentRequest PaymentDetails { get; set; }
    }
}
