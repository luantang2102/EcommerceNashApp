using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Core.Interfaces.IServices
{
    public interface ICartService
    {
        Task<CartItemResponse> AddItemToCartAsync(Guid userId, Guid productId, int quantity);
        Task ClearCartAsync(Guid userId);
        Task DeleteCartItemAsync(Guid cartItemId);
        Task<CartResponse> GetCartByUserIdAsync(Guid userId);
        Task<CartItemResponse> UpdateCartItemAsync(Guid cartItemId, int quantity);
    }
}
