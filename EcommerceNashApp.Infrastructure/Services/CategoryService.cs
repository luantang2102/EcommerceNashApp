using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Helpers.Params;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    class CategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<CategoryResponse>> GetCategoriesAsync(CategoryParams categoryParams)
        {
            var query = _context.Categories
                .Include(x => x.SubCategories)
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
                .Search(categoryParams.SearchTerm);

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await PagedList<CategoryResponse>.ToPagedList(
                projectedQuery,
                categoryParams.PageNumber,
                categoryParams.PageSize
            );
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId)
        {
            var category = await _context.Categories
                .Include(x => x.SubCategories)
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

        public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest categoryRequest)
        {
            int level = 1; // Default level 

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
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "categoryId", categoryId }
                };
                throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
            }

            category.Name = categoryRequest.Name;
            category.Description = categoryRequest.Description;
            category.IsActive = categoryRequest.IsActive;
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
