namespace EcommerceNashApp.Core.DTOs.Auth.Request
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

    }
}
