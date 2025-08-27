using Ecommerce.Domain.Entities;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class ProductTests
    {
        [Fact]
        // Deveria reduzir a quantidade de estoque pelo valor especificado
        public void DecreaseStock_ShouldReduceStockQuantityByGivenAmount()
        {
            // Arrange
            var product = new Product { StockQuantity = 100 };
            var amountToDecrease = 10;

            // Act
            product.DecreaseStock(amountToDecrease);

            // Assert
            Assert.Equal(90, product.StockQuantity);
        }

        [Fact]
        // Deveria aumentar a quantidade de estoque pelo valor especificado
        public void IncreaseStock_ShouldIncreaseStockQuantityByGivenAmount()
        {
            // Arrange
            var product = new Product { StockQuantity = 100 };
            var amountToIncrease = 25;

            // Act
            product.IncreaseStock(amountToIncrease);

            // Assert
            Assert.Equal(125, product.StockQuantity);
        }

        [Fact]
        // Deveria atualizar o preço do produto para o novo valor
        public void ChangePrice_ShouldUpdateProductPriceToNewValue()
        {
            // Arrange
            var product = new Product { Price = 150.00m };
            var newPrice = 175.50m;

            // Act
            product.ChangePrice(newPrice);

            // Assert
            Assert.Equal(newPrice, product.Price);
        }

        [Fact]
        public void DecreaseStock_ShouldDecrease_ByQuantity()
        {
            var p = new Product { StockQuantity = 10 };
            p.DecreaseStock(3);
            Assert.Equal(7, p.StockQuantity);
        }

        [Fact]
        public void IncreaseStock_ShouldIncrease_ByQuantity()
        {
            var p = new Product { StockQuantity = 1 };
            p.IncreaseStock(4);
            Assert.Equal(5, p.StockQuantity);
        }

        [Fact]
        public void ChangePrice_ShouldSet_NewPrice()
        {
            var p = new Product { Price = 5m };
            p.ChangePrice(12.34m);
            Assert.Equal(12.34m, p.Price);
        }
    }
}