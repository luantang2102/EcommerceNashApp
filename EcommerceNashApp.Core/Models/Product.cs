using EcommerceNashApp.Core.Models.Base;
using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Core.Models
{
    public class Product : BaseEntity
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public double AvarageRating { get; set; } = 0.0;

        // Navigation properties
        public virtual ICollection<Category> Categories { get; set; } = [];
        public virtual ICollection<ProductImage> ProductImages { get; set; } = [];
        public virtual ICollection<Rating> Ratings { get; set; } = [];
        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];
        public virtual ICollection<CartItem> CartItems { get; set; } = [];

    }
}
