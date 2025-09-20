namespace LinkedInApp.Services
{
    public interface IImageOverlayService
    {
        Task<string> CreateProfileImageAsync(string profilePicUrl, string name);
    }
}
