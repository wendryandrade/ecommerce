using Ecommerce.API.Resources.Customers.DTOs.Requests;
using Ecommerce.API.Resources.Customers.DTOs.Responses;
using Ecommerce.Application.Features.Customers.Commands;
using Ecommerce.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Resources.Customers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var command = new CreateCustomerCommand
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password
                };

                var customerId = await _mediator.Send(command);

                // Retorna o ID do novo cliente para confirmação
                return Ok(new { CustomerId = customerId });
            }
            catch (InvalidOperationException ex)
            {
                // Retorna um erro 400 se o e-mail já existir
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetCustomerByIdQuery { Id = id };
            var customerDto = await _mediator.Send(query);

            if (customerDto == null)
            {
                return NotFound();
            }

            // Mapeia o DTO da Application para o Response da API
            var response = new CustomerResponse
            {
                Id = customerDto.Id,
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email
            };

            return Ok(response);
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var query = new GetCustomerByEmailQuery { Email = email };
            var customerDto = await _mediator.Send(query);

            if (customerDto == null)
            {
                return NotFound();
            }

            // Mapeia o DTO da Application para o Response da API
            var response = new CustomerResponse
            {
                Id = customerDto.Id,
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCustomersQuery();
            var customerDtos = await _mediator.Send(query);

            var response = customerDtos.Select(dto => new CustomerResponse
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