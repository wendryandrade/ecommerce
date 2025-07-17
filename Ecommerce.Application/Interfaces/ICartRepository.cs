using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetByCustomerIdAsync(Guid customerId);
        Task AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Guid customerId, Guid productId);
    }
}
