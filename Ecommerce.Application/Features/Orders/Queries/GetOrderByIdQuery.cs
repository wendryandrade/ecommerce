using Ecommerce.Application.Features.Orders.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries
{
    public class GetOrderByIdQuery : IRequest<OrderDto?>
    {
        public Guid Id { get; set; }
    }
}