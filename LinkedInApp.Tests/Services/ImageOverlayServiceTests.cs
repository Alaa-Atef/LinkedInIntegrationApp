using LinkedInApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace LinkedInApp.Tests.Services
{
    public class ImageOverlayServiceTests
    {
        [Fact]
        public async Task CreateProfileImageAsync_ShouldGenerateImageFile()
        {
            // Arrange
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath)
                .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            var loggerMock = new Mock<ILogger<ImageOverlayService>>();

            // Create a fake HttpClient that returns a simple image
            using var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(100, 100);
            using var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            ms.Position = 0;

            var handler = new FakeHttpMessageHandler(ms.ToArray());
            var httpClient = new HttpClient(handler);

            var service = new ImageOverlayService(envMock.Object, httpClient, loggerMock.Object);

            var testName = "Unit Test User";
            var testImageUrl = "http://fake.url/profile.png"; // 👈 define dummy URL

            // Act
            var resultPath = await service.CreateProfileImageAsync(testImageUrl, testName);

            // Convert relative web path to actual file system path
            var actualFilePath = Path.Combine(
                envMock.Object.WebRootPath,
                resultPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
            );

            // Assert
            Assert.False(string.IsNullOrEmpty(resultPath));
            Assert.True(File.Exists(actualFilePath), $"Expected file at {actualFilePath}");
        }
    }
}