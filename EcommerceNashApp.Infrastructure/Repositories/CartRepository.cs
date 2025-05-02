using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Cart> GetAllAsync()
        {
            return _context.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product);
        }

        public async Task<Cart?> GetByIdAsync(Guid cartId)
        {
            return await _context.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == cartId);
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Cart> CreateAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cart cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
    }
}