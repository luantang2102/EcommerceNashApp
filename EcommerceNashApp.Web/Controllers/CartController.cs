using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services.Impl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EcommerceNashApp.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _apiService;

        public CartController(CartService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _apiService.GetCartAsync();
            var model = new CartPageView
            {
                Items = cart.CartItems
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            await _apiService.AddItemToCartAsync(productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, int quantity)
        {
            await _apiService.UpdateCartItemAsync(cartItemId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            await _apiService.DeleteCartItemAsync(cartItemId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            await _apiService.ClearCartAsync();
            return RedirectToAction("Index");
        }
    }
}