namespace Ecommerce.Domain.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public List<CartItem> CartItems { get; set; } = new();

        public void AddItem(Product product, int quantity)
        {
            ArgumentNullException.ThrowIfNull(product);
            if (quantity <= 0) throw new ArgumentException("A quantidade deve ser positiva");

            var existingItem = CartItems.Find(ci => ci.ProductId == product.Id);
            if (existingItem != null)
                existingItem.Quantity += quantity;
            else
                CartItems.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    CartId = this.Id
                });
        }

        public void RemoveItem(Guid productId)
        {
            CartItems.RemoveAll(ci => ci.ProductId == productId);
        }

        public void UpdateQuantity(Guid productId, int quantity)
        {
            var item = CartItems.Find(ci => ci.ProductId == productId);
            if (item == null) return;
            if (quantity <= 0) RemoveItem(productId);
            else item.Quantity = quantity;
        }
        public void Clear()
        {
            CartItems.Clear();
        }
    }
}
