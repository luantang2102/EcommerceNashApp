using Microsoft.OpenApi.Models;

namespace EcommerceNashApp.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ecommerce Nash API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
                {
                    Name = "jwt",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Description = "JWT token is stored in an HTTP-only cookie named 'jwt'."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "CookieAuth"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}