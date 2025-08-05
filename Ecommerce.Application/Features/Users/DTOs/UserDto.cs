namespace Ecommerce.Application.Features.Users.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        // A senha (PasswordHash) NUNCA é incluída em um DTO de retorno.
    }
}