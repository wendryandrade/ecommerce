using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
    }
}