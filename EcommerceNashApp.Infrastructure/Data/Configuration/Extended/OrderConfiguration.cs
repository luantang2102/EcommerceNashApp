using EcommerceNashApp.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;

namespace EcommerceNashApp.Infrastructure.Data.Configuration.Extended
{
    class OrderConfiguration : BaseEntityConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.UserProfile)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(e => e.UserProfileId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
