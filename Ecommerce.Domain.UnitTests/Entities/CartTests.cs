using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class CartTests
    {
        [Fact]
        // Deveria adicionar um novo item ao carrinho quando o produto não estiver presente
        public void AddItem_ShouldAddNewItem_WhenProductIsNotInCart()
        {
            // Arrange (Preparação)
            var cart = new Cart();
            var product = new Product { Id = Guid.NewGuid(), Price = 150.00m };

            // Act (Ação)
            cart.AddItem(product, 2);

            // Assert (Verificação)
            Assert.Single(cart.CartItems); // Deve haver apenas 1 tipo de item no carrinho
            Assert.Equal(2, cart.CartItems.First().Quantity); // A quantidade desse item deve ser 2
        }

        [Fact]
        // Deveria aumentar a quantidade do item existente no carrinho quando o produto já estiver presente
        public void AddItem_ShouldIncreaseQuantity_WhenProductIsAlreadyInCart()
        {
            // Arrange
            var cart = new Cart();
            var product = new Product { Id = Guid.NewGuid(), Price = 150.00m };

            // Adiciona o produto uma primeira vez
            cart.AddItem(product, 1);

            // Act
            // Adiciona o MESMO produto uma segunda vez
            cart.AddItem(product, 3);

            // Assert
            Assert.Single(cart.CartItems); // Continua a haver apenas 1 tipo de item no carrinho
            Assert.Equal(4, cart.CartItems.First().Quantity); // A quantidade total deve ser 1 + 3 = 4
        }

        [Fact]
        // Deveria remover um item do carrinho quando o produto estiver presente
        public void RemoveItem_ShouldRemoveProductFromCart()
        {
            // Arrange
            var cart = new Cart();
            var product1 = new Product { Id = Guid.NewGuid() };
            var product2 = new Product { Id = Guid.NewGuid() };
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 1);

            // Act
            cart.RemoveItem(product1.Id);

            // Assert
            Assert.Single(cart.CartItems); // Agora deve haver apenas 1 item
            Assert.Equal(product2.Id, cart.CartItems.First().ProductId); // O item que sobrou deve ser o product2
        }

        [Fact]
        // Deveria atualizar a quantidade de um item no carrinho quando a quantidade for positiva
        public void UpdateQuantity_ShouldChangeItemQuantity_WhenQuantityIsPositive()
        {
            // Arrange
            var cart = new Cart();
            var product = new Product { Id = Guid.NewGuid(), Price = 100 };
            cart.AddItem(product, 2);

            // Act
            cart.UpdateQuantity(product.Id, 5);

            // Assert
            Assert.Equal(5, cart.CartItems.First().Quantity);
        }

        [Fact]
        // Deveria atualizar a quantidade de um item no carrinho para zero ou menos, removendo o item do carrinho
        public void UpdateQuantity_ShouldRemoveItem_WhenQuantityIsZeroOrLess()
        {
            // Arrange
            var cart = new Cart();
            var product = new Product { Id = Guid.NewGuid(), Price = 100 };
            cart.AddItem(product, 2);

            // Act
            cart.UpdateQuantity(product.Id, 0); // Testando com zero

            // Assert
            Assert.Empty(cart.CartItems); // O item deve ser removido
        }

        [Fact]
        // Deveria limpar todos os itens do carrinho, deixando-o vazio
        public void Clear_ShouldRemoveAllItemsFromCart()
        {
            // Arrange
            var cart = new Cart();
            var product1 = new Product { Id = Guid.NewGuid() };
            var product2 = new Product { Id = Guid.NewGuid() };
            cart.AddItem(product1, 1);
            cart.AddItem(product2, 2);

            // Act
            cart.Clear();

            // Assert
            Assert.Empty(cart.CartItems); // A coleção de itens deve estar vazia
        }
    }
}