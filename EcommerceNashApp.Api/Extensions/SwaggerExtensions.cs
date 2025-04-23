using EcommerceNashApp.Api.Filters;
using Microsoft.OpenApi.Models;

namespace EcommerceNashApp.Api.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
             {
                 options.SwaggerDoc("v1", new OpenApiInfo { Title = "EcommerceNashApp API", Version = "v1" });

                 // Add JWT cookie authentication
                 options.AddSecurityDefinition("JwtCookie", new OpenApiSecurityScheme
                 {
                     Name = "jwt",
                     In = ParameterLocation.Cookie,
                     Type = SecuritySchemeType.ApiKey,
                     Description = "JWT token in 'jwt' cookie"
                 });

                 // Add CSRF token requirement for non-GET requests
                 options.AddSecurityDefinition("CsrfToken", new OpenApiSecurityScheme
                 {
                     Name = "X-CSRF-Token",
                     In = ParameterLocation.Header,
                     Type = SecuritySchemeType.ApiKey,
                     Description = "CSRF token matching 'csrf' cookie for POST/PUT/DELETE requests"
                 });

                 // Apply security requirements
                 options.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "JwtCookie" }
                         },
                         new List<string>()
                     },
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CsrfToken" }
                         },
                         new List<string>()
                     }
                 });

                 // Document common responses
                 options.OperationFilter<SwaggerResponseFilter>();
             });

            return services;
        }
    }
}