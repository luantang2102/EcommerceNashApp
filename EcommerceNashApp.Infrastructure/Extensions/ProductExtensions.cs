using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Infrastructure.Extentions
{
    public static class ProductExtensions
    {
        public static IQueryable<Product> Sort(this IQueryable<Product> query, string? orderBy)
        {
            query = orderBy switch
            {
                "dateCreatedDesc" => query.OrderByDescending(x => x.CreatedDate),
                "priceAsc" => query.OrderBy(x => x.Price),
                "priceDesc" => query.OrderByDescending(x => x.Price),
                _ => query.OrderBy(x => x.CreatedDate),
            };

            return query;
        }

        public static IQueryable<Product> Search(this IQueryable<Product> query, string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return query;
            var lowerCaseTerm = searchTerm.Trim().ToLower();
            return query.Where(x => x.Name.ToLower().Contains(lowerCaseTerm) || x.Description.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Product> Filter(this IQueryable<Product> query, string? categories, string? ratings, string? minPrice, string? maxPrice, bool? isFeatured)
        {
            var categoryList = new List<string>();
            var ratingList = new List<double>();

            if (!string.IsNullOrEmpty(categories))
            {
                categoryList.AddRange(categories.ToLower().Split(",").ToList());

                query = query.Where(x => x.Categories.Any(c => categoryList.Contains(c.Id.ToString().ToLower())));
            }

            if (!string.IsNullOrEmpty(ratings))
            {
                ratingList.AddRange(ratings.ToLower().Split(",")
                    .Select(r => double.TryParse(r, out var parsedRating) ? parsedRating : 0)
                    .ToList());
                query = query.Where(x => ratingList.Contains(Math.Floor(x.AvarageRating)));
            }

            if (!string.IsNullOrEmpty(minPrice))
            {
                var minPriceValue = double.TryParse(minPrice, out var parsedMinPrice) ? parsedMinPrice : 0;
                query = query.Where(x => x.Price >= minPriceValue);
            }

            if (!string.IsNullOrEmpty(maxPrice))
            {
                var maxPriceValue = double.TryParse(maxPrice, out var parsedMaxPrice) ? parsedMaxPrice : 0;
                query = query.Where(x => x.Price <= maxPriceValue);
            }

            if (isFeatured.HasValue)
            {
                query = query.Where(x => x.IsFeatured == isFeatured.Value);
            }

            return query;
        }

        public static ProductResponse MapModelToResponse(this Product product)
        {
            var averageRating = product.Ratings.Count > 0 ? product.Ratings.Average(x => x.Value) : 0;
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                InStock = product.InStock,
                StockQuantity = product.StockQuantity,
                AverageRating = averageRating,
                IsFeatured = product.IsFeatured,
                ProductImages = product.ProductImages.Select(pi => pi.MapModelToResponse()).ToList(),
                Categories = product.Categories.Select(c => c.MapModelToResponse()).ToList(),
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate
            };
        }
    }
}
