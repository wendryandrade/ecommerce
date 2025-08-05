using Ecommerce.Application.Features.Users.DTOs;
using Ecommerce.Application.Interfaces;
using MediatR;

namespace Ecommerce.Application.Features.Users.Queries.Handlers
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user == null)
            {
                return null;
            }

            // Mapeia a Entidade para o DTO da Application
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
    }
}