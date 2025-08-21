using Ecommerce.Application.Features.Users.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Users.Queries
{
    public class GetUserByEmailQuery : IRequest<UserDto?>
    {
        public string Email { get; set; } = string.Empty;
    }
}