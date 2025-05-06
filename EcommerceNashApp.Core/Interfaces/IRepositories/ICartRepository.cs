using EcommerceNashApp.Core.Models;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface ICartRepository
    {
        IQueryable<Cart> GetAllAsync();
        Task<Cart?> GetByIdAsync(Guid cartId);
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart> CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
    }
}