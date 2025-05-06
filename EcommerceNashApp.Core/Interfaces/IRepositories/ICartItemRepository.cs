using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Core.Interfaces.IRepositories
{
    public interface ICartItemRepository
    {
        IQueryable<CartItem> GetAllAsync();
        Task<CartItem?> GetByIdAsync(Guid cartItemId);
        Task<CartItem> CreateAsync(CartItem cartItem);
        Task UpdateAsync(CartItem cartItem);
        Task DeleteAsync(CartItem cartItem);
    }
}
