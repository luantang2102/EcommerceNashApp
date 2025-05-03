using EcommerceNashApp.Web.Services;
using EcommerceNashApp.Web.Services.Impl;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for NashApp API
var apiAddress = builder.Configuration["NashApp:Api:Address"];
if (string.IsNullOrEmpty(apiAddress))
{
    throw new InvalidOperationException("The API address is not configured in the application settings.");
}

builder.Services.AddHttpClient("NashApp.Api", client =>
{
    client.BaseAddress = new Uri(apiAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// Configure authentication with cookie scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "jwt";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(3); // Matches AuthController's jwt cookie
        options.SlidingExpiration = false; // Prevent extending beyond 3 days
        options.Events.OnSigningIn = context =>
        {
            // Ensure jwt cookie uses API's token, not session cookie
            context.Properties.IsPersistent = true;
            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(3);
            return Task.CompletedTask;
        };
    });

// Add antiforgery for CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN"; // Different from API's 'csrf'
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configure CORS to allow only the API origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApi", builder =>
    {
        builder.WithOrigins(apiAddress.TrimEnd('/'))
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .WithExposedHeaders("pagination");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS before authentication/authorization
app.UseCors("AllowApi");

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();