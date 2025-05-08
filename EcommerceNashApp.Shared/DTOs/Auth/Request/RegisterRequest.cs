namespace EcommerceNashApp.Shared.DTOs.Auth.Request
{
    public class RegisterRequest
    {
        public string? UserName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;

    }
}
