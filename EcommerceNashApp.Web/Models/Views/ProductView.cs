namespace EcommerceNashApp.Web.Models.Views
{
    public class ProductView
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int StockQuantity { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
        public List<ProductImageView> ProductImages { get; set; } = [];
    }
}
