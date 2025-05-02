using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
