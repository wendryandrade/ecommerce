using Ecommerce.Application.Features.Users.DTOs;
using MediatR;

namespace Ecommerce.Application.Features.Users.Queries
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
    }
}