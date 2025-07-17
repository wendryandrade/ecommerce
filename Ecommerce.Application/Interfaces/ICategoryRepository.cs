using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);
        Task<Category?> GetByIdAsync(Guid id);
        Task AddAsync(Category category, CancellationToken cancellationToken);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
    }
}
