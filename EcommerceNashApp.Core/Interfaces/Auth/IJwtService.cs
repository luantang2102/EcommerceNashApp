using EcommerceNashApp.Core.Models.Auth;

namespace EcommerceNashApp.Core.Interfaces.Auth
{
    public interface IJwtService
    {
        string GenerateRefreshToken();
        string GenerateToken(AppUser user, IList<string> roles);
    }
}
