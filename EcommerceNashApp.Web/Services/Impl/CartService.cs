using EcommerceNashApp.Web.Models;
using EcommerceNashApp.Web.Models.DTOs;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EcommerceNashApp.Web.Models.DTOs.Request;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class CartService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CartService> _logger;

        public CartService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("jwt", out var jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                _logger.LogDebug("Added Authorization header with JWT: {JwtToken}", jwtToken.Substring(0, 10) + "...");
            }
            else
            {
                _logger.LogWarning("No jwt cookie found");
            }
        }

        private void AddCsrfHeader()
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("csrf", out var csrfToken))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
                _httpClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrfToken);
                _logger.LogDebug("Added X-CSRF-TOKEN header: {CsrfToken}", csrfToken);
            }
            else
            {
                _logger.LogWarning("No csrf cookie found");
            }
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var client = _httpClientFactory.CreateClient("NashApp.Api");
            _logger.LogInformation("Calling /api/Auth/refresh-token");
            var response = await client.GetAsync("/api/Auth/refresh-token");
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Refresh token successful");
                return true;
            }
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Refresh token failed: {StatusCode}, {Content}", response.StatusCode, content);
            return false;
        }

        private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action)
        {
            AddAuthorizationHeader();
            var response = await action();
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Received 401 Unauthorized, attempting to refresh token");
                if (await RefreshTokenAsync())
                {
                    _logger.LogInformation("Retrying API call with new jwt token");
                    AddAuthorizationHeader();
                    response = await action();
                }
                else
                {
                    _logger.LogError("Token refresh failed, cannot retry API call");
                }
            }
            return response;
        }

        public async Task<CartDto> GetCartAsync()
        {
            _logger.LogInformation("Fetching cart");
            var response = await ExecuteWithRetryAsync(async () =>
            {
                return await _httpClient.GetAsync("/api/Cart");
            });
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiDto<CartDto>>(content);
            _logger.LogDebug("Cart fetched: {CartId}", apiResponse.Body.Id);
            return apiResponse.Body;
        }

        public async Task<CartItemDto> AddItemToCartAsync(Guid productId, int quantity)
        {
            _logger.LogInformation("Adding item to cart: ProductId={ProductId}, Quantity={Quantity}", productId, quantity);
            var response = await ExecuteWithRetryAsync(async () =>
            {
                AddCsrfHeader();
                var request = new CartItemRequest { ProductId = productId, Quantity = quantity };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync("/api/Cart/items", content);
            });
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiDto<CartItemDto>>(responseContent);
            _logger.LogDebug("Item added: {CartItemId}", apiResponse.Body.Id);
            return apiResponse.Body;
        }

        public async Task<CartItemDto> UpdateCartItemAsync(Guid cartItemId, int quantity)
        {
            _logger.LogInformation("Updating cart item: CartItemId={CartItemId}, Quantity={Quantity}", cartItemId, quantity);
            var response = await ExecuteWithRetryAsync(async () =>
            {
                AddCsrfHeader();
                var request = new CartItemRequest { Quantity = quantity };
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                return await _httpClient.PutAsync($"/api/Cart/items/{cartItemId}", content);
            });
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiDto<CartItemDto>>(responseContent);
            _logger.LogDebug("Item updated: {CartItemId}", apiResponse.Body.Id);
            return apiResponse.Body;
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            _logger.LogInformation("Deleting cart item: CartItemId={CartItemId}", cartItemId);
            var response = await ExecuteWithRetryAsync(async () =>
            {
                AddCsrfHeader();
                return await _httpClient.DeleteAsync($"/api/Cart/items/{cartItemId}");
            });
            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Item deleted: {CartItemId}", cartItemId);
        }

        public async Task ClearCartAsync()
        {
            _logger.LogInformation("Clearing cart");
            var response = await ExecuteWithRetryAsync(async () =>
            {
                AddCsrfHeader();
                return await _httpClient.DeleteAsync("/api/Cart");
            });
            response.EnsureSuccessStatusCode();
            _logger.LogDebug("Cart cleared");
        }

        public async Task<string> CreateOrUpdatePaymentIntentAsync()
        {
            _logger.LogInformation("Creating or updating payment intent");
            var response = await ExecuteWithRetryAsync(async () =>
            {
                AddCsrfHeader();
                return await _httpClient.PostAsync("/api/Payment/intent", null);
            });
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiDto<string>>(content);
            _logger.LogDebug("Payment intent created: {PaymentIntentId}", apiResponse.Body);
            return apiResponse.Body;
        }
    }
}