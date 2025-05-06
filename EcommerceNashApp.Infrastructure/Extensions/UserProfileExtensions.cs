using EcommerceNashApp.Core.Models.Extended;
using EcommerceNashApp.Shared.DTOs.Response;

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
                Address = userProfile.Address.ToString(),
                CreatedDate = userProfile.CreatedDate,
                UpdatedDate = userProfile.UpdatedDate,
                UserId = userProfile.UserId,
                UserName = userProfile.User.UserName,
                Email = userProfile.User.Email,
            };
        }
    }
}
