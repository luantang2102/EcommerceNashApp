using EcommerceNashApp.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace EcommerceNashApp.Infrastructure.Services
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }


    }
}
