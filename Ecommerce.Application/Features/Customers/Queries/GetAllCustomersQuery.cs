using Ecommerce.Application.Features.Customers.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Customers.Queries
{
    public class GetAllCustomersQuery : IRequest<List<CustomerDto>>
    {
    }
}