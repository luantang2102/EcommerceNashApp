using System.Net.Http.Headers;
using EcommerceNashApp.Web.Services;
using EcommerceNashApp.Web.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register IProductService with ProductService
builder.Services.AddScoped<IProductService, ProductService>();

// Configure HttpClient for NashApp.Api
builder.Services.AddHttpClient("NashApp.Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["NashApp:Api:Address"]!);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();