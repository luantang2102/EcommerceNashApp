using EcommerceNashApp.Core.Models.Extended;
using Microsoft.AspNetCore.Identity;

namespace EcommerceNashApp.Core.Models.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        // Navigation properties
        public virtual Cart Cart { get; set; } = null!;
        public virtual ICollection<UserProfile> UserProfiles { get; set; } = [];
        public virtual ICollection<Rating> Ratings { get; set; } = [];

    }
}
