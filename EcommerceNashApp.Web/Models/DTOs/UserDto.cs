using EcommerceNashApp.Core.DTOs.Response;

namespace EcommerceNashApp.Web.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<string> Roles { get; set; } = [];
        public List<UserProfileDto> UserProfiles { get; set; } = [];
    }
}
