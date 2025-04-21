using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EcommerceNashApp.Core.Exeptions;
using EcommerceNashApp.Core.Settings;
using EcommerceNashApp.Api.Filters;
using EcommerceNashApp.Infrastructure.Exceptions;

namespace EcommerceNashApp.Api.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            return services;
        }

        public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Extract token from the 'jwt' cookie for most requests
                        var token = context.Request.Cookies["jwt"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        throw new AccessDeniedException(ErrorCode.UNAUTHORIZED_ACCESS);
                    },
                    OnForbidden = context =>
                    {
                        throw new AccessDeniedException(ErrorCode.ACCESS_DENIED);
                    }
                };
            });

            return services;
        }
    }
}