using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            // Busca a chave secreta do appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Cria as credenciais para assinar o token
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Adiciona as "claims" - informações que queremos guardar dentro do token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // ID do usuário
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // Inclui a Role no token
                new Claim(ClaimTypes.Role, user.Role) 
            };

            // Cria o token com todas as informações
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Define a validade do token (ex: 2 horas)
                signingCredentials: credentials);

            // Escreve o token como uma string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}