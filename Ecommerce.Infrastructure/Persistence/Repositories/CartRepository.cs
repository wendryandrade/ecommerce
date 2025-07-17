using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetByCustomerIdAsync(Guid customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            // Marca apenas o Cart como modificado
            _context.Entry(cart).State = EntityState.Modified;

            // Marca os itens do carrinho como 'Added' se forem novos
            foreach (var item in cart.CartItems)
            {
                if (!_context.CartItems.Any(ci => ci.Id == item.Id))
                {
                    _context.Entry(item).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid customerId, Guid productId)
        {
            var cart = await GetByCustomerIdAsync(customerId);
            if (cart == null) return;

            cart.RemoveItem(productId);
            await _context.SaveChangesAsync();
        }
    }
}
