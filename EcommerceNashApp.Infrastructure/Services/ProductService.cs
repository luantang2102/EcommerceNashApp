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
        private readonly AppDbContext _context;//IProductRepository 
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

            var projectedQuery = query.Select(x => x.MaptoProductResponse());

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
            return product.MaptoProductResponse();
        }

        public async Task<ProductResponse> CreateProductAsync(ProductRequest productRequest)
        {
            var product = new Product
            {
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                InStock = productRequest.InStock,
                
            };
            if (productRequest.FormImages.Count > 0)
            {
                foreach(var image in productRequest.FormImages)
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
            return product.MaptoProductResponse();
        }

    }
}
