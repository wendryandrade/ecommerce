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

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .AsTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Cart cart)
        {
            // Garante que o Cart está sendo rastreado como modificado
            _context.Entry(cart).State = EntityState.Modified;

            // Carrega itens atuais do banco para detectar remoções
            var existingItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();

            // Itens removidos (existem no banco, mas não estão na lista atual do carrinho)
            var removedItems = existingItems
                .Where(dbItem => !cart.CartItems.Any(ci => ci.Id == dbItem.Id))
                .ToList();
            if (removedItems.Count > 0)
            {
                _context.CartItems.RemoveRange(removedItems);
            }

            // Marca os itens do carrinho como 'Added' se forem novos ou 'Modified' se já existirem
            foreach (var item in cart.CartItems)
            {
                if (item.Id == Guid.Empty || !existingItems.Any(ci => ci.Id == item.Id))
                {
                    // Garante que o CartId está correto ao adicionar novo item
                    item.CartId = cart.Id;
                    _context.Entry(item).State = EntityState.Added;
                }
                else
                {
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId, Guid productId)
        {
            // Remove o item específico do carrinho do usuário no banco
            var cart = await _context.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return;

            var item = await _context.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
            if (item == null) return;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
