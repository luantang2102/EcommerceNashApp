using Microsoft.AspNetCore.Http;

namespace EcommerceNashApp.Core.DTOs.Request
{
    public class ProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; }
        public List<ExistingProductImageRequest> Images { get; set; } = [];
        public List<IFormFile> FormImages { get; set; } = [];
        public List<Guid> CategoryIds { get; set; } = [];
    }
}
