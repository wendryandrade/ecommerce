using Ecommerce.Application.Interfaces;
using MediatR;
using System.Security.Cryptography;

namespace Ecommerce.Application.Features.Auth.Commands.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string?>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public LoginCommandHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<string?> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Encontrar o usuário pelo e-mail
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return null; // Usuário não encontrado
            }

            // Verificar a senha (usando a mesma lógica do hash nativo)
            byte[] hashBytes = Convert.FromBase64String(user.PasswordHash);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Sonar S2053: o salt deve ser imprevisível no momento da geração do hash.
            // Aqui apenas REUTILIZAMOS o salt já armazenado no hash do usuário para verificar a senha.
            // Justificativa: esta rotina NÃO gera novos hashes; apenas valida credenciais.
            var pbkdf2 = new Rfc2898DeriveBytes(request.Password, salt, 10000, HashAlgorithmName.SHA256); // NOSONAR: Using salt extracted from stored hash for verification, not generating a new hash
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return null; // Senha incorreta
                }
            }

            // Se a senha estiver correta, gerar e retornar o token
            var token = _authService.GenerateJwtToken(user);
            return token;
        }
    }
}