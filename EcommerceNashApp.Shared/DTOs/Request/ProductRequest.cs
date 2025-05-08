using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace EcommerceNashApp.Shared.DTOs.Request
{
    public class ProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool InStock { get; set; } = true;
        public int StockQuantity { get; set; }
        public bool IsFeatured { get; set; } = false;
        public List<ExistingProductImageRequest> Images { get; set; } = new List<ExistingProductImageRequest>();
        public List<IFormFile> FormImages { get; set; } = new List<IFormFile>();
        public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    }
}
