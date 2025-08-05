namespace Ecommerce.Application.Features.Carts.DTOs
{
    public class CartDto
    {
        public Guid UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
