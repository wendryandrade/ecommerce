using Ecommerce.Application.Features.Customers.DTOs;
using Ecommerce.Application.Features.Customers.Queries;
using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Customers.Queries.Handlers
{
    public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);

            if (customer == null)
            {
                return null;
            }

            // Mapeia a Entidade para o DTO da Application
            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };
        }
    }
}