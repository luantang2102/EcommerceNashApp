using EcommerceNashApp.Web.Models.DTOs;
using EcommerceNashApp.Web.Models.Views;

namespace EcommerceNashApp.Web.Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IHttpClientFactory httpClient, ILogger<CategoryService> logger)
        {
            _httpClient = httpClient.CreateClient("NashApp.Api");
            _logger = logger;
        }

        public CategoryView MapCategoryDtoToView(CategoryDto categoryDto)
        {
            return new CategoryView
            {
                Id = categoryDto.Id,
                Description = categoryDto.Description,
                Name = categoryDto.Name,
                SubCategories = categoryDto.SubCategories.Select(x => MapCategoryDtoToView(x)).ToList()
            };
        }
        public async Task<List<CategoryView>> GetCategoriesTreeAsync(CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Categories/tree");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            // Fixing the generic type usage and syntax issues
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiDto<List<CategoryDto>>>(cancellationToken);

            if (apiResponse?.Body != null)
            {
                var categoryViews = apiResponse.Body.Select(x => MapCategoryDtoToView(x)).ToList();

                return categoryViews;
            }

            return [];
        }
    }
}
