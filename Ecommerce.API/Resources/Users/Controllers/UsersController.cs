using Ecommerce.API.Resources.Users.DTOs.Requests;
using Ecommerce.API.Resources.Users.DTOs.Responses;
using Ecommerce.Application.Features.Users.Commands;
using Ecommerce.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var userDto = await _mediator.Send(query);

            if (userDto == null)
            {
                return NotFound();
            }

            // Mapeia o DTO da Application para o Response da API
            var response = new UserResponse
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email
            };

            return Ok(response);
        }

        // GET api/users/by-email?email={email}
        [HttpGet("by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var query = new GetUserByEmailQuery { Email = email };
            var userDto = await _mediator.Send(query);

            if (userDto == null)
            {
                return NotFound();
            }

            // Mapeia o DTO da Application para o Response da API
            var response = new UserResponse
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email
            };

            return Ok(response);
        }

        // GET: api/users - Protegido para Admin
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllUsersQuery();
            var userDtos = await _mediator.Send(query);

            var response = userDtos.Select(dto => new UserResponse
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            }).ToList();

            return Ok(response);
        }
    }
}