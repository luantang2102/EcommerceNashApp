using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;

namespace EcommerceNashApp.Core.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedList<CategoryResponse>> GetCategoriesAsync(CategoryParams categoryParams);
        Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId);
        Task<List<CategoryResponse>> GetCategoriesByIdsAsync(List<Guid> categoryIds);
        Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest);
        Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CategoryRequest categoryRequest);
        Task<bool> DeleteCategoryAsync(Guid categoryId);
        Task<List<CategoryResponse>> GetCategoriesTreeAsync();
    }
}
