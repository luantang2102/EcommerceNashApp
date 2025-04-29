namespace EcommerceNashApp.Core.DTOs.Auth.Response
{
    public class TokenResponse
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required string CsrfToken { get; set; }
        public required AuthResponse AuthResponse { get; set; }
    }
}
