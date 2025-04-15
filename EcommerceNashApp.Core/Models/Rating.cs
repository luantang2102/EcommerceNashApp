using EcommerceNashApp.Core.Models.Base;
using EcommerceNashApp.Core.Models.Identity;

namespace EcommerceNashApp.Core.Models
{
    public class Rating : BaseEntity
    {
        public int Value { get; set; }
        public required string Comment { get; set; }

        // Foreign key
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual AppUser User { get; set; } = null!;
    }
}
