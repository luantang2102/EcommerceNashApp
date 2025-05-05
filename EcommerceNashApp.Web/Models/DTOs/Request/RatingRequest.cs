namespace EcommerceNashApp.Web.Models.DTOs.Request
{
    public class RatingRequest
    {
        public int Value { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
    }
}
