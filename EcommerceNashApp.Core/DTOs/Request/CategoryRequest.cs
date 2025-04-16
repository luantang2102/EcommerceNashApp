namespace EcommerceNashApp.Core.DTOs.Request
{
    public class CategoryRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentCategoryId { get; set; } = null;

    }
}
