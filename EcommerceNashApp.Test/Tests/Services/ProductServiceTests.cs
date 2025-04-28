using CloudinaryDotNet.Actions;
using EcommerceNashApp.Core.DTOs.Request;
using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using EcommerceNashApp.Infrastructure.Services;
using EcommerceNashApp.Shared.Paginations;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EcommerceNashApp.Test.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IMediaService> _mediaServiceMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            //var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            //optionsBuilder.UseSqlServer("");

            //var options = optionsBuilder.Options;

            //using var context = new AppDbContext(options);

            

            _productRepositoryMock = new Mock<IProductRepository>();
            _mediaServiceMock = new Mock<IMediaService>();
            _productService = new ProductService(_productRepositoryMock.Object, _mediaServiceMock.Object);
        }

        [Fact]
        public async Task GetProductsAsync_ReturnsPagedList()
        {
            // Arrange
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Test Product", Description = "Test Desc" }
            };
            _productRepositoryMock.Setup(r => r.GetAllAsync())
                .Returns(() => products.AsQueryable().AsDbSet());

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

    // Extension method to convert IEnumerable to DbSet for Moq.EntityFrameworkCore
    public static class QueryableExtensions
    {
        public static IQueryable<T> AsDbSet<T>(this IQueryable<T> source) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
            return mockDbSet.Object.AsQueryable();
        }
    }
}