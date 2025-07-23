using Ecommerce.Application.Features.Customers.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Customers.Queries
{
    public class GetCustomerByIdQuery : IRequest<CustomerDto?>
    {
        public Guid Id { get; set; }
    }
}