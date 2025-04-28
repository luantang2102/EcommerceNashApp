using EcommerceNashApp.Core.Settings;
using EcommerceNashApp.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class CloudinaryServiceTests
    {
        private readonly Mock<IOptions<CloudinaryConfig>> _configMock;
        private readonly CloudinaryService _cloudinaryService;

        public CloudinaryServiceTests()
        {
            _configMock = new Mock<IOptions<CloudinaryConfig>>();
            _configMock.Setup(c => c.Value).Returns(new CloudinaryConfig
            {
                CloudName = "test",
                ApiKey = "key",
                ApiSecret = "secret"
            });
            _cloudinaryService = new CloudinaryService(_configMock.Object);
        }

        [Fact]
        public async Task AddImageAsync_WithValidFile_ReturnsUploadResult()
        {
            // Arrange
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.Length).Returns(100);
            formFileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));

            // Act
            // Note: Actual Cloudinary interaction is external, so we rely on mocking in real tests
            var result = await _cloudinaryService.AddImageAsync(formFileMock.Object);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteImageAsync_WithValidPublicId_ReturnsDeletionResult()
        {
            // Arrange
            var publicId = "test-public-id";

            // Act
            // Note: Actual Cloudinary deletion is external, so we rely on mocking in real tests
            var result = await _cloudinaryService.DeleteImageAsync(publicId);

            // Assert
            Assert.NotNull(result);
        }
    }
}
