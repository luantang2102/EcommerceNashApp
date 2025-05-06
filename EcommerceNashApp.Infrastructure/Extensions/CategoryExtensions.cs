using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class CategoryExtensions
    {
        public static IQueryable<Category> Sort(this IQueryable<Category> query, string? orderBy)
        {
            query = orderBy switch
            {
                "dateCreatedDesc" => query.OrderByDescending(x => x.CreatedDate),
                _ => query.OrderBy(x => x.CreatedDate),
            };
            return query;
        }


        public static IQueryable<Category> Search(this IQueryable<Category> query, string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return query;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return query.Where(x => x.Name.ToLower().Contains(lowerCaseTerm) || x.Description.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Category> Filter(this IQueryable<Category> query, string? level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                var levelValue = int.TryParse(level, out var parsedLevel) ? parsedLevel : 0;
                query = query.Where(x => x.Level == levelValue);
            }
            return query;
        }

        public static CategoryResponse MapModelToResponse(this Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Level = category.Level,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                ParentCategoryId = category.ParentCategory?.Id,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategories = category.SubCategories.Select(c => c.MapModelToResponse()).ToList()
            };
        }
    }
}
