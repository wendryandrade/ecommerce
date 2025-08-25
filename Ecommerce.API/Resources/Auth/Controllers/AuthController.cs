using Ecommerce.API.Resources.Auth.DTOs.Requests;
using Ecommerce.API.Resources.Auth.DTOs.Responses;
using Ecommerce.API.Resources.Users.DTOs.Requests;
using Ecommerce.Application.Features.Auth.Commands;
using Ecommerce.Application.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.API.Common;

namespace Ecommerce.API.Resources.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")] 
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

                Response.AddSuccessMessage("Usuario registrado com sucesso.");
                return CreatedAtRoute("GetUserById", new { id = userId }, new { id = userId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid operation",
                    Detail = ex.Message
                });
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
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid credentials",
                    Detail = "E-mail ou senha inválidos."
                });
            }

            Response.AddSuccessMessage("Login realizado com sucesso.");
            return Ok(new LoginResponse { Token = token });
        }
    }
}