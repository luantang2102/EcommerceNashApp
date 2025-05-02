using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Infrastructure.Data.Configuration;
using EcommerceNashApp.Infrastructure.Data.Configuration.Extended;
using EcommerceNashApp.Infrastructure.Data.Configuration.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations for extended entities
            modelBuilder.ApplyConfiguration<AppUser>(new AppUserConfiguration());
            modelBuilder.ApplyConfiguration<Cart>(new CartConfiguration());
            modelBuilder.ApplyConfiguration<Product>(new ProductConfiguration());
            modelBuilder.ApplyConfiguration<Category>(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration<CartItem>(new CartItemConfiguration());
            modelBuilder.ApplyConfiguration<OrderItem>(new OrderItemConfiguration());
            modelBuilder.ApplyConfiguration<Order>(new OrderConfiguration());
            modelBuilder.ApplyConfiguration<ProductImage>(new ProductImageConfiguration());
            modelBuilder.ApplyConfiguration<Rating>(new RatingConfiguration());
            modelBuilder.ApplyConfiguration<UserProfile>(new UserProfileConfiguration());

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>().ToTable("AppUsers");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        }
    }
}
