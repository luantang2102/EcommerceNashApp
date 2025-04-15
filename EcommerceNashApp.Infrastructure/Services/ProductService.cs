using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Extentions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams)
        {
            var query = _context.Products
                .Include(p => p.Categories)
                .Include(p => p.Ratings)
                .Include(p => p.ProductImages)
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Categories, productParams.Ratings, productParams.MinPrice, productParams.MaxPrice)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MaptoProductResponse());

            return await PagedList<ProductResponse>.ToPagedList(
                projectedQuery,
                productParams.PageNumber,
                productParams.PageSize
            );
        }
    }
}
