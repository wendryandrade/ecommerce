using Ecommerce.Application.Features.Carts.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Carts.Queries
{
    public class GetCartByCustomerIdQuery : IRequest<CartDto>
    {
        public Guid CustomerId { get; set; }

        public GetCartByCustomerIdQuery(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}
