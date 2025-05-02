using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Order> GetAllAsync()
        {
            return _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .Include(x => x.UserProfile);
        }

        public async Task<Order?> GetByIdAsync(Guid orderId)
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .Include(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.Id == orderId);
        }

        public async Task<Order?> GetByUserProfileIdAsync(Guid userProfileId)
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Product)
                .Include(x => x.UserProfile)
                .FirstOrDefaultAsync(x => x.UserProfileId == userProfileId);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Order order)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }
}