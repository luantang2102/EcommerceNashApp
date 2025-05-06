using EcommerceNashApp.Shared.DTOs.Response;

namespace EcommerceNashApp.Shared.DTOs.Auth.Response
{
    public class AuthResponse
    {
        public required string CsrfToken { get; set; }
        public UserResponse User { get; set; } = null!;
    }
}
