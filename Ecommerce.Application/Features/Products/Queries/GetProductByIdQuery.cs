using MediatR;
using Ecommerce.Application.Products.Dtos;

namespace Ecommerce.Application.Products.Queries
{
    public class GetProductByIdQuery : IRequest<ProductDto?>
    {
        public Guid Id { get; set; }
        public GetProductByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}