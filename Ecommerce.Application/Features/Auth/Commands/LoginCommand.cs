using MediatR;

namespace Ecommerce.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<string?> // Retorna o token como uma string
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}