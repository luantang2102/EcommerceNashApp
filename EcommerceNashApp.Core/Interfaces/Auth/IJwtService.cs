using EcommerceNashApp.Core.Models.Identity;

namespace EcommerceNashApp.Core.Interfaces.Auth
{
    public interface IJwtService
    {
        string GenerateRefreshToken();
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
