using CloudinaryDotNet.Actions;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Interfaces.IRepositories;
using EcommerceNashApp.Core.Interfaces.IServices;
using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Exceptions;
using EcommerceNashApp.Infrastructure.Extentions;
using EcommerceNashApp.Infrastructure.Helpers.Params;
using EcommerceNashApp.Infrastructure.Services;
using EcommerceNashApp.Shared.DTOs.Request;
using EcommerceNashApp.Shared.DTOs.Response;
using EcommerceNashApp.Shared.Paginations;
using EcommerceNashApp.Shared.Paginations.Service;
using Microsoft.AspNetCore.Http;
using Moq;
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
        private readonly Mock<IPaginationService> _paginationServiceMock;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _mediaServiceMock = new Mock<IMediaService>();
            _paginationServiceMock = new Mock<IPaginationService>();
            _productService = new ProductService(_productRepositoryMock.Object, _mediaServiceMock.Object, _paginationServiceMock.Object);
        }

        [Fact]
        public async Task GetProductsAsync_ReturnsPagedList()
        {
            // Arrange
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product",
                    Description = "Test Desc",
                    Price = 99.99,
                    InStock = true,
                    StockQuantity = 50,
                    AvarageRating = 4.5,
                    IsFeatured = true,
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "http://example.com/image.jpg", PublicId = "image1", IsMain = true }
                    },
                    Categories = new List<Category>
                    {
                        new Category
                        {
                            Id = Guid.NewGuid(),
                            Name = "Test Category",
                            Description = "Category Desc",
                            Level = 1,
                            IsActive = true
                        }
                    }
                }
            };
            var queryable = products.AsQueryable();

            _productRepositoryMock.Setup(r => r.GetAllAsync()).Returns(queryable);
            _paginationServiceMock
                .Setup(p => p.EF_ToPagedList(It.IsAny<IQueryable<ProductResponse>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<ProductResponse>(
                    products.Select(p => p.MapModelToResponse()).ToList(), 1, 1, 10));

            // Act
            var result = await _productService.GetProductsAsync(productParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Product", result[0].Name);
            Assert.Equal(99.99, result[0].Price);
            Assert.True(result[0].InStock);
            Assert.Equal(50, result[0].StockQuantity);
            Assert.Equal(4.5, result[0].AverageRating);
            Assert.True(result[0].IsFeatured);
            Assert.Single(result[0].ProductImages);
            Assert.Equal("http://example.com/image.jpg", result[0].ProductImages[0].ImageUrl);
            Assert.True(result[0].ProductImages[0].IsMain);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ReturnsProductResponse()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Desc",
                Price = 49.99,
                InStock = false,
                StockQuantity = 0,
                AvarageRating = 3.0,
                IsFeatured = false,
                ProductImages = new List<ProductImage>
                {
                    new ProductImage { Id = Guid.NewGuid(), ImageUrl = "http://example.com/image.jpg", PublicId = "image1", IsMain = true }
                },
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Category",
                        Description = "Category Desc",
                        Level = 1,
                        IsActive = true
                    }
                }
            };
            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(49.99, result.Price);
            Assert.False(result.InStock);
            Assert.Equal(0, result.StockQuantity);
            Assert.Equal(3.0, result.AverageRating);
            Assert.False(result.IsFeatured);
            Assert.Single(result.ProductImages);
            Assert.Equal("http://example.com/image.jpg", result.ProductImages[0].ImageUrl);
            Assert.True(result.ProductImages[0].IsMain);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductNotFound_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.GetProductByIdAsync(productId));
            Assert.Equal(ErrorCode.PRODUCT_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_ReturnsPagedList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product",
                    Description = "Test Desc",
                    Price = 29.99,
                    InStock = true,
                    StockQuantity = 100,
                    AvarageRating = 4.0,
                    IsFeatured = false,
                    Categories = new List<Category>
                    {
                        new Category
                        {
                            Id = categoryId,
                            Name = "Test Category",
                            Description = "Category Desc",
                            Level = 1,
                            IsActive = true
                        }
                    },
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage { Id = Guid.NewGuid(), ImageUrl = "http://example.com/image.jpg", PublicId = "image1", IsMain = true }
                    }
                }
            };
            var queryable = products.AsQueryable();

            _productRepositoryMock.Setup(r => r.GetByCategoryIdAsync(categoryId)).Returns(queryable);
            _paginationServiceMock
                .Setup(p => p.EF_ToPagedList(It.IsAny<IQueryable<ProductResponse>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<ProductResponse>(
                    products.Select(p => p.MapModelToResponse()).ToList(), 1, 1, 10));

            // Act
            var result = await _productService.GetProductsByCategoryIdAsync(categoryId, productParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Product", result[0].Name);
            Assert.Equal(29.99, result[0].Price);
            Assert.True(result[0].InStock);
            Assert.Equal(100, result[0].StockQuantity);
            Assert.Equal(4.0, result[0].AverageRating);
            Assert.False(result[0].IsFeatured);
            Assert.Single(result[0].ProductImages);
            Assert.Equal("http://example.com/image.jpg", result[0].ProductImages[0].ImageUrl);
            Assert.True(result[0].ProductImages[0].IsMain);
        }

        [Fact]
        public async Task GetProductsByCategoryIdAsync_WhenNoProductsFound_ReturnsEmptyPagedList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
            var products = new List<Product>();
            var queryable = products.AsQueryable();

            _productRepositoryMock.Setup(r => r.GetByCategoryIdAsync(categoryId)).Returns(queryable);
            _paginationServiceMock
                .Setup(p => p.EF_ToPagedList(It.IsAny<IQueryable<ProductResponse>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new PagedList<ProductResponse>(new List<ProductResponse>(), 0, 1, 10));

            // Act
            var result = await _productService.GetProductsByCategoryIdAsync(categoryId, productParams);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateProductAsync_WithValidRequest_ReturnsProductResponse()
        {
            // Arrange
            var productRequest = new ProductRequest
            {
                Name = "New Product",
                Description = "New Desc",
                Price = 150.00,
                InStock = true,
                StockQuantity = 20,
                IsFeatured = true,
                CategoryIds = new List<Guid> { Guid.NewGuid() },
                FormImages = new List<IFormFile> { new Mock<IFormFile>().Object }
            };
            var category = new Category
            {
                Id = productRequest.CategoryIds[0],
                Name = "Test Category",
                Description = "Category Desc",
                Level = 1,
                IsActive = true
            };
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                InStock = productRequest.InStock,
                StockQuantity = productRequest.StockQuantity,
                IsFeatured = productRequest.IsFeatured,
                Categories = new List<Category> { category },
                ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = "http://example.com/image.jpg",
                        PublicId = "publicId",
                        IsMain = true,
                        ProductId = Guid.NewGuid()
                    }
                }
            };

            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category> { category });
            _mediaServiceMock.Setup(m => m.AddImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult { PublicId = "publicId", SecureUrl = new Uri("http://example.com/image.jpg") });
            _productRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _productService.CreateProductAsync(productRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productRequest.Name, result.Name);
            Assert.Equal(productRequest.Price, result.Price);
            Assert.True(result.InStock);
            Assert.Equal(20, result.StockQuantity);
            Assert.True(result.IsFeatured);
            Assert.Single(result.ProductImages);
            Assert.Equal("http://example.com/image.jpg", result.ProductImages[0].ImageUrl);
            Assert.Equal("publicId", result.ProductImages[0].PublicId);
            Assert.True(result.ProductImages[0].IsMain);
            _productRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once());
        }

        [Fact]
        public async Task CreateProductAsync_WhenCategoryNotActive_ThrowsAppException()
        {
            // Arrange
            var productRequest = new ProductRequest
            {
                Name = "New Product",
                Description = "New Desc",
                Price = 150.00,
                CategoryIds = new List<Guid> { Guid.NewGuid() }
            };
            var category = new Category
            {
                Id = productRequest.CategoryIds[0],
                Name = "Inactive Category",
                Description = "Category Desc",
                Level = 1,
                IsActive = false
            };

            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category> { category });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.CreateProductAsync(productRequest));
            Assert.Equal(ErrorCode.CATEGORY_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidRequest_ReturnsUpdatedProductResponse()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productRequest = new ProductRequest
            {
                Name = "Updated Product",
                Description = "Updated Desc",
                Price = 200.00,
                InStock = false,
                StockQuantity = 10,
                IsFeatured = true,
                CategoryIds = new List<Guid> { Guid.NewGuid() },
                FormImages = new List<IFormFile> { new Mock<IFormFile>().Object }
            };
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product",
                Description = "Old Desc",
                Price = 100.00,
                InStock = true,
                StockQuantity = 50,
                AvarageRating = 4.0,
                IsFeatured = false,
                ProductImages = new List<ProductImage>
                {
                    new ProductImage { Id = Guid.NewGuid(), ImageUrl = "http://example.com/old.jpg", PublicId = "oldPublicId", IsMain = true }
                },
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Old Category",
                        Description = "Old Category Desc",
                        Level = 1,
                        IsActive = true
                    }
                }
            };
            var category = new Category
            {
                Id = productRequest.CategoryIds[0],
                Name = "Test Category",
                Description = "Category Desc",
                Level = 1,
                IsActive = true
            };
            var updatedProduct = new Product
            {
                Id = productId,
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                InStock = productRequest.InStock,
                StockQuantity = productRequest.StockQuantity,
                IsFeatured = productRequest.IsFeatured,
                ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = "http://example.com/image.jpg",
                        PublicId = "publicId",
                        IsMain = true,
                        ProductId = productId
                    }
                },
                Categories = new List<Category> { category }
            };

            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync(existingProduct);
            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category> { category });
            _mediaServiceMock.Setup(m => m.AddImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(new ImageUploadResult { PublicId = "publicId", SecureUrl = new Uri("http://example.com/image.jpg") });
            _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Callback<Product>(p =>
            {
                existingProduct.Name = p.Name;
                existingProduct.Description = p.Description;
                existingProduct.Price = p.Price;
                existingProduct.InStock = p.InStock;
                existingProduct.StockQuantity = p.StockQuantity;
                existingProduct.IsFeatured = p.IsFeatured;
                existingProduct.ProductImages = p.ProductImages;
                existingProduct.Categories = p.Categories;
            }).Returns(Task.CompletedTask);

            // Act
            var result = await _productService.UpdateProductAsync(productId, productRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productRequest.Name, result.Name);
            Assert.Equal(productRequest.Price, result.Price);
            Assert.False(result.InStock);
            Assert.Equal(10, result.StockQuantity);
            Assert.True(result.IsFeatured);
            Assert.Single(result.ProductImages);
            Assert.Equal("http://example.com/image.jpg", result.ProductImages[0].ImageUrl);
            Assert.Equal("publicId", result.ProductImages[0].PublicId);
            Assert.True(result.ProductImages[0].IsMain);
            _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once());
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductNotFound_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productRequest = new ProductRequest
            {
                Name = "Updated Product",
                Description = "Updated Desc",
                Price = 200.00
            };
            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.UpdateProductAsync(productId, productRequest));
            Assert.Equal(ErrorCode.PRODUCT_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task UpdateProductAsync_WhenCategoriesNotFound_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productRequest = new ProductRequest
            {
                Name = "Updated Product",
                Description = "Updated Desc",
                CategoryIds = new List<Guid> { Guid.NewGuid() }
            };
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product",
                Description = "Old Desc",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Old Category",
                        Description = "Old Category Desc",
                        Level = 1,
                        IsActive = true
                    }
                }
            };
            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync(existingProduct);
            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.UpdateProductAsync(productId, productRequest));
            Assert.Equal(ErrorCode.CATEGORY_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task UpdateProductAsync_WhenCategoryNotActive_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var productRequest = new ProductRequest
            {
                Name = "Updated Product",
                Description = "Updated Desc",
                CategoryIds = new List<Guid> { Guid.NewGuid() }
            };
            var existingProduct = new Product
            {
                Id = productId,
                Name = "Old Product",
                Description = "Old Desc",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Old Category",
                        Description = "Old Category Desc",
                        Level = 1,
                        IsActive = true
                    }
                }
            };
            var category = new Category
            {
                Id = productRequest.CategoryIds[0],
                Name = "Inactive Category",
                Description = "Category Desc",
                Level = 1,
                IsActive = false
            };

            _productRepositoryMock.Setup(r => r.GetWithImagesAsync(productId)).ReturnsAsync(existingProduct);
            _productRepositoryMock.Setup(r => r.GetCategoriesByIdsAsync(productRequest.CategoryIds))
                .ReturnsAsync(new List<Category> { category });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.UpdateProductAsync(productId, productRequest));
            Assert.Equal(ErrorCode.CATEGORY_NOT_FOUND, exception.GetErrorCode());
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductExists_DeletesProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Desc",
                InStock = true,
                StockQuantity = 30,
                ProductImages = new List<ProductImage>
                {
                    new ProductImage { Id = Guid.NewGuid(), ImageUrl = "http://example.com/image.jpg", PublicId = "image1", IsMain = true }
                },
                Categories = new List<Category>
                {
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test Category",
                        Description = "Category Desc",
                        Level = 1,
                        IsActive = true
                    }
                }
            };
            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.DeleteAsync(product)).Returns(Task.CompletedTask);

            // Act
            await _productService.DeleteProductAsync(productId);

            // Assert
            _productRepositoryMock.Verify(r => r.DeleteAsync(product), Times.Once());
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductNotFound_ThrowsAppException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _productRepositoryMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AppException>(() => _productService.DeleteProductAsync(productId));
            Assert.Equal(ErrorCode.PRODUCT_NOT_FOUND, exception.GetErrorCode());
        }
    }
}