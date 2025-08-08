using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class UserTests
    {
        [Fact]
        // Deveria adicionar um novo endereço à coleção de endereços do usuário
        public void AddAddress_ShouldAddAddressToTheAddressesCollection()
        {
            // Arrange (Preparação)
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "João",
                // A coleção de endereços começa vazia
                Addresses = new System.Collections.Generic.List<Address>()
            };

            var newAddress = new Address
            {
                Id = Guid.NewGuid(),
                Street = "Rua Nova",
                City = "Lisboa"
            };

            // Act (Ação)
            user.AddAddress(newAddress);

            // Assert (Verificação)
            Assert.Single(user.Addresses); // A coleção deve agora ter 1 endereço
            Assert.Equal("Rua Nova", user.Addresses.First().Street); // Verifica se o endereço adicionado é o correto
        }
    }
}