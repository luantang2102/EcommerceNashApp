using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Core.Settings;
using EcommerceNashApp.Infrastructure.Services.Auth;
using Microsoft.Extensions.Options;
using Moq;

namespace EcommerceNashApp.Test.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _jwtSettingsMock.Setup(s => s.Value).Returns(new JwtSettings
            {
                Key = "super-secret-key-1234567890abcdef",
                Issuer = "test-issuer",
                Audience = "test-audience",
                DurationInMinutes = 60
            });
            _jwtService = new JwtService(_jwtSettingsMock.Object);
        }

        [Fact]
        public void GenerateToken_WithValidUser_ReturnsToken()
        {
            // Arrange
            var user = new AppUser { Id = Guid.NewGuid(), UserName = "TestUser" };
            var roles = new List<string> { "User" };

            // Act
            var token = _jwtService.GenerateToken(user, roles);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyToken()
        {
            // Act
            var token = _jwtService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }
    }
}
