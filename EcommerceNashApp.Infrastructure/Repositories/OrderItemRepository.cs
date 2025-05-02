using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<OrderItem> GetAllAsync()
        {
            return _context.OrderItems
                .Include(x => x.Product)
                .Include(x => x.Order);
        }

        public async Task<OrderItem?> GetByIdAsync(Guid orderItemId)
        {
            return await _context.OrderItems
                .Include(x => x.Product)
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == orderItemId);
        }

        public async Task<OrderItem> CreateAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            return orderItem;
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(OrderItem orderItem)
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
    }
}