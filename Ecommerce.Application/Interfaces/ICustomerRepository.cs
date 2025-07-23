using Ecommerce.Domain.Entities;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer, CancellationToken cancellationToken);
        Task<Customer?> GetByEmailAsync(string email);
        Task<Customer?> GetByIdAsync(Guid id);
        Task<List<Customer>> GetAllAsync();
    }
}