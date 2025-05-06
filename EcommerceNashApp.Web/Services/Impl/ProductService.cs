using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.DTOs.Wrapper;
using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Web.Models.Views;
using System.Text.Json;
using System.Web;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient; private readonly ILogger _logger;

        public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("NashApp.Api");
            _logger = logger;
        }

        public ProductView MapProductResponseToView(ProductResponse productResponse)
        {
            return new ProductView
            {
                Id = productResponse.Id,
                Name = productResponse.Name,
                Description = productResponse.Description,
                Price = productResponse.Price,
                StockQuantity = productResponse.StockQuantity,
                AverageRating = productResponse.AverageRating,
                ProductImages = productResponse.ProductImages?.Select(MapProductImageResponseToView).ToList() ?? new List<ProductImageView>()
            };
        }

        public ProductImageView MapProductImageResponseToView(ProductImageResponse productImageResponse)
        {
            return new ProductImageView
            {
                Id = productImageResponse.Id,
                ImageUrl = productImageResponse.ImageUrl,
                PublicId = productImageResponse.PublicId,
                IsMain = productImageResponse.IsMain
            };
        }

        public ProductRatingView MapRatingResponseToView(RatingResponse ratingResponse)
        {
            return new ProductRatingView
            {
                Id = ratingResponse.Id,
                Value = ratingResponse.Value,
                Comment = ratingResponse.Comment,
                Username = ratingResponse.User.UserName ?? "Anonymous",
                CreatedDate = ratingResponse.CreatedDate
            };
        }

        public async Task<PagedList<ProductView>> GetProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken)
        {
            var queryString = $"pageNumber={paginationParams.PageNumber}&pageSize={paginationParams.PageSize}";
            return await FetchProductsAsync($"api/Products?{queryString}", paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        }

        public async Task<PagedList<ProductView>> GetFeaturedProductsAsync(PaginationParams paginationParams, CancellationToken cancellationToken)
        {
            var queryString = $"pageNumber={paginationParams.PageNumber}&pageSize={paginationParams.PageSize}&isFeatured=true";
            return await FetchProductsAsync($"api/Products?{queryString}", paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        }

        public async Task<PagedList<ProductView>> GetFilteredProductsAsync(
            string? categories = null,
            string? minPrice = null,
            string? maxPrice = null,
            string? orderBy = null,
            string? searchTerm = null,
            string? ratings = null,
            int pageNumber = 1,
            int pageSize = 12,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new Dictionary<string, string>
        {
            { "PageNumber", pageNumber.ToString() },
            { "PageSize", pageSize.ToString() }
        };
            if (!string.IsNullOrEmpty(categories)) queryParams.Add("Categories", categories);
            if (!string.IsNullOrEmpty(minPrice)) queryParams.Add("MinPrice", minPrice);
            if (!string.IsNullOrEmpty(maxPrice)) queryParams.Add("MaxPrice", maxPrice);
            if (!string.IsNullOrEmpty(orderBy)) queryParams.Add("OrderBy", orderBy);
            if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add("SearchTerm", searchTerm);
            if (!string.IsNullOrEmpty(ratings)) queryParams.Add("Ratings", ratings);

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));
            return await FetchProductsAsync($"api/Products?{queryString}", pageNumber, pageSize, cancellationToken);
        }

        public async Task<ProductView?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching product with ID {ProductId}", id);
            try
            {
                var response = await _httpClient.GetAsync($"api/Products/{id}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(cancellationToken);
                if (apiResponse?.Body != null)
                {
                    return MapProductResponseToView(apiResponse.Body);
                }
                _logger.LogWarning("No product found for ID {ProductId}", id);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for product ID {ProductId}", id);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize product response for ID {ProductId}", id);
            }
            return null;
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
            return [];
        }

        private async Task<PagedList<ProductView>> FetchProductsAsync(string requestUri, int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching products from {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                response.EnsureSuccessStatusCode();

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductResponse>>>(cancellationToken);

                // Try both "Pagination" and "pagination" headers
                var paginationHeader = response.Headers.Contains("Pagination")
                    ? response.Headers.GetValues("Pagination").FirstOrDefault()
                    : response.Headers.Contains("pagination")
                        ? response.Headers.GetValues("pagination").FirstOrDefault()
                        : null;

                _logger.LogInformation("Pagination Header: {Header}", paginationHeader ?? "null");

                if (apiResponse?.Body != null && paginationHeader != null)
                {
                    try
                    {
                        var pagination = JsonSerializer.Deserialize<PaginationHeader>(
                            paginationHeader,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                        );
                        if (pagination != null)
                        {
                            var productViews = apiResponse.Body.Select(MapProductResponseToView).ToList();
                            var result = new PagedList<ProductView>(
                                productViews,
                                pagination.TotalCount,
                                pagination.CurrentPage,
                                pagination.PageSize
                            );
                            return result;
                        }
                        _logger.LogWarning("Deserialized pagination is null for header: {Header}", paginationHeader);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize pagination header: {Header}", paginationHeader);
                    }
                }
                else
                {
                    _logger.LogWarning("No pagination header or empty response body for {RequestUri}", requestUri);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed for {RequestUri}", requestUri);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize product response for {RequestUri}", requestUri);
            }
            _logger.LogWarning("Returning empty product list for {RequestUri}", requestUri);
            return new PagedList<ProductView>([], 0, pageNumber, pageSize);
        }
    }

}