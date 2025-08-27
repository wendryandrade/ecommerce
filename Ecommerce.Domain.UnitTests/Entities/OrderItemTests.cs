using Ecommerce.Domain.Entities;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class OrderItemTests
    {
        [Fact]
        public void Constructor_ShouldCreateOrderItemWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var quantity = 2;
            var unitPrice = 45.75m;

            // Act
            var orderItem = new OrderItem
            {
                Id = id,
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            // Assert
            Assert.Equal(id, orderItem.Id);
            Assert.Equal(orderId, orderItem.OrderId);
            Assert.Equal(productId, orderItem.ProductId);
            Assert.Equal(quantity, orderItem.Quantity);
            Assert.Equal(unitPrice, orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var orderItem = new OrderItem();

            // Assert
            Assert.Equal(Guid.Empty, orderItem.Id);
            Assert.Equal(Guid.Empty, orderItem.OrderId);
            Assert.Equal(Guid.Empty, orderItem.ProductId);
            Assert.Equal(0, orderItem.Quantity);
            Assert.Equal(0m, orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var orderItem = new OrderItem();
            var newQuantity = 4;
            var newUnitPrice = 55.25m;

            // Act
            orderItem.Quantity = newQuantity;
            orderItem.UnitPrice = newUnitPrice;

            // Assert
            Assert.Equal(newQuantity, orderItem.Quantity);
            Assert.Equal(newUnitPrice, orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldCalculateTotalPrice()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 3,
                UnitPrice = 29.99m
            };

            // Act
            var totalPrice = orderItem.Quantity * orderItem.UnitPrice;

            // Assert
            Assert.Equal(89.97m, totalPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleZeroQuantity()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 0,
                UnitPrice = 15.00m
            };

            // Act & Assert
            Assert.Equal(0, orderItem.Quantity);
            Assert.Equal(0m, orderItem.Quantity * orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleZeroUnitPrice()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 5,
                UnitPrice = 0m
            };

            // Act & Assert
            Assert.Equal(0m, orderItem.UnitPrice);
            Assert.Equal(0m, orderItem.Quantity * orderItem.UnitPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleLargeQuantities()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 500,
                UnitPrice = 2.50m
            };

            // Act
            var totalPrice = orderItem.Quantity * orderItem.UnitPrice;

            // Assert
            Assert.Equal(1250.00m, totalPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleDecimalUnitPrices()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 99.99m
            };

            // Act
            var totalPrice = orderItem.Quantity * orderItem.UnitPrice;

            // Assert
            Assert.Equal(99.99m, totalPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleNegativeQuantities()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = -2,
                UnitPrice = 10.00m
            };

            // Act
            var totalPrice = orderItem.Quantity * orderItem.UnitPrice;

            // Assert
            Assert.Equal(-20.00m, totalPrice);
        }

        [Fact]
        public void OrderItem_ShouldHandleNegativeUnitPrices()
        {
            // Arrange
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                UnitPrice = -5.00m
            };

            // Act
            var totalPrice = orderItem.Quantity * orderItem.UnitPrice;

            // Assert
            Assert.Equal(-10.00m, totalPrice);
        }
    }
}
