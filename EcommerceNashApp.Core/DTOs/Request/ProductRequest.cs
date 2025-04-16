using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.DTOs.Request
{
    public class ProductRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; }
        public List<ExistingProductImageRequest> Images { get; set; } = [];
        public List<IFormFile> FormImages { get; set; } = [];

    }
}
