using Ecommerce.Application.Features.Orders.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Orders.Queries
{
    public class GetOrdersByCustomerIdQuery : IRequest<List<OrderDto>>
    {
        public Guid CustomerId { get; set; }

        public GetOrdersByCustomerIdQuery(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}