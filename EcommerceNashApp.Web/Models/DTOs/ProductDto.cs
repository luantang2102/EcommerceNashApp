using EcommerceNashApp.Core.DTOs.Response;

namespace EcommerceNashApp.Web.Models.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;
        public List<ProductImageDto> ProductImages { get; set; } = [];
        public List<CategoryDto> Categories { get; set; } = [];
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
