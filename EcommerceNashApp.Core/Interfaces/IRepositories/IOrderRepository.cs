using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface IOrderRepository
    {
        IQueryable<Order> GetAllAsync();
        Task<Order?> GetByIdAsync(Guid orderId);
        Task<Order?> GetByUserProfileIdAsync(Guid userProfileId);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
    }
}
