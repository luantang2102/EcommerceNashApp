using System;

namespace EcommerceNashApp.Shared.DTOs.Response
{
    public class ProductImageResponse
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
