namespace EcommerceNashApp.Core.DTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
    }
}
