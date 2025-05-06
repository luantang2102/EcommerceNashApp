using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using System;
using System.Threading.Tasks;

namespace EcommerceNashApp.Web.Services
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync();
        Task<CartItemResponse> AddItemToCartAsync(Guid productId, int quantity);
        Task<CartItemResponse> UpdateCartItemAsync(Guid cartItemId, int quantity);
        Task DeleteCartItemAsync(Guid cartItemId);
        Task ClearCartAsync();
        Task<ShippingAddressRequest> GetSavedAddressAsync();
    }
}