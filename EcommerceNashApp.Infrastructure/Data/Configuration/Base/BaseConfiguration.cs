using EcommerceNashApp.Core.Models.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceNashApp.Infrastructure.Data.Configuration.Base
{
    public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()"); // Use SQL function for default value

            builder.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

        }
    }
}

