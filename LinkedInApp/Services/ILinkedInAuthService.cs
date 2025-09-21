using System.Security.Claims;

namespace LinkedInApp.Services
{
    public interface ILinkedInAuthService
    {
        Task AddUserClaimsAsync(string accessToken, ClaimsIdentity identity);
    }
}
