using EcommerceNashApp.Web.Models.DTOs;

namespace EcommerceNashApp.Web.Services
{
    public interface ICartService
    {
        Task<CartItemDto> AddItemToCartAsync(Guid productId, int quantity);
        Task ClearCartAsync();
        Task<string> CreateOrUpdatePaymentIntentAsync();
        Task DeleteCartItemAsync(Guid cartItemId);
        Task<CartDto> GetCartAsync();
        Task<CartItemDto> UpdateCartItemAsync(Guid cartItemId, int quantity);
    }
}
