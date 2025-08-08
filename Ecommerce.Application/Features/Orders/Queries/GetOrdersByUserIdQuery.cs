using Ecommerce.Application.Features.Orders.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries
{
    public class GetOrdersByUserIdQuery : IRequest<List<OrderDto>>
    {
        public Guid UserId { get; set; }

        public GetOrdersByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}