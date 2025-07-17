namespace Ecommerce.API.Resources.Carts.DTOs.Responses
{
    public class CartResponse
    {
        public Guid CustomerId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Total);
    }
}
