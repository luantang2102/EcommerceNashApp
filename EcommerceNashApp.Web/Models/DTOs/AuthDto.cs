namespace EcommerceNashApp.Web.Models.DTOs
{
    public class AuthDto
    {
        public string Jwt { get; set; }
        public string RefreshToken { get; set; }
        public string CsrfToken { get; set; }
        public UserDto User { get; set; } = null!;

    }
}