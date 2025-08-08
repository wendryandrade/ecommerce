using Ecommerce.Domain.Entities;

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
    }
}