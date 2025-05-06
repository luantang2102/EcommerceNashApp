using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<PagedList<CategoryResponse>> GetCategoriesAsync(CategoryParams categoryParams)
        {
            var query = _categoryRepository.GetAllAsync();

            if (categoryParams.ParentCategoryId.HasValue)
            {
                var parentExists = await _categoryRepository.ParentCategoryExistsAsync(categoryParams.ParentCategoryId.Value);
                if (!parentExists)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "parentCategoryId", categoryParams.ParentCategoryId.Value }
                    };
                    throw new AppException(ErrorCode.PARENT_CATEGORY_NOT_FOUND, attributes);
                }
                query = query.Where(c => c.ParentCategoryId == categoryParams.ParentCategoryId);
            }

            query = query
                .Sort(categoryParams.OrderBy)
                .Search(categoryParams.SearchTerm)
                .Filter(categoryParams.Level);

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await PagedList<CategoryResponse>.ToPagedList(
                projectedQuery,
                categoryParams.PageNumber,
                categoryParams.PageSize
            );
        }

        public async Task<List<CategoryResponse>> GetCategoriesTreeAsync()
        {
            var rootCategories = await _categoryRepository.GetRootCategoriesAsync().ToListAsync();
            return rootCategories.Select(c => MapCategoryToResponse(c)).ToList();
        }

        private CategoryResponse MapCategoryToResponse(Category category)
        {
            var response = category.MapModelToResponse();
            response.ParentCategoryName = category.ParentCategory?.Name;
            response.SubCategories = category.SubCategories?.Select(sc => MapCategoryToResponse(sc)).ToList() ?? new List<CategoryResponse>();
            return response;
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            return category.MapModelToResponse();
        }

        public async Task<List<CategoryResponse>> GetCategoriesByIdsAsync(List<Guid> categoryIds)
        {
            var categories = await _categoryRepository.GetByIdsAsync(categoryIds);
            if (categories.Count == 0)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryIds", string.Join(", ", categoryIds) }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            return categories.Select(c => c.MapModelToResponse()).ToList();
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest)
        {
            int level = 0;

            if (categoryRequest.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(categoryRequest.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "parentCategoryId", categoryRequest.ParentCategoryId.Value }
                    };
                    throw new AppException(ErrorCode.PARENT_CATEGORY_NOT_FOUND, attributes);
                }

                level = parentCategory.Level + 1;
            }

            var category = new Category
            {
                Name = categoryRequest.Name,
                Description = categoryRequest.Description,
                IsActive = categoryRequest.IsActive,
                ParentCategoryId = categoryRequest.ParentCategoryId,
                Level = level
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);
            return createdCategory.MapModelToResponse();
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CategoryRequest categoryRequest)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            if (categoryRequest.ParentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(categoryRequest.ParentCategoryId.Value);
                if (parentCategory == null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "parentCategoryId", categoryRequest.ParentCategoryId.Value }
                    };
                    throw new AppException(ErrorCode.PARENT_CATEGORY_NOT_FOUND, attributes);
                }

                category.Level = parentCategory.Level + 1;
            }
            else
            {
                category.Level = 0;
            }

            category.Name = categoryRequest.Name;
            category.Description = categoryRequest.Description;
            category.IsActive = categoryRequest.IsActive;
            category.ParentCategoryId = categoryRequest.ParentCategoryId;
            category.UpdatedDate = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            return category.MapModelToResponse();
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _categoryRepository.GetWithSubCategoriesAsync(categoryId);
            if (category == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            if (category.SubCategories.Any())
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId },
                    { "subCategoriesCount", category.SubCategories.Count }
                };
                throw new AppException(ErrorCode.CATEGORY_HAS_SUBCATEGORIES, attributes);
            }

            await _categoryRepository.DeleteAsync(category);
            return true;
        }
    }
}