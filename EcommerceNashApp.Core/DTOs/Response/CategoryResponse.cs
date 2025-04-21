namespace EcommerceNashApp.Core.DTOs.Response
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int Level { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? ParentCategoryId { get; set; } 
        public string? ParentCategoryName { get; set; } = string.Empty;
        public List<CategoryResponse> SubCategories { get; set; } = [];
    }
}
