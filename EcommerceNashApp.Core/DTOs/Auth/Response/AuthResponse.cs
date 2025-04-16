using EcommerceNashApp.Core.DTOs.Response;

namespace EcommerceNashApp.Core.DTOs.Auth.Response
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
    }
}
