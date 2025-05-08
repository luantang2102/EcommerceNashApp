using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Models.Views;
using EcommerceNashApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EcommerceNashApp.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        private bool IsAuthenticated()
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            var isAuthenticated = !string.IsNullOrEmpty(userInfoJson);
            _logger.LogDebug("Checked authentication state: {IsAuthenticated}", isAuthenticated);
            return isAuthenticated;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
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
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cart");
                TempData["Error"] = "Không thể tải giỏ hàng. Vui lòng thử lại sau.";
                return View(new CartPageView { Items = [] });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, returning Unauthorized for AddItem");
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng." });
            }

            try
            {
                _logger.LogInformation("Adding item to cart: ProductId={ProductId}, Quantity={Quantity}, Size={Size}",
                    request.ProductId, request.Quantity, request.Size);
                var cartItem = await _cartService.AddItemToCartAsync(Guid.Parse(request.ProductId), request.Quantity);
                return Ok(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng." });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("stock"))
            {
                _logger.LogWarning("Insufficient stock for product {ProductId}, Quantity={Quantity}", request.ProductId, request.Quantity);
                return BadRequest(new { success = false, message = "Số lượng yêu cầu vượt quá tồn kho.", errorCode = "InsufficientStock" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding to cart: {Error}", ex.Message);
                return StatusCode(500, new { success = false, message = "Không thể thêm sản phẩm vào giỏ hàng." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng." });
                }
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "ProductDetails", new { id = productId }) });
            }

            try
            {
                _logger.LogInformation("Adding item to cart: ProductId={ProductId}, Quantity={Quantity}", productId, quantity);
                await _cartService.AddItemToCartAsync(productId, quantity);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng." });
                }
                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("stock"))
            {
                _logger.LogWarning("Insufficient stock for product {ProductId}, Quantity={Quantity}", productId, quantity);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { success = false, message = "Số lượng yêu cầu vượt quá tồn kho.", errorCode = "InsufficientStock" });
                }
                TempData["Error"] = "Số lượng yêu cầu vượt quá tồn kho.";
                return RedirectToAction("Index", "ProductDetails", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, new { success = false, message = "Không thể thêm sản phẩm vào giỏ hàng." });
                }
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Index", "ProductDetails", new { id = productId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(Guid cartItemId, int quantity)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }

            if (quantity <= 0)
            {
                _logger.LogInformation("Quantity ({Quantity}) is 0 or negative, removing cart item: CartItemId={CartItemId}", quantity, cartItemId);
                try
                {
                    await _cartService.DeleteCartItemAsync(cartItemId);
                    TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing cart item: CartItemId={CartItemId}", cartItemId);
                    TempData["Error"] = "Không thể xóa sản phẩm khỏi giỏ hàng. Vui lòng thử lại.";
                }
                return RedirectToAction("Index");
            }

            try
            {
                _logger.LogInformation("Updating cart item: CartItemId={CartItemId}, Quantity={Quantity}", cartItemId, quantity);
                await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                TempData["Success"] = "Đã cập nhật giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access in UpdateCartItemAsync: {Message}", ex.Message);
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                TempData["Error"] = "Không thể cập nhật giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCartItem(Guid cartItemId)
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }

            try
            {
                _logger.LogInformation("Removing cart item: CartItemId={CartItemId}", cartItemId);
                await _cartService.DeleteCartItemAsync(cartItemId);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access in DeleteCartItemAsync: {Message}", ex.Message);
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item");
                TempData["Error"] = "Không thể xóa sản phẩm khỏi giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            if (!IsAuthenticated())
            {
                _logger.LogWarning("User not authenticated, redirecting to login");
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }

            try
            {
                _logger.LogInformation("Clearing cart");
                await _cartService.ClearCartAsync();
                TempData["Success"] = "Đã xóa toàn bộ giỏ hàng.";
                return RedirectToAction("Index");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized access in ClearCartAsync: {Message}", ex.Message);
                return RedirectToAction("Index", "Login", new { returnUrl = Url.Action("Index", "Cart") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["Error"] = "Không thể xóa giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }
    }

    public class AddToCartRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
    }
}