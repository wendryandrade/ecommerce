using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Infrastructure.UnitTests.Persistence.Context
{
    public class AppDbContextTests
    {
        private static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Constructor_ShouldCreateContextWithValidOptions()
        {
            // Arrange & Act
            using var context = CreateInMemoryContext();

            // Assert
            Assert.NotNull(context);
            Assert.NotNull(context.Products);
            Assert.NotNull(context.Categories);
            Assert.NotNull(context.Users);
            Assert.NotNull(context.Addresses);
            Assert.NotNull(context.Carts);
            Assert.NotNull(context.CartItems);
            Assert.NotNull(context.Orders);
            Assert.NotNull(context.OrderItems);
            Assert.NotNull(context.Payments);
            Assert.NotNull(context.Shippings);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigurePriceColumnsAsDecimal()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Act
            var productEntity = context.Model.FindEntityType(typeof(Product));
            var orderItemEntity = context.Model.FindEntityType(typeof(OrderItem));
            var orderEntity = context.Model.FindEntityType(typeof(Order));
            var paymentEntity = context.Model.FindEntityType(typeof(Payment));
            var shippingEntity = context.Model.FindEntityType(typeof(Shipping));
            var cartItemEntity = context.Model.FindEntityType(typeof(CartItem));

            // Assert - Verify that decimal properties exist and are configured
            Assert.NotNull(productEntity?.FindProperty("Price"));
            Assert.NotNull(orderItemEntity?.FindProperty("UnitPrice"));
            Assert.NotNull(orderEntity?.FindProperty("TotalAmount"));
            Assert.NotNull(paymentEntity?.FindProperty("Amount"));
            Assert.NotNull(shippingEntity?.FindProperty("ShippingCost"));
            Assert.NotNull(cartItemEntity?.FindProperty("UnitPrice"));

            // Verify the properties are of decimal type
            Assert.Equal(typeof(decimal), productEntity?.FindProperty("Price")?.ClrType);
            Assert.Equal(typeof(decimal), orderItemEntity?.FindProperty("UnitPrice")?.ClrType);
            Assert.Equal(typeof(decimal), orderEntity?.FindProperty("TotalAmount")?.ClrType);
            Assert.Equal(typeof(decimal), paymentEntity?.FindProperty("Amount")?.ClrType);
            Assert.Equal(typeof(decimal), shippingEntity?.FindProperty("ShippingCost")?.ClrType);
            Assert.Equal(typeof(decimal), cartItemEntity?.FindProperty("UnitPrice")?.ClrType);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigureOrderShippingAddressRelationship()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Act
            var orderEntity = context.Model.FindEntityType(typeof(Order));
            var navigation = orderEntity?.FindNavigation("ShippingAddress");

            // Assert
            Assert.NotNull(navigation);
            Assert.Equal("ShippingAddressId", navigation.ForeignKey.Properties[0].Name);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigureOrderPaymentOneToOneRelationship()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            // Act
            var paymentEntity = context.Model.FindEntityType(typeof(Payment));
            var orderIdProperty = paymentEntity?.FindProperty("OrderId");

            // Assert
            Assert.NotNull(orderIdProperty);
            Assert.True(orderIdProperty.IsForeignKey());
        }

        [Fact]
        public async Task CanCreateAndSaveProduct()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Test Category",
                Description = "Test Description"
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 10,
                CategoryId = category.Id,
                Category = category
            };

            // Act
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Assert
            var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            Assert.NotNull(savedProduct);
            Assert.Equal("Test Product", savedProduct.Name);
            Assert.Equal(99.99m, savedProduct.Price);
        }

        [Fact]
        public async Task CanCreateAndSaveCompleteOrderFlow()
        {
            // Arrange
            using var context = CreateInMemoryContext();

            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Test Street",
                City = "Test City",
                State = "TS",
                PostalCode = "12345-678"
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ShippingAddressId = address.Id,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 150.00m,
                Status = Domain.Enums.OrderStatus.Pending
            };

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethod = Domain.Enums.PaymentMethod.CreditCard,
                TransactionId = "TXN123456",
                Amount = 150.00m,
                Status = Domain.Enums.PaymentStatus.Paid
            };

            // Act
            context.Addresses.Add(address);
            context.Orders.Add(order);
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            // Assert
            var savedOrder = await context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            Assert.NotNull(savedOrder);
            Assert.Equal(150.00m, savedOrder.TotalAmount);
            Assert.NotNull(savedOrder.ShippingAddress);
            Assert.NotNull(savedOrder.Payment);
            Assert.Equal("TXN123456", savedOrder.Payment.TransactionId);
        }
    }
}
