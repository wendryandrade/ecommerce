using Ecommerce.Application.Features.Carts.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Carts.Queries
{
    public class GetCartByUserIdQuery : IRequest<CartDto?>
    {
        public Guid UserId { get; set; }

        public GetCartByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
