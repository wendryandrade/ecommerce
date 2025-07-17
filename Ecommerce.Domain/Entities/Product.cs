namespace Ecommerce.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; } 
        public int StockQuantity { get; set; }

        public void DecreaseStock(int quantity) => StockQuantity -= quantity;
        public void IncreaseStock(int quantity) => StockQuantity += quantity;
        public void ChangePrice(decimal newPrice) => Price = newPrice;
    }
}
