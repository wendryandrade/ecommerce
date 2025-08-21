using Ecommerce.Application.Products.Dtos;

namespace Ecommerce.Application.Features.Categories.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CategoryProductDto> Products { get; set; } = new();
    }
}
