namespace EcommerceNashApp.Core.DTOs.Request
{
    public class ExistingProductImageRequest
    {
        public required string ImageUrl { get; set; }
        public bool IsMain { get; set; }
    }
}
