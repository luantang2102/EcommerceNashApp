using EcommerceNashApp.Core.DTOs.Response;

namespace EcommerceNashApp.Core.DTOs.Auth.Response
{
    public class AuthResponse
    {
        public required string CsrfToken { get; set; }
        public UserResponse User { get; set; } = null!;
    }
}
