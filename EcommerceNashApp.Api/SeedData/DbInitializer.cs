using EcommerceNashApp.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EcommerceNashApp.Api.SeedData
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            // Seed users and roles
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>()
              ?? throw new InvalidOperationException("Failed to retrieve user manager");
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            await SeedUsersAndRoles(userManager, roleManager);
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
    }
}
