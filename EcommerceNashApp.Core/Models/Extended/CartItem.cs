using EcommerceNashApp.Core.Models.Base;

namespace EcommerceNashApp.Core.Models.Extended
{
    public class CartItem : BaseEntity
    {
        public int Quantity { get; set; }
        public double Price { get; set; }

        // Foreign key
        public Guid ProductId { get; set; }
        public Guid CartId { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Cart Cart { get; set; } = null!;

    }
}
