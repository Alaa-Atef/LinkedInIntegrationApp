using LinkedInApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Text.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(); // Replace default logger with Serilog

// Add MVC services
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ImageOverlayService>();
builder.Services.AddHttpClient<IImageOverlayService, ImageOverlayService>();
builder.Services.AddHttpClient<ILinkedInAuthService, LinkedInAuthService>();

// Configure Authentication with Cookie + LinkedIn OpenID Connect
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "LinkedIn";
    })
    .AddCookie()
    .AddOAuth("LinkedIn", options =>
    {
        options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"]!;
        options.CallbackPath = new PathString("/signin-linkedin");

        options.AuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
        options.TokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";

        // Use OpenID Connect scopes instead of r_liteprofile
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.SaveTokens = true;

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var linkedInAuthService = context.HttpContext.RequestServices.GetRequiredService<ILinkedInAuthService>();
                await linkedInAuthService.AddUserClaimsAsync(context.AccessToken!, context.Identity!);
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<LinkedInApp.Middleware.ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("Starting application in {Environment}", builder.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw; // rethrow so the host process knows something went wrong
}
finally
{
    Log.CloseAndFlush();
}