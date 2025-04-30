using EcommerceNashApp.Core.DTOs.Response;
using EcommerceNashApp.Core.Models.Extended;

namespace EcommerceNashApp.Infrastructure.Extensions
{
    public static class UserProfileExtensions
    {
        public static UserProfileResponse MapModelToResponse(this UserProfile userProfile)
        {
            return new UserProfileResponse
            {
                Id = userProfile.Id,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                PhoneNumber = userProfile.PhoneNumber,
                Address = userProfile.Address,
                CreatedDate = userProfile.CreatedDate,
                UpdatedDate = userProfile.UpdatedDate,
                UserId = userProfile.UserId,
                UserName = userProfile.User.UserName,
                Email = userProfile.User.Email,
            };
        }
    }
}
