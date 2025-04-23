using EcommerceNashApp.Core.Models.Auth;

namespace EcommerceNashApp.Core.DTOs.Response
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<UserProfileResponse> UserProfiles { get; set; } = [];

    }
}
