using EcommerceNashApp.Web.Services;
using EcommerceNashApp.Web.Services.Impl;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "nash_session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromDays(3);
    options.Cookie.MaxAge = TimeSpan.FromDays(3);
});

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddHttpContextAccessor();

// Configure HttpClient for NashApp API
var apiAddress = builder.Configuration["NashApp:Api:Address"] ?? "https://localhost:5001";
builder.Services.AddHttpClient("NashApp.Api", client =>
{
    client.BaseAddress = new Uri(apiAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = false,
    AllowAutoRedirect = true,
    UseDefaultCredentials = false
})
.AddHttpMessageHandler<CookieHandler>();

// Add custom cookie handler
builder.Services.AddTransient<CookieHandler>();

// Add custom cookie handler
builder.Services.AddTransient<CookieHandler>();

// Add antiforgery for CSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configure CORS to allow MVC and API origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApi", builder =>
    {
        builder.WithOrigins("https://localhost:3001", "https://localhost:5001")
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

// Enable session and CORS
app.UseSession();
app.UseCors("AllowApi");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Custom handler to log and ensure cookies
public class CookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CookieHandler> _logger;

    public CookieHandler(IHttpContextAccessor httpContextAccessor, ILogger<CookieHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Add cookies from HttpContext directly to the request header instead of relying on CookieContainer
        var httpContextCookies = _httpContextAccessor.HttpContext?.Request.Cookies;
        if (httpContextCookies != null && httpContextCookies.Any())
        {
            var cookieHeader = string.Join("; ", httpContextCookies.Select(c => $"{c.Key}={c.Value}"));
            request.Headers.Add("Cookie", cookieHeader);

            // Log the cookie header we're sending
            _logger.LogDebug("Sending Cookie header: {CookieHeader}",
                string.Join("; ", httpContextCookies.Select(c => $"{c.Key}={c.Value.Substring(0, Math.Min(5, c.Value.Length))}...")));

            // Add CSRF token header if it exists
            if (httpContextCookies.TryGetValue("csrf", out var csrfToken) && !request.Headers.Contains("X-CSRF-TOKEN"))
            {
                request.Headers.Add("X-CSRF-TOKEN", csrfToken);
                _logger.LogDebug("Added X-CSRF-TOKEN header: {CsrfToken}", csrfToken);
            }
        }
        else
        {
            _logger.LogWarning("No cookies found in HttpContext to send with request");
        }

        // Send request
        var response = await base.SendAsync(request, cancellationToken);

        // Process and forward Set-Cookie headers from API response to browser
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var cookie in setCookies)
            {
                _logger.LogDebug("Set-Cookie received: {Cookie}", cookie.Substring(0, Math.Min(50, cookie.Length)) + "...");
                _httpContextAccessor.HttpContext?.Response.Headers.Append("Set-Cookie", cookie);
            }
        }

        return response;
    }
}