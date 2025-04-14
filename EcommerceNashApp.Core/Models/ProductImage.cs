using EcommerceNashApp.Core.Models.Base;

namespace EcommerceNashApp.Core.Models
{
    public class ProductImage : BaseEntity
    {
        public required string ImageUrl { get; set; }
        public bool IsMain { get; set; }

        // Foreign key
        public Guid ProductId { get; set; }

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}
