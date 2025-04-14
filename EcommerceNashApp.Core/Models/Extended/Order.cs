using EcommerceNashApp.Core.Models.Base;

namespace EcommerceNashApp.Core.Models.Extended
{
    public class Order : BaseEntity
    {
        public double TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;

        // Foreign key
        public Guid UserProfileId { get; set; }

        // Navigation properties
        public virtual UserProfile UserProfile { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    }
}
