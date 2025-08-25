namespace Ecommerce.API.Resources.Carts.DTOs.Requests
{
    public class AddCartItemAdminRequest
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}




