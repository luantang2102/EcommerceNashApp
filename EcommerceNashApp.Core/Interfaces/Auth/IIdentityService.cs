using EcommerceNashApp.Core.DTOs.Auth.Request;
using EcommerceNashApp.Core.DTOs.Auth.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceNashApp.Core.Interfaces.Auth
{
    public interface IIdentityService
    {
        Task<AuthResponse> LoginAsync(LoginRequest loginRequest);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto);
    }
}
