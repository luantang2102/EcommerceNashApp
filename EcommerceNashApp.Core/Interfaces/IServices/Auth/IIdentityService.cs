using EcommerceNashApp.Shared.DTOs.Auth.Request;
using EcommerceNashApp.Shared.DTOs.Auth.Response;

namespace EcommerceNashApp.Core.Interfaces.IServices.Auth
{
    public interface IIdentityService
    {
        Task<AuthResponse> GetCurrentUserAsync(Guid userId);
        Task<TokenResponse> LoginAsync(LoginRequest loginRequest);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<TokenResponse> RegisterAsync(RegisterRequest registerRequest);
    }
}
