using Ecommerce.Application.Features.Users.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public Guid Id { get; set; }
    }
}