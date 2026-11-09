using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDayMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<RaceDayApiClient>();
// 2. API HttpClient
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]!);
});

// 3. Access to the current HTTP context
builder.Services.AddHttpContextAccessor();

// 4. Server-side session storage
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);

    options.Cookie.Name = "RaceDayMVC.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 5. MVC authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDayMVC.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// 6. Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// 7. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session must be available before authentication.
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 8. Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();