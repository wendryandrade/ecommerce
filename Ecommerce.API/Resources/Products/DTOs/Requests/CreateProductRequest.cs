namespace Ecommerce.API.Resources.Products.DTOs.Requests
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
    }
}
