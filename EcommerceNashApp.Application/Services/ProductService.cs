using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extensions;
using EcommerceNashApp.Infrastructure.Extentions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;

namespace EcommerceNashApp.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMediaService _mediaService;
        private readonly IPaginationService _paginationService;
        private readonly ICartRepository _cartRepository;

        public ProductService(
            IProductRepository productRepository,
            IMediaService mediaService,
            IPaginationService paginationService,
            ICartRepository cartRepository)
        {
            _productRepository = productRepository;
            _mediaService = mediaService;
            _paginationService = paginationService;
            _cartRepository = cartRepository;
        }

        public async Task<PagedList<ProductResponse>> GetProductsAsync(ProductParams productParams)
        {
            var query = _productRepository.GetAllAsync()
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Categories, productParams.Ratings, productParams.MinPrice, productParams.MaxPrice, productParams.IsFeatured);

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await _paginationService.EF_ToPagedList(
                projectedQuery,
                productParams.PageNumber,
                productParams.PageSize
            );
        }

        public async Task<ProductResponse> GetProductByIdAsync(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
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
            var query = _productRepository.GetByCategoryIdAsync(categoryId)
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Categories, productParams.Ratings, productParams.MinPrice, productParams.MaxPrice, productParams.IsFeatured);

            var projectedQuery = query.Select(x => x.MapModelToResponse());

            return await _paginationService.EF_ToPagedList(
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
                IsFeatured = productRequest.IsFeatured,
                ProductImages = []
            };

            if (productRequest.CategoryIds.Count > 0)
            {
                var categories = await _productRepository.GetCategoriesByIdsAsync(productRequest.CategoryIds);
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
                    var uploadResult = await _mediaService.AddImageAsync(image);
                    var productImage = new ProductImage
                    {
                        PublicId = uploadResult.PublicId,
                        ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                        Product = product
                    };
                    product.ProductImages.Add(productImage);
                }
            }

            var createdProduct = await _productRepository.CreateAsync(product);
            return createdProduct.MapModelToResponse();
        }

        public async Task<ProductResponse> UpdateProductAsync(Guid productId, ProductRequest productRequest)
        {
            var product = await _productRepository.GetWithImagesAsync(productId);
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
            product.StockQuantity = productRequest.StockQuantity;
            product.IsFeatured = productRequest.IsFeatured;
            product.UpdatedDate = DateTime.UtcNow;

            var existingImages = product.ProductImages.ToList();
            var requestImageIds = productRequest.Images.Select(i => i.Id).ToList();

            foreach (var image in existingImages)
            {
                if (!requestImageIds.Contains(image.Id))
                {
                    await _mediaService.DeleteImageAsync(image.PublicId);
                    product.ProductImages.Remove(image);
                }
            }

            if (productRequest.CategoryIds.Count > 0)
            {
                var categories = await _productRepository.GetCategoriesByIdsAsync(productRequest.CategoryIds);
                if (categories.Count != productRequest.CategoryIds.Count)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "categoryIds", productRequest.CategoryIds }
                    };
                    throw new AppException(ErrorCode.CATEGORY_NOT_FOUND, attributes);
                }

                var productWithCategories = await _productRepository.GetWithCategoriesAsync(productId);
                if (productWithCategories == null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", productId }
                    };
                    throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
                }

                productWithCategories.Categories.Clear();
                foreach (var category in categories)
                {
                    productWithCategories.Categories.Add(category);
                }
                product.Categories = productWithCategories.Categories;
            }
            else
            {
                var productWithCategories = await _productRepository.GetWithCategoriesAsync(productId);
                if (productWithCategories == null)
                {
                    var attributes = new Dictionary<string, object>
                    {
                        { "productId", productId }
                    };
                    throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
                }
                productWithCategories.Categories.Clear();
                product.Categories = productWithCategories.Categories;
            }

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
                    var uploadResult = await _mediaService.AddImageAsync(image);
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

            await _productRepository.UpdateAsync(product);
            return product.MapModelToResponse();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var product = await _productRepository.GetWithImagesAsync(productId);
            if (product == null)
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_NOT_FOUND, attributes);
            }

            // Check if the product exists in any cart using the repository
            if (await _cartRepository.HasProductAsync(productId))
            {
                var attributes = new Dictionary<string, object>
                {
                    { "productId", productId }
                };
                throw new AppException(ErrorCode.PRODUCT_IN_CART, attributes);
            }

            // Delete associated images
            foreach (var image in product.ProductImages)
            {
                await _mediaService.DeleteImageAsync(image.PublicId);
            }

            // Delete the product
            await _productRepository.DeleteAsync(product);
        }
    }
}