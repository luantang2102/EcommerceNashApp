using System;

namespace EcommerceNashApp.Shared.DTOs.Response
{
    public class RatingResponse
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public UserResponse User { get; set; } = null!;
        public Guid ProductId { get; set; } = Guid.Empty;
    }
}
