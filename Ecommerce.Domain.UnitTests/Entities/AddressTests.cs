using Ecommerce.Domain.Entities;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class AddressTests
    {
        [Fact]
        public void Constructor_ShouldCreateAddressWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var street = "Rua das Flores";
            var city = "São Paulo";
            var state = "SP";
            var postalCode = "01234-567";

            // Act
            var address = new Address
            {
                Id = id,
                Street = street,
                City = city,
                State = state,
                PostalCode = postalCode
            };

            // Assert
            Assert.Equal(id, address.Id);
            Assert.Equal(street, address.Street);
            Assert.Equal(city, address.City);
            Assert.Equal(state, address.State);
            Assert.Equal(postalCode, address.PostalCode);
        }

        [Fact]
        public void Address_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var address = new Address();

            // Assert
            Assert.Equal(Guid.Empty, address.Id);
            Assert.Equal(string.Empty, address.Street);
            Assert.Equal(string.Empty, address.City);
            Assert.Equal(string.Empty, address.State);
            Assert.Equal(string.Empty, address.PostalCode);
        }

        [Fact]
        public void Address_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var address = new Address();
            var newStreet = "Avenida Paulista";
            var newCity = "Rio de Janeiro";
            var newState = "RJ";
            var newPostal = "20040-030";

            // Act
            address.Street = newStreet;
            address.City = newCity;
            address.State = newState;
            address.PostalCode = newPostal;

            // Assert
            Assert.Equal(newStreet, address.Street);
            Assert.Equal(newCity, address.City);
            Assert.Equal(newState, address.State);
            Assert.Equal(newPostal, address.PostalCode);
        }

        [Fact]
        public void Address_ShouldHandleEmptyStrings()
        {
            // Arrange
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Street = "",
                City = "",
                State = "",
                PostalCode = ""
            };

            // Act & Assert
            Assert.Equal("", address.Street);
            Assert.Equal("", address.City);
            Assert.Equal("", address.State);
            Assert.Equal("", address.PostalCode);
        }
    }
}
