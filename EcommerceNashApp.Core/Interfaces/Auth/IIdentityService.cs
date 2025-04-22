using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;

namespace EcommerceNashApp.Core.Interfaces.Auth
{
    public interface IIdentityService
    {
        Task<AuthResponse> GetCurrentUserAsync();
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<AuthResponse> RegisterAsync(RegisterRequest registerRequest);
    }
}
