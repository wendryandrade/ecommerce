using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class OrderTests
    {
        [Fact]
        public void Constructor_ShouldCreateOrderWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var shippingAddressId = Guid.NewGuid();
            var orderDate = DateTime.UtcNow;
            var totalAmount = 150.50m;
            var status = OrderStatus.Pending;

            // Act
            var order = new Order
            {
                Id = id,
                UserId = userId,
                ShippingAddressId = shippingAddressId,
                OrderDate = orderDate,
                TotalAmount = totalAmount,
                Status = status
            };

            // Assert
            Assert.Equal(id, order.Id);
            Assert.Equal(userId, order.UserId);
            Assert.Equal(shippingAddressId, order.ShippingAddressId);
            Assert.Equal(orderDate, order.OrderDate);
            Assert.Equal(totalAmount, order.TotalAmount);
            Assert.Equal(status, order.Status);
        }

        [Fact]
        public void Order_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var order = new Order();

            // Assert
            Assert.Equal(Guid.Empty, order.Id);
            Assert.Equal(Guid.Empty, order.UserId);
            Assert.Equal(Guid.Empty, order.ShippingAddressId);
            Assert.Equal(default(DateTime), order.OrderDate);
            Assert.Equal(0m, order.TotalAmount);
            Assert.Equal(default(OrderStatus), order.Status);
            Assert.NotNull(order.OrderItems);
            Assert.Empty(order.OrderItems);
        }

        [Fact]
        public void Order_ShouldAllowAddingOrderItems()
        {
            // Arrange
            var order = new Order();
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                UnitPrice = 25.00m
            };

            // Act
            order.OrderItems.Add(orderItem);

            // Assert
            Assert.Single(order.OrderItems);
            Assert.Equal(orderItem, order.OrderItems.First());
        }

        [Fact]
        public void Order_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var order = new Order();
            var newStatus = OrderStatus.Paid;
            var newTotalAmount = 200.00m;

            // Act
            order.Status = newStatus;
            order.TotalAmount = newTotalAmount;

            // Assert
            Assert.Equal(newStatus, order.Status);
            Assert.Equal(newTotalAmount, order.TotalAmount);
        }

        [Fact]
        public void Order_ShouldHaveShippingAddressReference()
        {
            // Arrange
            var order = new Order();
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Rua das Flores",
                City = "São Paulo",
                State = "SP",
                PostalCode = "01234-567"
            };

            // Act: como a entidade não sincroniza FK automaticamente, definir ambos
            order.ShippingAddressId = address.Id;
            order.ShippingAddress = address;

            // Assert
            Assert.Equal(address, order.ShippingAddress);
            Assert.Equal(address.Id, order.ShippingAddressId);
        }

        [Fact]
        public void Order_ShouldHavePaymentReference()
        {
            // Arrange
            var order = new Order();
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                Amount = 150.00m,
                PaymentMethod = PaymentMethod.CreditCard,
                Status = PaymentStatus.Pending
            };

            // Act
            order.Payment = payment;

            // Assert
            Assert.Equal(payment, order.Payment);
        }

        [Fact]
        public void Order_ShouldHaveShippingReference()
        {
            // Arrange
            var order = new Order();
            var shipping = new Shipping
            {
                Id = Guid.NewGuid(),
                TrackingCode = "TRK123456",
                Carrier = "FedEx"
            };

            // Act
            order.Shipping = shipping;

            // Assert
            Assert.Equal(shipping, order.Shipping);
        }
    }
}
