using EcommerceNashApp.Api.Controllers.Base;
using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.DTOs.Wrapper;
using EcommerceNashApp.Core.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EcommerceNashApp.Api.Controllers
{
    public class CartController : BaseApiController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
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

        [HttpPost("items")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemRequest request)
        {
            var userId = User.GetUserId();
            var cartItem = await _cartService.AddItemToCartAsync(userId, request.ProductId, request.Quantity);
            return Ok(new ApiResponse<CartItemResponse>(200, "Item added to cart successfully", cartItem));
        }

        [HttpPut("items/{cartItemId}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] CartItemRequest request)
        {
            var cartItem = await _cartService.UpdateCartItemAsync(cartItemId, request.Quantity);
            return Ok(new ApiResponse<CartItemResponse>(200, "Cart item updated successfully", cartItem));
        }

        [HttpDelete("items/{cartItemId}")]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> DeleteCartItem(Guid cartItemId)
        {
            await _cartService.DeleteCartItemAsync(cartItemId);
            return Ok(new ApiResponse<string>(200, "Cart item deleted successfully", "Deleted"));
        }

        [HttpDelete]
        [Authorize(Policy = "RequireUserRole")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.GetUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(new ApiResponse<string>(200, "Cart cleared successfully", "Cleared"));
        }
    }
}