using Ecommerce.API.Resources.Addresses.DTOs.Responses;
using Ecommerce.API.Resources.Payments.DTOs.Responses;

namespace Ecommerce.API.Resources.Orders.DTOs.Responses
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public PaymentResponse Payment { get; set; }
        public AddressResponse ShippingAddress { get; set; }
    }
}