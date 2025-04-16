using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class UserExtensions
    {
        public static UserResponse MapModelToResponse(this AppUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                ImageUrl = user.ImageUrl,
                PublicId = user.PublicId,
                Email = user.Email ?? string.Empty,
            };
        }
    }
}
