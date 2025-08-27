using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class ShippingTests
    {
        [Fact]
        public void Constructor_ShouldCreateShippingWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var trackingCode = "TRK123456789";
            var carrier = "FedEx";
            var estimatedDeliveryDate = DateTime.UtcNow.AddDays(5);

            // Act
            var shipping = new Shipping
            {
                Id = id,
                OrderId = orderId,
                TrackingCode = trackingCode,
                Carrier = carrier,
                EstimatedDeliveryDate = estimatedDeliveryDate
            };

            // Assert
            Assert.Equal(id, shipping.Id);
            Assert.Equal(orderId, shipping.OrderId);
            Assert.Equal(trackingCode, shipping.TrackingCode);
            Assert.Equal(carrier, shipping.Carrier);
            Assert.Equal(estimatedDeliveryDate, shipping.EstimatedDeliveryDate);
        }

        [Fact]
        public void Shipping_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var shipping = new Shipping();

            // Assert
            Assert.Equal(Guid.Empty, shipping.Id);
            Assert.Equal(Guid.Empty, shipping.OrderId);
            Assert.Equal(string.Empty, shipping.TrackingCode);
            Assert.Equal(string.Empty, shipping.Carrier);
            Assert.Equal(default(DateTime), shipping.EstimatedDeliveryDate);
        }

        [Fact]
        public void Shipping_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var shipping = new Shipping();
            var newCarrier = "UPS";
            var newTrackingCode = "TRK987654321";
            var newDeliveryDate = DateTime.UtcNow.AddDays(3);

            // Act
            shipping.Carrier = newCarrier;
            shipping.TrackingCode = newTrackingCode;
            shipping.EstimatedDeliveryDate = newDeliveryDate;

            // Assert
            Assert.Equal(newCarrier, shipping.Carrier);
            Assert.Equal(newTrackingCode, shipping.TrackingCode);
            Assert.Equal(newDeliveryDate, shipping.EstimatedDeliveryDate);
        }

        [Fact]
        public void Shipping_ShouldHandleDifferentCarriers()
        {
            // Arrange
            var shipping = new Shipping();
            var carriers = new[] { "FedEx", "UPS", "DHL", "USPS", "Correios" };

            // Act & Assert
            foreach (var carrier in carriers)
            {
                shipping.Carrier = carrier;
                Assert.Equal(carrier, shipping.Carrier);
            }
        }

        [Fact]
        public void Shipping_ShouldHandleEmptyTrackingCode()
        {
            // Arrange
            var shipping = new Shipping
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                TrackingCode = "",
                Carrier = "FedEx"
            };

            // Act & Assert
            Assert.Equal("", shipping.TrackingCode);
        }

        [Fact]
        public void Shipping_ShouldHandlePastDeliveryDate()
        {
            // Arrange
            var shipping = new Shipping
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                TrackingCode = "TRK123456",
                Carrier = "FedEx",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            Assert.True(shipping.EstimatedDeliveryDate < DateTime.UtcNow);
        }

        [Fact]
        public void Shipping_ShouldHandleFutureDeliveryDate()
        {
            // Arrange
            var shipping = new Shipping
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                TrackingCode = "TRK123456",
                Carrier = "FedEx",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7)
            };

            // Act & Assert
            Assert.True(shipping.EstimatedDeliveryDate > DateTime.UtcNow);
        }
    }
}
