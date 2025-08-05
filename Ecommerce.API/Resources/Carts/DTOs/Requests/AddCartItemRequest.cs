namespace Ecommerce.API.Resources.Carts.DTOs.Requests
{
    public class AddCartItemRequest
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
