using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken cancellationToken);
        Task UpdateAsync(Order order); 
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}