using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data.Configuration.Base;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Data.Configuration.Extended
{
    class UserProfileConfiguration : BaseEntityConfiguration<UserProfile>
    {
        public override void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.User)
                   .WithMany(x => x.UserProfiles)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
