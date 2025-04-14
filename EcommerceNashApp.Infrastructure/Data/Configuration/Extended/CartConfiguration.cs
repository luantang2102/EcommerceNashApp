using EcommerceNashApp.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;

namespace EcommerceNashApp.Infrastructure.Data.Configuration.Extended
{
    class CartConfiguration : BaseEntityConfiguration<Cart>
    {
        public override void Configure(EntityTypeBuilder<Cart> builder)
        {
            base.Configure(builder);

            builder.HasOne(c => c.User)
                   .WithOne(u => u.Cart)
                   .HasForeignKey<Cart>(c => c.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}
