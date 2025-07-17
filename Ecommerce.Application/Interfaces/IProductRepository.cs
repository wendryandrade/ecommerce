using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(CancellationToken cancellationToken);
        Task<Product?> GetByIdAsync(Guid id);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
    }
}
