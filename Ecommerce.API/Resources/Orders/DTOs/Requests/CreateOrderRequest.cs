namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    using System.Text.Json.Serialization;

    public class CreateOrderRequest
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public CreateOrderAddressRequest ShippingAddress { get; set; } = new();
        public CreateOrderPaymentRequest PaymentDetails { get; set; } = new();
    }
}
