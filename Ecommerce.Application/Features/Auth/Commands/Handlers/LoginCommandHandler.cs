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

            // Verificar a senha usando PBKDF2 (mesma lógica do CreateUserCommandHandler)
            
            // Validar se o PasswordHash não é nulo ou vazio
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                if (user.PasswordHash == null)
                    throw new ArgumentNullException(nameof(request), "PasswordHash não pode ser nulo");
                else
                    throw new ArgumentException("PasswordHash não pode estar vazio", nameof(request));
            }
            
            try
            {
                // Extrair salt e hash do PasswordHash armazenado
                byte[] hashBytes = Convert.FromBase64String(user.PasswordHash);
                
                // Os primeiros 32 bytes são o salt
                byte[] salt = new byte[32];
                Array.Copy(hashBytes, 0, salt, 0, 32);
                
                // Os próximos 20 bytes são o hash
                byte[] storedHash = new byte[20];
                Array.Copy(hashBytes, 32, storedHash, 0, 20);
                
                // Calcular hash da senha fornecida com o mesmo salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(request.Password, salt, 100000, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(20);
                    
                    // Comparar os hashes
                    if (!storedHash.SequenceEqual(computedHash))
                    {
                        return null; // Senha incorreta
                    }
                }
            }
            catch (FormatException)
            {
                throw new FormatException("Formato do PasswordHash é inválido");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException)
            {
                throw; // Relançar exceções de argumento
            }
            catch
            {
                return null; // Outros erros de hash inválido
            }

            // Se a senha estiver correta, gerar e retornar o token
            var token = _authService.GenerateJwtToken(user);
            return token;
        }
    }
}