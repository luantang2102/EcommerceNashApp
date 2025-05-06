using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class CartItemRepository : ICartItemRepository
    {
        private readonly AppDbContext _context;

        public CartItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<CartItem> GetAllAsync()
        {
            return _context.CartItems
                .Include(x => x.Product)
                .Include(x => x.Cart);
        }

        public async Task<CartItem?> GetByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(x => x.Product)
                .Include(x => x.Cart)
                .FirstOrDefaultAsync(x => x.Id == cartItemId);
        }

        public async Task<CartItem> CreateAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task UpdateAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }
}