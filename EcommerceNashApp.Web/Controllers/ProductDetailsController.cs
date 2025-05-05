using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.Views;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using EcommerceNashApp.Web.Services.Impl;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Services;

namespace EcommerceNashApp.Web.Controllers
{
    public class ProductDetailsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IRatingService _ratingService;
        private readonly ILogger<ProductDetailsController> _logger;

        public ProductDetailsController(
            IProductService productService,
            IRatingService ratingService,
            ILogger<ProductDetailsController> logger)
        {
            _productService = productService;
            _ratingService = ratingService;
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] Guid id, CancellationToken cancellationToken)
        {
            // Fetch product details
            var product = await _productService.GetProductByIdAsync(id, cancellationToken);
            if (product == null)
            {
                _logger.LogWarning("Product not found for ID {ProductId}", id);
                return NotFound();
            }

            // Fetch related products
            var relatedProducts = await _productService.GetProductsAsync(
                new PaginationParams { PageNumber = 1, PageSize = 10 },
                cancellationToken
            );

            // Fetch ratings
            var ratings = await _ratingService.GetProductRatingsAsync(id, cancellationToken);

            // Check if the user has rated (based on session UserInfo)
            ProductRatingView? userRating = null;
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (!string.IsNullOrEmpty(userInfoJson))
            {
                try
                {
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                    if (userInfo != null && userInfo.UserId != Guid.Empty)
                    {
                        userRating = await _ratingService.GetUserRatingAsync(id, userInfo.UserId, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError("Invalid UserId in session UserInfo: {UserId}", userInfo?.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing UserInfo from session");
                }
            }

            var model = new ProductDetailsView
            {
                Product = product,
                RelatedProducts = relatedProducts.ToList(),
                Ratings = ratings,
                UserRating = userRating,
                IsLoggedIn = !string.IsNullOrEmpty(userInfoJson) // Session-based authentication
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRating(Guid productId, Guid userId, int rating, string comment, CancellationToken cancellationToken)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                _logger.LogWarning("User not authenticated (no UserInfo in session), redirecting to login");
                return Redirect($"{Url.Action("Index", "Login")}?returnUrl={Url.Action("Index", "ProductDetails")}?id={productId}");
            }

            try
            {
                // Validate userId against session
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                if (userInfo == null || userInfo.UserId == Guid.Empty || userInfo.UserId != userId)
                {
                    _logger.LogError("Invalid or mismatched UserId: Form {FormUserId}, Session {SessionUserId}", userId, userInfo?.UserId);
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Validate rating
                if (rating < 1 || rating > 5)
                {
                    _logger.LogWarning("Invalid rating value {Rating} for product {ProductId}", rating, productId);
                    TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }
                if (string.IsNullOrWhiteSpace(comment))
                {
                    _logger.LogWarning("Comment is empty for product {ProductId}", productId);
                    TempData["Error"] = "Vui lòng nhập bình luận.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Check if user already rated
                var existingRating = await _ratingService.GetUserRatingAsync(productId, userId, cancellationToken);
                if (existingRating != null)
                {
                    _logger.LogWarning("User {UserId} already rated product {ProductId}", userId, productId);
                    TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Create rating
                await _ratingService.CreateRatingAsync(userId, productId, rating, comment, cancellationToken);
                _logger.LogInformation("Rating added by user {UserId} for product {ProductId}", userId, productId);
                TempData["Success"] = "Đánh giá đã được gửi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding rating for product {ProductId} by user {UserId}", productId, userId);
                TempData["Error"] = "Không thể gửi đánh giá. Vui lòng thử lại.";
            }

            return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRating(Guid productId, Guid ratingId, Guid userId, int rating, string comment, CancellationToken cancellationToken)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                _logger.LogWarning("User not authenticated (no UserInfo in session), redirecting to login");
                return Redirect($"{Url.Action("Index", "Login")}?returnUrl={Url.Action("Index", "ProductDetails")}?id={productId}");
            }

            try
            {
                // Validate userId against session
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                if (userInfo == null || userInfo.UserId == Guid.Empty || userInfo.UserId != userId)
                {
                    _logger.LogError("Invalid or mismatched UserId: Form {FormUserId}, Session {SessionUserId}", userId, userInfo?.UserId);
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Validate rating
                if (rating < 1 || rating > 5)
                {
                    _logger.LogWarning("Invalid rating value {Rating} for product {ProductId}", rating, productId);
                    TempData["Error"] = "Đánh giá phải từ 1 đến 5 sao.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }
                if (string.IsNullOrWhiteSpace(comment))
                {
                    _logger.LogWarning("Comment is empty for product {ProductId}", productId);
                    TempData["Error"] = "Vui lòng nhập bình luận.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Update rating
                await _ratingService.UpdateRatingAsync(userId, productId, ratingId, rating, comment, cancellationToken);
                _logger.LogInformation("Rating {RatingId} updated by user {UserId} for product {ProductId}", ratingId, userId, productId);
                TempData["Success"] = "Đánh giá đã được cập nhật.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating {RatingId} for product {ProductId} by user {UserId}", ratingId, productId, userId);
                TempData["Error"] = "Không thể cập nhật đánh giá. Vui lòng thử lại.";
            }

            return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRating(Guid productId, Guid ratingId, Guid userId, CancellationToken cancellationToken)
        {
            var userInfoJson = HttpContext.Session.GetString("UserInfo");
            if (string.IsNullOrEmpty(userInfoJson))
            {
                _logger.LogWarning("User not authenticated (no UserInfo in session), redirecting to login");
                return Redirect($"{Url.Action("Index", "Login")}?returnUrl={Url.Action("Index", "ProductDetails")}?id={productId}");
            }

            try
            {
                // Validate userId against session
                var userInfo = JsonSerializer.Deserialize<UserInfo>(userInfoJson);
                if (userInfo == null || userInfo.UserId == Guid.Empty || userInfo.UserId != userId)
                {
                    _logger.LogError("Invalid or mismatched UserId: Form {FormUserId}, Session {SessionUserId}", userId, userInfo?.UserId);
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
                }

                // Delete rating
                await _ratingService.DeleteRatingAsync(userId, ratingId, cancellationToken);
                _logger.LogInformation("Rating {RatingId} deleted by user {UserId} for product {ProductId}", ratingId, userId, productId);
                TempData["Success"] = "Đánh giá đã được xóa.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating {RatingId} for product {ProductId} by user {UserId}", ratingId, productId, userId);
                TempData["Error"] = "Không thể xóa đánh giá. Vui lòng thử lại.";
            }

            return Redirect($"{Url.Action("Index", "ProductDetails")}?id={productId}");
        }
    }
}