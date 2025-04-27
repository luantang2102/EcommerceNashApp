namespace EcommerceNashApp.Web.Models.DTOs
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
