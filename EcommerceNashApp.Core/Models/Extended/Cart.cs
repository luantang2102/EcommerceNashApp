﻿using EcommerceNashApp.Core.Models.Base;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Core.Models.Identity;

namespace EcommerceNashApp.Core.Models
{
    public class Cart : BaseEntity
    {
        // Foreign key
        public Guid UserId { get; set; }

        // Navigation property
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = [];

    }
}
