namespace EcommerceNashApp.Core.DTOs.Request
{
    public class ExistingProductImageRequest
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }
}
