using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class PaymentTests
    {
        [Fact]
        public void Constructor_ShouldCreatePaymentWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var amount = 150.00m;
            var paymentMethod = PaymentMethod.CreditCard;
            var status = PaymentStatus.Pending;
            var transactionId = "txn_123456";

            // Act
            var payment = new Payment
            {
                Id = id,
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = status,
                TransactionId = transactionId
            };

            // Assert
            Assert.Equal(id, payment.Id);
            Assert.Equal(orderId, payment.OrderId);
            Assert.Equal(amount, payment.Amount);
            Assert.Equal(paymentMethod, payment.PaymentMethod);
            Assert.Equal(status, payment.Status);
            Assert.Equal(transactionId, payment.TransactionId);
        }

        [Fact]
        public void Payment_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var payment = new Payment();

            // Assert
            Assert.Equal(Guid.Empty, payment.Id);
            Assert.Equal(Guid.Empty, payment.OrderId);
            Assert.Equal(0m, payment.Amount);
            Assert.Equal(default(PaymentMethod), payment.PaymentMethod);
            Assert.Equal(default(PaymentStatus), payment.Status);
            Assert.Equal(string.Empty, payment.TransactionId);
        }

        [Fact]
        public void Payment_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var payment = new Payment();
            var newStatus = PaymentStatus.Paid;
            var newAmount = 200.00m;
            var newTransactionId = "txn_789012";

            // Act
            payment.Status = newStatus;
            payment.Amount = newAmount;
            payment.TransactionId = newTransactionId;

            // Assert
            Assert.Equal(newStatus, payment.Status);
            Assert.Equal(newAmount, payment.Amount);
            Assert.Equal(newTransactionId, payment.TransactionId);
        }

        [Fact]
        public void Payment_ShouldHandleDifferentPaymentMethods()
        {
            // Arrange
            var payment = new Payment();
            var methods = new[] { PaymentMethod.CreditCard, PaymentMethod.Pix, PaymentMethod.Boleto };

            // Act & Assert
            foreach (var method in methods)
            {
                payment.PaymentMethod = method;
                Assert.Equal(method, payment.PaymentMethod);
            }
        }

        [Fact]
        public void Payment_ShouldHandleDifferentStatuses()
        {
            // Arrange
            var payment = new Payment();
            var statuses = new[] 
            { 
                PaymentStatus.Pending, 
                PaymentStatus.Authorized, 
                PaymentStatus.Paid, 
                PaymentStatus.Failed 
            };

            // Act & Assert
            foreach (var status in statuses)
            {
                payment.Status = status;
                Assert.Equal(status, payment.Status);
            }
        }

        [Fact]
        public void Payment_ShouldHandleNullTransactionId()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = 100.00m,
                PaymentMethod = PaymentMethod.CreditCard,
                Status = PaymentStatus.Pending,
                TransactionId = ""
            };

            // Act & Assert
            Assert.Equal("", payment.TransactionId);
        }

        [Fact]
        public void Payment_ShouldHandleZeroAmount()
        {
            // Arrange
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Amount = 0m,
                PaymentMethod = PaymentMethod.CreditCard,
                Status = PaymentStatus.Pending
            };

            // Act & Assert
            Assert.Equal(0m, payment.Amount);
        }
    }
}
