using Ecommerce.Application.Features.Customers.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Customers.Queries
{
    public class GetCustomerByEmailQuery : IRequest<CustomerDto?>
    {
        public string Email { get; set; }
    }
}