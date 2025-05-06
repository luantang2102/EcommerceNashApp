using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface IOrderItemRepository
    {
        IQueryable<OrderItem> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(Guid orderItemId);
        Task<OrderItem> CreateAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(OrderItem orderItem);
    }
}

