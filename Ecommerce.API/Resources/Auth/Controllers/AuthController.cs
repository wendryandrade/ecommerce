using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Users.DTOs.Requests;
using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            try
            {
                var command = new CreateUserCommand
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password
                };

                var userId = await _mediator.Send(command);

                return Ok(new { UserId = userId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginCommand { Email = request.Email, Password = request.Password }; 
            var token = await _mediator.Send(command);

            if (token == null)
            {
                return Unauthorized("E-mail ou senha inválidos.");
            }

            return Ok(new LoginResponse { Token = token }); // Retornando o DTO de Response
        }
    }
}