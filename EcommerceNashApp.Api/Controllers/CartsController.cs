using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceNashApp.Api.Controllers
{
    public class CartsController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.GetUserId();
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(new ApiResponse<CartResponse>(200, "Cart retrieved successfully", cart));
        }

        [HttpPost("me/items")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemRequest request)
        {
            var userId = User.GetUserId();
            var cartItem = await _cartService.AddItemToCartAsync(userId, request.ProductId, request.Quantity);
            return Ok(new ApiResponse<CartItemResponse>(200, "Item added to cart successfully", cartItem));
        }

        [HttpPut("me/items/{cartItemId}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] CartItemRequest request)
        {
            var cartItem = await _cartService.UpdateCartItemAsync(cartItemId, request.Quantity);
            return Ok(new ApiResponse<CartItemResponse>(200, "Cart item updated successfully", cartItem));
        }

        [HttpDelete("me/items/{cartItemId}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> DeleteCartItem(Guid cartItemId)
        {
            await _cartService.DeleteCartItemAsync(cartItemId);
            return Ok(new ApiResponse<string>(200, "Cart item deleted successfully", "Deleted"));
        }

        [HttpDelete("me")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.GetUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(new ApiResponse<string>(200, "Cart cleared successfully", "Cleared"));
        }
    }
}