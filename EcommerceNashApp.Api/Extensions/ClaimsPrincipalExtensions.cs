using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Infrastructure.Exceptions;
using System.Security.Claims;

namespace EcommerceNashApp.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                throw new AppException(ErrorCode.UNAUTHORIZED_ACCESS, new Dictionary<string, object>
                {
                    { "claim", ClaimTypes.NameIdentifier }
                });
            }

            try
            {
                return Guid.Parse(claim.Value);
            }
            catch (FormatException)
            {
                throw new AppException(ErrorCode.INVALID_CLAIM, new Dictionary<string, object>
                {
                    { "claim", claim.Value },
                    { "expectedType", "Guid" }
                });
            }
        }
    }
}