using EcommerceNashApp.Core.Models;
using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EcommerceNashApp.Api.SeedData
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var userManager = services.GetRequiredService<UserManager<AppUser>>()
                ?? throw new InvalidOperationException("Failed to retrieve user manager");
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>()
                ?? throw new InvalidOperationException("Failed to retrieve role manager");

            await SeedUsersAndRoles(userManager, roleManager);

            var dbContext = services.GetRequiredService<AppDbContext>()
                ?? throw new InvalidOperationException("Failed to retrieve app db context");

            await SeedCategory(dbContext);
            await SeedSubCategories(dbContext); 
            await SeedProducts(dbContext); 
        }

        private static async Task SeedUsersAndRoles(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (!roleManager.Roles.Any())
            {
                foreach (var role in Enum.GetValues<UserRole>())
                {
                    if (!await roleManager.RoleExistsAsync(role.ToString()))
                    {
                        await roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
                    }
                }
            }

            if (!userManager.Users.Any(x => !string.IsNullOrEmpty(x.Email)))
            {
                var password = "Luantang@123!";
                var newUser = new AppUser()
                {
                    UserName = "luantang",
                    Email = "luantang.work@gmail.com",
                    EmailConfirmed = true,
                };
                var result = await userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, UserRole.User.ToString());
                }

                var newAdmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                };
                var adminResult = await userManager.CreateAsync(newAdmin, password);
                if (adminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, UserRole.Admin.ToString());
                }
            }
        }

        private static async Task SeedCategory(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Category 1", Description = "Description 1" },
                    new Category { Name = "Category 2", Description = "Description 2" },
                    new Category { Name = "Category 3", Description = "Description 3" }
                };  

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedSubCategories(AppDbContext context)
        {
            var parentCategories = await context.Categories
                .Where(c => c.Name == "Category 1" || c.Name == "Category 2")
                .ToListAsync();

            if (!context.Categories.Any(c => c.Level > 0))
            {
                var subCategories = new List<Category>
                {
                    new Category
                    {
                        Name = "SubCategory 1.1",
                        Description = "Sub of Category 1",
                        Level = 1,
                        ParentCategoryId = parentCategories.First(c => c.Name == "Category 1").Id
                    },
                    new Category
                    {
                        Name = "SubCategory 1.2",
                        Description = "Another sub of Category 1",
                        Level = 1,
                        ParentCategoryId = parentCategories.First(c => c.Name == "Category 1").Id
                    },
                    new Category
                    {
                        Name = "SubCategory 2.1",
                        Description = "Sub of Category 2",
                        Level = 1,
                        ParentCategoryId = parentCategories.First(c => c.Name == "Category 2").Id
                    }
                };

                await context.Categories.AddRangeAsync(subCategories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedProducts(AppDbContext context)
        {
            if (!context.Products.Any())
            {
                var existingCategories = await context.Categories
                    .Where(c => c.Name == "Category 1" || c.Name == "Category 2" || c.Name == "Category 3")
                    .ToListAsync();

                var product1 = new Product
                {
                    Name = "Product 1",
                    Description = "Description for product 1",
                    Price = 10.0,
                    InStock = true,
                    StockQuantity = 100,
                    Categories = new List<Category> { existingCategories[0], existingCategories[1] },
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl = "https://down-vn.img.susercontent.com/file/850dbf89d966ffcd43feb9ea148f6634@resize_w900_nl.webp",
                            PublicId = "sample1",
                            IsMain = true
                        },
                        new ProductImage
                        {
                            ImageUrl = "https://down-vn.img.susercontent.com/file/3251977c3cd7ad7dc3b076c88610df27.webp",
                            PublicId = "sample2",
                            IsMain = false
                        }
                    }
                };

                var product2 = new Product
                {
                    Name = "Product 2",
                    Description = "Description for product 2",
                    Price = 20.0,
                    InStock = true,
                    StockQuantity = 200,
                    Categories = new List<Category> { existingCategories[2] },
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl = "https://down-vn.img.susercontent.com/file/be401154665999f78c2b8a177300284d.webp",
                            PublicId = "sample3",
                            IsMain = true
                        },
                        new ProductImage
                        {
                            ImageUrl = "https://down-vn.img.susercontent.com/file/55c1f54f43b0416b5530bb230b55976c.webp",
                            PublicId = "sample4",
                            IsMain = false
                        },
                        new ProductImage
                        {
                            ImageUrl = "https://down-vn.img.susercontent.com/file/f78263b4c5fbd8a97b7f44db7b24cd06.webp",
                            PublicId = "sample5",
                            IsMain = false
                        }
                    }
                };

                await context.Products.AddRangeAsync(product1, product2);
                await context.SaveChangesAsync();
            }
        }

    }
}
