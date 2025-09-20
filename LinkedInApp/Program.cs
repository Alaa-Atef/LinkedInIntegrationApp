using LinkedInApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ImageOverlayService>();
builder.Services.AddHttpClient<ImageOverlayService>();

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
        options.UserInformationEndpoint = "https://api.linkedin.com/v2/userinfo";

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
                // Fetch user info
                var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                var response = await context.Backchannel.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var root = user.RootElement;

                // Full name
                if (root.TryGetProperty("name", out var nameProp))
                {
                    context.Identity!.AddClaim(new Claim(ClaimTypes.Name, nameProp.GetString()!));
                }

                // Profile picture
                if (root.TryGetProperty("picture", out var pictureProp))
                {
                    var pictureUrl = pictureProp.GetString();
                    if (!string.IsNullOrEmpty(pictureUrl))
                    {
                        context.Identity!.AddClaim(new Claim("profile-picture", pictureUrl));
                    }
                }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
