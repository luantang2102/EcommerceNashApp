using CloudinaryDotNet.Actions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using EcommerceNashApp.Infrastructure.Services;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.AspNetCore.Http;
using Moq;

namespace EcommerceNashApp.Test.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IMediaService> _mediaServiceMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _mediaServiceMock = new Mock<IMediaService>();
            _productService = new ProductService(_productRepositoryMock.Object, _mediaServiceMock.Object);
        }

        [Fact]
        public async Task GetProductsAsync_ReturnsPagedList()
        {
            // Arrange
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<Product> { new Product { Id = Guid.NewGuid(), Name = "Test Product", Description = "Test Desc" } };
            var queryable = products.AsQueryable();
            _productRepositoryMock.Setup(r => r.GetAllAsync()).Returns(queryable);
            PagedList<ProductResponse> pagedList = new PagedList<ProductResponse>(
                products.Select(p => new ProductResponse { Id = p.Id, Name = p.Name }).ToList(),
                1, 1, 10);

            // Act
            var result = await _productService.GetProductsAsync(productParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Product", result[0].Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ReturnsProductResponse()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Test Product", Description = "Test Desc" };
            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product", result.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductNotFound_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.GetProductByIdAsync(productId));
            Assert.Equal(ErrorCode.PRODUCT_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task CreateProductAsync_WithValidRequest_ReturnsProductResponse()
        {
            // Arrange
            var productRequest = new ProductRequest
            {
                Name = "New Product",
                Price = 100,
                CategoryIds = new List<Guid> { Guid.NewGuid() },
                FormImages = new List<IFormFile> { new Mock<IFormFile>().Object }
            };
            var category = new Category { Id = productRequest.CategoryIds[0], Name = "Test Category", Description = "Test Desc" };
            var product = new Product { Id = Guid.NewGuid(), Name = productRequest.Name, Description = productRequest.Description };
            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category> { category });
            _mediaServiceMock.Setup(m => m.AddImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult { PublicId = "publicId", SecureUrl = new Uri("http://example.com") });
            _productRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _productService.CreateProductAsync(productRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productRequest.Name, result.Name);
            _productRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once());
        }
    }
}


