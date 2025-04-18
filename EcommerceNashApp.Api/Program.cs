using EcommerceNashApp.Api.Extensions;
using EcommerceNashApp.Api.Filters;
using EcommerceNashApp.Api.SeedData;
using EcommerceNashApp.Core.Helpers.Configurations;
using EcommerceNashApp.Core.Interfaces;
using EcommerceNashApp.Core.Interfaces.Auth;
using EcommerceNashApp.Core.Models.Identity;
using EcommerceNashApp.Core.Settings;
using EcommerceNashApp.Core.Validators;
using EcommerceNashApp.Infrastructure.Data;
using EcommerceNashApp.Infrastructure.Services;
using EcommerceNashApp.Infrastructure.Services.Auth;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

/// Add services to the container.

// Add Controller and Validation filters 
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// Disable automatic model state error response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Swagger/OpenAPI 
builder.Services.AddCustomSwagger();


// Bind settings from appsettings.json
builder.Services.Configure<CloudinaryConfig>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Setup ASP.NET Core Identity
builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

// Enable CORS (configure in middleware if needed)
builder.Services.AddCors();

// Dependency injection for services
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRatingService, RatingService>();

// Exception handling and authentication
builder.Services.AddGlobalExceptionHandling();
builder.Services.AddCustomJwtAuthentication(jwtSettings);

// FluentValidation setup
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ProductRequestValidator>();

var app = builder.Build();

// Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce Nash API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware pipeline
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// Enable CORS
app.UseCors(cors =>
{
    cors.AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins("https://localhost:3000");
});
    
// Map controller routes
app.MapControllers();

// Seed initial database data
await DbInitializer.InitDb(app);

app.Run();
