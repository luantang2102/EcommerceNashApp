namespace EcommerceNashApp.Shared.DTOs.Auth.Response
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string CsrfToken { get; set; } = string.Empty;
        public AuthResponse AuthResponse { get; set; } = null!;
    }
}
