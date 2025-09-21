using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;

namespace LinkedInApp.Services
{
    public class ImageOverlayService : IImageOverlayService
    {
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ImageOverlayService> _logger;

        public ImageOverlayService(IWebHostEnvironment env, HttpClient httpClient, ILogger<ImageOverlayService> logger)
        {
            _env = env;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> CreateProfileImageAsync(string profilePicUrl, string name)
        {
            _logger.LogInformation("Starting image overlay for user {UserName}", name);

            try
            {
                // 1. Load background
                var bgPath = Path.Combine(_env.WebRootPath, "backgrounds", "background.png");
                using var background = await Image.LoadAsync(bgPath);

                // 2. Load profile picture from URL using injected HttpClient
                using var profileStream = await _httpClient.GetStreamAsync(profilePicUrl);
                using var profilePic = await Image.LoadAsync(profileStream);

                // Resize profile picture to 200x200
                profilePic.Mutate(x => x.Resize(300, 300));

                int picX = (background.Width - profilePic.Width) / 2;
                int picY = (background.Height / 2) - 200;

                // 3. Overlay profile picture
                background.Mutate(x =>
                {
                    // Place profile picture at bottom-left corner
                    x.DrawImage(profilePic, new Point(picX, picY), 1f);
                });

                // 4. Load custom font and draw name
                var fontPath = Path.Combine(_env.WebRootPath, "fonts", "Inter_24pt-Regular.ttf");
                var fontCollection = new FontCollection();
                var family = fontCollection.Add(fontPath);
                var font = family.CreateFont(36);

                background.Mutate(x =>
                {
                    x.DrawText(name, font, Color.Black, new PointF(300, background.Height - 180));
                });

                // 5. Save to /wwwroot/generated
                var outputDir = Path.Combine(_env.WebRootPath, "generated");
                Directory.CreateDirectory(outputDir);

                var fileName = $"profile_{Guid.NewGuid()}.png";
                var filePath = Path.Combine(outputDir, fileName);

                await background.SaveAsPngAsync(filePath);

                // Return relative path for serving in UI
                _logger.LogInformation("Successfully created image for {UserName}", name);
                return $"/generated/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create profile image for {UserName}", name);
                throw;
            }
        }
    }
}
