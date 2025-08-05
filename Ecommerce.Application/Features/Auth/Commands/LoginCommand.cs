using MediatR;

namespace Ecommerce.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<string> // Retorna o token como uma string
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}