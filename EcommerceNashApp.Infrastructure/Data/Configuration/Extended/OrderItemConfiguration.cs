using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceNashApp.Infrastructure.Data.Configuration.Extended
{
    class OrderItemConfiguration : BaseEntityConfiguration<OrderItem>
    {
        public override void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(e => e.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(e => e.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
