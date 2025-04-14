using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;

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
                   .WithMany(p => p.Categories);
        }
    }
}
