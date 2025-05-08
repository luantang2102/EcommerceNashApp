using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        public CartService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private bool HasValidJwtToken()
        {
            if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("auth_jwt", out var jwtToken))
            {
                _logger.LogWarning("No auth_jwt cookie found");
                return false;
            }

            if (!jwtToken.Contains("."))
            {
                _logger.LogError("JWT is malformed: {JwtToken}", jwtToken.Substring(0, Math.Min(20, jwtToken.Length)) + "...");
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);
                var exp = token.ValidTo;

                if (exp < DateTime.UtcNow)
                {
                    _logger.LogWarning("JWT is expired: ValidUntil={ValidUntil}, Now={Now}", exp, DateTime.UtcNow);
                    return false;
                }

                _logger.LogDebug("JWT is valid until: {Expiration}", exp.ToString("o"));
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse JWT token");
                return false;
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            if (!_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("refresh", out var refreshToken))
            {
                _logger.LogWarning("No refresh token cookie found, cannot refresh token");
                return false;
            }

            try
            {
                _logger.LogInformation("Attempting to refresh token");
                var response = await _httpClient.GetAsync("/api/Auth/refresh-token");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Token refreshed successfully");
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Refresh token failed: StatusCode={StatusCode}, Response={Content}",
                    response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during token refresh");
                return false;
            }
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action, string operationName)
        {
            if (!HasValidJwtToken())
            {
                _logger.LogInformation("No valid JWT token found for {Operation}, attempting refresh", operationName);

                if (await RefreshTokenAsync())
                {
                    if (!HasValidJwtToken())
                    {
                        _logger.LogError("Still no valid JWT token after refresh for {Operation}", operationName);
                        throw new UnauthorizedAccessException("Authentication failed");
                    }
                }
                else
                {
                    _logger.LogError("Failed to refresh token for {Operation}", operationName);
                    throw new UnauthorizedAccessException("Authentication failed");
                }
            }

            HttpResponseMessage response;
            try
            {
                response = await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during {Operation}", operationName);
                throw;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("{Operation} returned 401 Unauthorized, attempting token refresh", operationName);

                if (await RefreshTokenAsync() && HasValidJwtToken())
                {
                    _logger.LogInformation("Token refreshed, retrying {Operation}", operationName);

                    try
                    {
                        response = await action();

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            _logger.LogError("Still unauthorized after token refresh for {Operation}", operationName);
                            throw new UnauthorizedAccessException("Authentication failed after token refresh");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception during retry of {Operation} after token refresh", operationName);
                        throw;
                    }
                }
                else
                {
                    _logger.LogError("Failed to refresh token for {Operation}", operationName);
                    throw new UnauthorizedAccessException("Authentication failed");
                }
            }

            return response;
        }

        public async Task<CartResponse> GetCartAsync()
        {
            _logger.LogInformation("Fetching cart");

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.GetAsync("/api/Carts");
            }, "GetCartAsync");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CartResponse>>(content);

            _logger.LogDebug("Cart fetched successfully: {CartId}, {ItemCount} items",
                apiResponse.Body.Id, apiResponse.Body.CartItems?.Count ?? 0);

            return apiResponse.Body;
        }

        public async Task<CartItemResponse> AddItemToCartAsync(Guid productId, int quantity)
        {
            _logger.LogInformation("Adding item to cart: ProductId={ProductId}, Quantity={Quantity}", productId, quantity);

            var request = new CartItemRequest { ProductId = productId, Quantity = quantity };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.PostAsync("/api/Carts/me/items", content);
            }, "AddItemToCartAsync");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CartItemResponse>>(responseContent);

            _logger.LogDebug("Item added to cart: CartItemId={CartItemId}, ProductId={ProductId}, Quantity={Quantity}",
                apiResponse.Body.Id, apiResponse.Body.ProductId, apiResponse.Body.Quantity);

            return apiResponse.Body;
        }

        public async Task<CartItemResponse> UpdateCartItemAsync(Guid cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                _logger.LogWarning("Invalid quantity ({Quantity}) for cart item: CartItemId={CartItemId}", quantity, cartItemId);
                throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
            }

            _logger.LogInformation("Updating cart item: CartItemId={CartItemId}, Quantity={Quantity}", cartItemId, quantity);

            var request = new CartItemRequest { Quantity = quantity };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.PutAsync($"/api/Carts/me/items/{cartItemId}", content);
            }, "UpdateCartItemAsync");

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<CartItemResponse>>(responseContent);

            _logger.LogDebug("Cart item updated: CartItemId={CartItemId}, Quantity={Quantity}",
                apiResponse.Body.Id, apiResponse.Body.Quantity);

            return apiResponse.Body;
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            _logger.LogInformation("Deleting cart item: CartItemId={CartItemId}", cartItemId);

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.DeleteAsync($"/api/Carts/me/items/{cartItemId}");
            }, "DeleteCartItemAsync");

            response.EnsureSuccessStatusCode();

            _logger.LogDebug("Cart item deleted: CartItemId={CartItemId}", cartItemId);
        }

        public async Task ClearCartAsync()
        {
            _logger.LogInformation("Clearing cart");

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.DeleteAsync("/api/Carts/me");
            }, "ClearCartAsync");

            response.EnsureSuccessStatusCode();

            _logger.LogDebug("Cart cleared successfully");
        }

        public async Task<ShippingAddressRequest> GetSavedAddressAsync()
        {
            _logger.LogInformation("Fetching saved address");

            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.GetAsync("/api/UserProfile/address");
            }, "GetSavedAddressAsync");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("No saved address found");
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<ShippingAddressRequest>>(content);

            _logger.LogDebug("Saved address fetched successfully");

            return apiResponse.Body;
        }
    }
}