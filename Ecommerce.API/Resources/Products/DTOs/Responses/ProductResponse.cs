namespace Ecommerce.API.Resources.Products.DTOs.Responses
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = default!;


        /*
        Você recebe na API um CreateProductRequest ou UpdateProductRequest

        Mapeia para CreateProductDto ou UpdateProductDto da Application para enviar ao handler

        O handler retorna um ProductDto

        Você mapeia ProductDto para ProductResponse e retorna para o cliente
        */
    }
}
