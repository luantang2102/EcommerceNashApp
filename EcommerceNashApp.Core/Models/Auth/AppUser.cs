﻿using EcommerceNashApp.Core.Models.Base;
using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Core.Models.Auth
{
    public enum UserRole
    {
        User = 1,
        Admin = 2
    }

    public class AppUser : BaseUser
    {
        public override string? UserName { get; set; } = "Anonymous";
        public override string? Email { get; set; } = "Anonymous";
        public string? ImageUrl { get; set; } = string.Empty;
        public string? PublicId { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        // Navigation properties
        public virtual Cart Cart { get; set; } = null!;
        public virtual ICollection<UserProfile> UserProfiles { get; set; } = [];
        public virtual ICollection<Rating> Ratings { get; set; } = [];

    }
}
