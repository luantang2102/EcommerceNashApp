using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcommerceNashApp.Web.Controllers.Components
{
    public class CartModalViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartModalViewComponent(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
            }

            try
            {
                _logger.LogInformation("Fetching cart for user");
                var cart = await _cartService.GetCartAsync();
                var model = new CartPageView
                {
                    Items = cart.CartItems
                };
                return View(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access in GetCartAsync: {Message}", ex.Message);
                return View(new CartPageView { Items = [] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cart");
                TempData["Error"] = "Không thể tải giỏ hàng. Vui lòng thử lại sau.";
                return View(new CartPageView { Items = [] });
            }
        }

        private bool IsAuthenticated()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            var isAuthenticated = !string.IsNullOrEmpty(userInfoJson);
            _logger.LogDebug("Checked authentication state: {IsAuthenticated}", isAuthenticated);
            return isAuthenticated;
        }
    }
}