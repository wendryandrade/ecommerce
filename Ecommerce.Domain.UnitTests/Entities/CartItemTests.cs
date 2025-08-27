using Ecommerce.Domain.Entities;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class CartItemTests
    {
        [Fact]
        public void Constructor_ShouldCreateCartItemWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cartId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 3;
            var unitPrice = 29.99m;
            var product = new Product { Id = productId, Name = "Test Product", Price = unitPrice };

            // Act
            var cartItem = new CartItem
            {
                Id = id,
                CartId = cartId,
                ProductId = productId,
                Product = product,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            // Assert
            Assert.Equal(id, cartItem.Id);
            Assert.Equal(cartId, cartItem.CartId);
            Assert.Equal(productId, cartItem.ProductId);
            Assert.Equal(product, cartItem.Product);
            Assert.Equal(quantity, cartItem.Quantity);
            Assert.Equal(unitPrice, cartItem.UnitPrice);
        }

        [Fact]
        public void CartItem_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var cartItem = new CartItem();

            // Assert
            Assert.Equal(Guid.Empty, cartItem.Id);
            Assert.Equal(Guid.Empty, cartItem.CartId);
            Assert.Equal(Guid.Empty, cartItem.ProductId);
            Assert.Null(cartItem.Product);
            Assert.Equal(0, cartItem.Quantity);
            Assert.Equal(0, cartItem.UnitPrice);
        }

        [Fact]
        public void CartItem_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var cartItem = new CartItem();
            var newQuantity = 5;
            var newUnitPrice = 49.99m;
            var newProduct = new Product { Id = Guid.NewGuid(), Name = "Updated Product" };

            // Act
            cartItem.Quantity = newQuantity;
            cartItem.UnitPrice = newUnitPrice;
            cartItem.Product = newProduct;

            // Assert
            Assert.Equal(newQuantity, cartItem.Quantity);
            Assert.Equal(newUnitPrice, cartItem.UnitPrice);
            Assert.Equal(newProduct, cartItem.Product);
        }

        [Fact]
        public void CartItem_ShouldHandleZeroQuantity()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 0,
                UnitPrice = 10.00m
            };

            // Act & Assert
            Assert.Equal(0, cartItem.Quantity);
        }

        [Fact]
        public void CartItem_ShouldHandleNegativeQuantity()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = -1,
                UnitPrice = 10.00m
            };

            // Act & Assert
            Assert.Equal(-1, cartItem.Quantity);
        }

        [Fact]
        public void CartItem_ShouldHandleLargeQuantity()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 999999,
                UnitPrice = 1.00m
            };

            // Act & Assert
            Assert.Equal(999999, cartItem.Quantity);
        }

        [Fact]
        public void CartItem_ShouldHandleZeroUnitPrice()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 0.00m
            };

            // Act & Assert
            Assert.Equal(0.00m, cartItem.UnitPrice);
        }

        [Fact]
        public void CartItem_ShouldHandleNegativeUnitPrice()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = -10.00m
            };

            // Act & Assert
            Assert.Equal(-10.00m, cartItem.UnitPrice);
        }

        [Fact]
        public void CartItem_ShouldHandleHighPrecisionPrices()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 123.456789m
            };

            // Act & Assert
            Assert.Equal(123.456789m, cartItem.UnitPrice);
        }

        [Fact]
        public void CartItem_ShouldHandleNullProduct()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 10.00m,
                Product = null
            };

            // Act & Assert
            Assert.Null(cartItem.Product);
        }

        [Fact]
        public void CartItem_ShouldCalculateTotalPrice()
        {
            // Arrange
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 3,
                UnitPrice = 25.50m
            };

            // Act
            var totalPrice = cartItem.Quantity * cartItem.UnitPrice;

            // Assert
            Assert.Equal(76.50m, totalPrice);
        }
    }
}
