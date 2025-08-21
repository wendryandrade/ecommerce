using Ecommerce.API.Resources.Products.DTOs.Responses;

namespace Ecommerce.API.Resources.Categories.DTOs.Responses
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CategoryProductResponse> Products { get; set; } = new();
    }

}
