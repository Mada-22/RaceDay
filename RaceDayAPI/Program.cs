
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using RaceDay.Services;
using RaceDayAPI.Data;
using RaceDayAPI.Service;
using RaceDayAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. Controllers and Swagger
// --------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Cookie authentication is handled through the HTTP cookie.
    // JWT Bearer authentication is no longer configured here.
});

// --------------------------------------------------
// 2. Database connection
// --------------------------------------------------

builder.Services.AddDbContext<RaceDayDBContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------
// 3. Cookie authentication
// --------------------------------------------------

builder.Services
.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.Cookie.Name = "RaceDay.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;


    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;

    // Return HTTP status codes instead of redirecting API clients.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode =
            StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    };
});


// --------------------------------------------------
// 4. Authorization
// --------------------------------------------------

builder.Services.AddAuthorization();

// --------------------------------------------------
// 5. Application service registrations
// --------------------------------------------------

builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ParticipantService>();
builder.Services.AddScoped<OrganizerService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RouteService>();
builder.Services.AddScoped<EntryService>();
builder.Services.AddScoped<ResultService>();

// --------------------------------------------------
// 6. Build application
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// 7. Swagger middleware
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------------------------------------
// 8. HTTP pipeline
// --------------------------------------------------

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --------------------------------------------------
// 9. Start application
// --------------------------------------------------

app.Run();

