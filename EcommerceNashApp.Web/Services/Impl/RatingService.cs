using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using EcommerceNashApp.Web.Models.Views;
using System.Text;
using System.Text.Json;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class RatingService : IRatingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RatingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RatingService(IHttpClientFactory httpClientFactory, ILogger<RatingService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private ProductRatingView MapRatingResponseToView(RatingResponse ratingResponse)
        {
            return new ProductRatingView
            {
                Id = ratingResponse.Id,
                Value = ratingResponse.Value,
                Comment = ratingResponse.Comment,
                Username = ratingResponse.User?.UserName ?? "Anonymous",
                CreatedDate = ratingResponse.CreatedDate,
                UserId = ratingResponse.User.Id // Map Guid UserId
            };
        }

        public async Task<List<ProductRatingView>> GetProductRatingsAsync(Guid productId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching ratings for product ID {ProductId}", productId);
            try
            {
                var response = await _httpClient.GetAsync($"api/Ratings/product/{productId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<RatingResponse>>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    return apiResponse.Body.Select(MapRatingResponseToView).ToList();
                }
                _logger.LogWarning("No ratings found for product ID {ProductId}", productId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for ratings of product ID {ProductId}", productId);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize ratings response for product ID {ProductId}", productId);
            }
            return new List<ProductRatingView>();
        }

        public async Task<ProductRatingView?> GetUserRatingAsync(Guid productId, Guid userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking rating for user {UserId} on product {ProductId}", userId, productId);
            try
            {
                var ratings = await GetProductRatingsAsync(productId, cancellationToken);
                var userRating = ratings.FirstOrDefault(r => r.UserId == userId);
                if (userRating != null)
                {
                    _logger.LogInformation("User {UserId} has rated product {ProductId}", userId, productId);
                    return userRating;
                }
                _logger.LogInformation("User {UserId} has not rated product {ProductId}", userId, productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user rating for user {UserId} on product {ProductId}", userId, productId);
            }
            return null;
        }

        public async Task<ProductRatingView> CreateRatingAsync(Guid userId, Guid productId, int value, string comment, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating rating for user {UserId} on product {ProductId}", userId, productId);
            var request = new RatingRequest { ProductId = productId, Value = value, Comment = comment };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/Ratings", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RatingResponse>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    _logger.LogInformation("Rating created successfully for user {UserId} on product {ProductId}", userId, productId);
                    return MapRatingResponseToView(apiResponse.Body);
                }
                throw new Exception("Failed to create rating: Empty response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating for user {UserId} on product {ProductId}", userId, productId);
                throw;
            }
        }

        public async Task<ProductRatingView> UpdateRatingAsync(Guid userId, Guid productId, Guid ratingId, int value, string comment, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating rating {RatingId} for user {UserId}", ratingId, userId);
            var request = new RatingRequest { Value = value, Comment = comment, ProductId = productId };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"api/Ratings/{ratingId}", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RatingResponse>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    _logger.LogInformation("Rating {RatingId} updated successfully for user {UserId}", ratingId, userId);
                    return MapRatingResponseToView(apiResponse.Body);
                }
                throw new Exception("Failed to update rating: Empty response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating rating {RatingId} for user {UserId}", ratingId, userId);
                throw;
            }
        }

        public async Task DeleteRatingAsync(Guid userId, Guid ratingId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting rating {RatingId} for user {UserId}", ratingId, userId);
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Ratings/{ratingId}", cancellationToken);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Rating {RatingId} deleted successfully for user {UserId}", ratingId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting rating {RatingId} for user {UserId}", ratingId, userId);
                throw;
            }
        }
    }
}