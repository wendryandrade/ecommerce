using MediatR;
using Ecommerce.Application.Products.Dtos;

namespace Ecommerce.Application.Products.Queries
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
    }
}
