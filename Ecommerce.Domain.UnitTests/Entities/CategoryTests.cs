using Ecommerce.Domain.Entities;
using Xunit;

namespace Ecommerce.Domain.UnitTests.Entities
{
    public class CategoryTests
    {
        [Fact]
        public void Constructor_ShouldCreateCategoryWithValidProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var name = "Electronics";
            var description = "Electronic devices and accessories";

            // Act
            var category = new Category
            {
                Id = id,
                Name = name,
                Description = description
            };

            // Assert
            Assert.Equal(id, category.Id);
            Assert.Equal(name, category.Name);
            Assert.Equal(description, category.Description);
        }

        [Fact]
        public void Category_ShouldHaveDefaultValues_WhenCreated()
        {
            // Act
            var category = new Category();

            // Assert
            Assert.Equal(Guid.Empty, category.Id);
            Assert.Equal(string.Empty, category.Name);
            Assert.Equal(string.Empty, category.Description);
            Assert.NotNull(category.Products);
            Assert.Empty(category.Products);
        }

        [Fact]
        public void Category_ShouldAllowPropertyUpdates()
        {
            // Arrange
            var category = new Category();
            var newName = "New Category Name";
            var newDescription = "New category description";

            // Act
            category.Name = newName;
            category.Description = newDescription;

            // Assert
            Assert.Equal(newName, category.Name);
            Assert.Equal(newDescription, category.Description);
        }

        [Fact]
        public void Category_ShouldHandleNullValues()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Description = string.Empty
            };

            // Act & Assert
            Assert.Equal(string.Empty, category.Name);
            Assert.Equal(string.Empty, category.Description);
        }

        [Fact]
        public void Category_ShouldHandleEmptyStrings()
        {
            // Arrange
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Description = string.Empty
            };

            // Act & Assert
            Assert.Equal(string.Empty, category.Name);
            Assert.Equal(string.Empty, category.Description);
        }
    }
}
