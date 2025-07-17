namespace Ecommerce.API.Resources.Carts.DTOs.Requests
{
    public class DecreaseCartItemQuantityRequest
    {
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
    }
}
