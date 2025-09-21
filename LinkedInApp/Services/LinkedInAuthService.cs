using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace LinkedInApp.Services
{
    public class LinkedInAuthService : ILinkedInAuthService
    {
        private readonly HttpClient _httpClient;
        private const string UserInfoEndpoint = "https://api.linkedin.com/v2/userinfo";

        public LinkedInAuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddUserClaimsAsync(string accessToken, ClaimsIdentity identity)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = user.RootElement;

            if (root.TryGetProperty("name", out var nameProp))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, nameProp.GetString() ?? ""));
            }

            if (root.TryGetProperty("picture", out var pictureProp))
            {
                var pictureUrl = pictureProp.GetString();
                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    identity.AddClaim(new Claim("profile-picture", pictureUrl));
                }
            }
        }
    }
}
