using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Core.Models.Base;

namespace EcommerceNashApp.Core.Models.Extended
{
    public class UserProfile : BaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }

        // Foreign key
        public Guid UserId { get; set; }

        // Navigation property
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = [];

    }
}
