using EcommerceNashApp.Core.DTOs.Response;

namespace EcommerceNashApp.Web.Models.DTOs
{
    public class RatingDto
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public UserDto User { get; set; } = null!;
        public Guid ProductId { get; set; } = Guid.Empty;
    }
}

