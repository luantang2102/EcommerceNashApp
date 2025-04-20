using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Helpers;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extentions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductService(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams)
        {
            var query = _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages)
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Categories, productParams.Ratings, productParams.MinPrice, productParams.MaxPrice)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await PagedList<ProductResponse>.ToPagedList(
                projectedQuery,
                productParams.PageNumber,
                productParams.PageSize
            );
        }

        public async Task<ProductResponse> GetProductByIdAsync(Guid productId)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages)
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            return product.MapModelToResponse();
        }

        public async Task<PagedList<ProductResponse>> GetProductsByCategoryIdAsync(Guid categoryId, ProductParams productParams)
        {
            var query = _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Ratings)
                .Include(x => x.ProductImages)
                .Where(x => x.Categories.Any(c => c.Id == categoryId))
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Categories, productParams.Ratings, productParams.MinPrice, productParams.MaxPrice)
                .AsQueryable();

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await PagedList<ProductResponse>.ToPagedList(
                projectedQuery,
                productParams.PageNumber,
                productParams.PageSize
            );
        }

        public async Task<ProductResponse> CreateProductAsync(ProductRequest productRequest)
        {
            var product = new Product
            {
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                InStock = productRequest.InStock,
                StockQuantity = productRequest.StockQuantity,
            };

            if (productRequest.CategoryIds.Count > 0)
            {
                var categories = await _context.Categories
                    .Where(c => productRequest.CategoryIds.Contains(c.Id))
                    .ToListAsync();
                if (categories.Count != productRequest.CategoryIds.Count)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "categoryIds", productRequest.CategoryIds }
                    };
                    throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
                }
                product.Categories = categories;
            }

            if (productRequest.FormImages.Count > 0)
            {
                foreach (var image in productRequest.FormImages)
                {
                    var uploadResult = await _cloudinaryService.AddImageAsync(image);
                    var productImage = new ProductImage
                    {
                        PublicId = uploadResult.PublicId,
                        ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                        Product = product
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.MapModelToResponse();
        }

        public async Task<ProductResponse> UpdateProductAsync(Guid productId, ProductRequest productRequest)
        {
            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            product.Name = productRequest.Name;
            product.Description = productRequest.Description;
            product.Price = productRequest.Price;
            product.InStock = productRequest.InStock;

            var existingImages = product.ProductImages.ToList();
            var requestImageIds = productRequest.Images.Select(i => i.Id).ToList();

            foreach (var image in existingImages)
            {
                if (!requestImageIds.Contains(image.Id))
                {
                    await _cloudinaryService.DeleteImageAsync(image.PublicId);
                    _context.ProductImages.Remove(image);
                }
            }

            // Update IsMain for existing images
            foreach (var existing in product.ProductImages)
            {
                var match = productRequest.Images.FirstOrDefault(i => i.Id == existing.Id);
                if (match != null)
                {
                    existing.IsMain = match.IsMain;
                }
            }

            if (productRequest.FormImages?.Count > 0)
            {
                foreach (var image in productRequest.FormImages)
                {
                    var uploadResult = await _cloudinaryService.AddImageAsync(image);
                    var productImage = new ProductImage
                    {
                        PublicId = uploadResult.PublicId,
                        ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                        IsMain = false,
                        Product = product
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            await _context.SaveChangesAsync();
            return product.MapModelToResponse();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            foreach (var image in product.ProductImages)
            {
                await _cloudinaryService.DeleteImageAsync(image.PublicId);
            }

            _context.ProductImages.RemoveRange(product.ProductImages);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

    }
}
