using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<CategoryResponse>> GetCategoriesAsync(CategoryParams categoryParams)
        {
            var query = _context.Categories
                .Include(x => x.ParentCategory)
                .AsQueryable();

            if (categoryParams.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(c => c.Id == categoryParams.ParentCategoryId.Value);

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
            // Fetch only root categories (ParentCategoryId is null) and include their subcategories
            var rootCategories = await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories) // Include subcategories
                    .ThenInclude(sc => sc.SubCategories) // Include nested subcategories (Level 2)
                        .ThenInclude(ssc => ssc.SubCategories) // Include deeper nested subcategories (Level 3)
                .Where(c => c.ParentCategoryId == null) // Only root categories
                .ToListAsync();

            // Map to CategoryResponse
            var categoryResponses = rootCategories.Select(c => MapCategoryToResponse(c)).ToList();

            return categoryResponses;
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
            var category = await _context.Categories
                .Include(x => x.ParentCategory)
                .FirstOrDefaultAsync(x => x.Id == categoryId);

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
            var categories = await _context.Categories
                .Where(x => categoryIds.Contains(x.Id))
                .Include(x => x.ParentCategory)
                .ToListAsync();

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
            int level = 0; // Default level 

            if (categoryRequest.ParentCategoryId.HasValue)
            {
                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryRequest.ParentCategoryId.Value);

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

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category.MapModelToResponse();
        }

        public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, CategoryRequest categoryRequest)
        {
            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

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
                var parentCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == categoryRequest.ParentCategoryId.Value);

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
                category.Level = 0; // Default level if no parent category
            }

            category.Name = categoryRequest.Name;
            category.Description = categoryRequest.Description;
            category.IsActive = categoryRequest.IsActive;
            category.ParentCategoryId = categoryRequest.ParentCategoryId;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return category.MapModelToResponse();
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            // Check if this category has subcategories
            if (category.SubCategories.Any())
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId },
                    { "subCategoriesCount", category.SubCategories.Count }
                };
                throw new AppException(ErrorCode.CATEGORY_HAS_SUBCATEGORIES, attributes);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}