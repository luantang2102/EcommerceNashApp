using System.Net.Http.Headers;
using EcommerceNashApp.Core.Models.Auth;
using EcommerceNashApp.Web.Services;
using EcommerceNashApp.Web.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register IProductService with ProductService
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var apiAddress = builder.Configuration["NashApp:Api:Address"];
if (string.IsNullOrEmpty(apiAddress))
{
    throw new InvalidOperationException("The API address is not configured in the application settings.");
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole(UserRole.User.ToString()));
});

builder.Services.AddHttpClient("NashApp.Api", client =>
{
    client.BaseAddress = new Uri(apiAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddAuthentication("CookieAuth")
        .AddCookie("CookieAuth", options =>
        {
            options.Cookie.Name = "jwt";
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
        });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()
               .WithExposedHeaders("pagination"); // Lowercase
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();