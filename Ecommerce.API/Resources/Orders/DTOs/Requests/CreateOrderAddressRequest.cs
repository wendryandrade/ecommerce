namespace Ecommerce.API.Resources.Orders.DTOs.Requests
{
    public class CreateOrderAddressRequest
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
    }
}
