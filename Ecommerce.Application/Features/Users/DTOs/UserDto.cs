namespace Ecommerce.Application.Features.Users.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // A senha (PasswordHash) NUNCA é incluída em um DTO de retorno.
    }
}