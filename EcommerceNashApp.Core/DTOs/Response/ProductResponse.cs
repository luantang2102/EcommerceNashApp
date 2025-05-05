namespace EcommerceNashApp.Core.DTOs.Response
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
        public bool IsFeatured { get; set; } = false;
        public List<ProductImageResponse> ProductImages { get; set; } = [];
        public List<CategoryResponse> Categories { get; set; } = [];
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
