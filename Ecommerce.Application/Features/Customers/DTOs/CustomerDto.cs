using System;

namespace Ecommerce.Application.Features.Customers.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        // A senha (PasswordHash) NUNCA é incluída em um DTO de retorno.
    }
}