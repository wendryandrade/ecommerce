using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Guid userId, Guid productId);
    }
}
