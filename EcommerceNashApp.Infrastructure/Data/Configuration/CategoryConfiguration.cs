using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceNashApp.Infrastructure.Data.Configuration
{
    class CategoryConfiguration : BaseEntityConfiguration<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> builder)
        {
            base.Configure(builder);

            builder.HasOne(c => c.ParentCategory)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(c => c.ParentCategoryId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Products)
                   .WithMany(p => p.Categories)
                   .UsingEntity<Dictionary<string, object>>(
                       "ProductCategories",
                       j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
                       j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Restrict),
                       j =>
                       {
                           j.HasKey("CategoryId", "ProductId");
                       });
        }
    }
}
