using Ecommerce.API.Resources.Products.DTOs.Responses;

namespace Ecommerce.API.Resources.Categories.DTOs.Responses
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<CategoryProductResponse> Products { get; set; }
    }

}
