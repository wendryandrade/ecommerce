using Ecommerce.Application.Products.Dtos;
using MediatR;

namespace Ecommerce.Application.Products.Commands
{
    public class UpdateProductCommand : IRequest<ProductDto?>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryId { get; set; }
    }
}
