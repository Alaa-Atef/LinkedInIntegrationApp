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
                var bgPath = Path.Combine(_env.WebRootPath, "backgrounds", "background.png");
                using var background = await Image.LoadAsync(bgPath);

                using var profileStream = await _httpClient.GetStreamAsync(profilePicUrl);
                using var profilePic = await Image.LoadAsync(profileStream);

                profilePic.Mutate(x => x.Resize(300, 300));

                int picX = (background.Width - profilePic.Width) / 2;
                int picY = (background.Height / 2) - 200;

                background.Mutate(x =>
                {
                    x.DrawImage(profilePic, new Point(picX, picY), 1f);
                });

                var fontPath = Path.Combine(_env.WebRootPath, "fonts", "Inter_24pt-Regular.ttf");
                var fontCollection = new FontCollection();
                var family = fontCollection.Add(fontPath);
                var font = family.CreateFont(36);

                background.Mutate(x =>
                {
                    x.DrawText(name, font, Color.Black, new PointF(300, background.Height - 180));
                });

                var outputDir = Path.Combine(_env.WebRootPath, "generated");
                Directory.CreateDirectory(outputDir);

                var fileName = $"profile_{Guid.NewGuid()}.png";
                var filePath = Path.Combine(outputDir, fileName);

                await background.SaveAsPngAsync(filePath);

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
