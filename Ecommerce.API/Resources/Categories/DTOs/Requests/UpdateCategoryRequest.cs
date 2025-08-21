namespace Ecommerce.API.Resources.Categories.DTOs.Requests
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}