using LinkedInApp.Controllers;
using LinkedInApp.Models;
using LinkedInApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace LinkedInApp.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Profile_WithValidPicture_ShouldReturnViewWithGeneratedImage()
        {
            // Arrange
            var mockService = new Mock<IImageOverlayService>();
            mockService.Setup(s => s.CreateProfileImageAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync("/images/generated/test.png");

            var controller = new HomeController(mockService.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim("profile-picture", "http://test.com/profile.jpg")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfileResult>(viewResult.Model);
            Assert.Equal("John Doe", model.Name);
            Assert.Equal("/images/generated/test.png", model.ImagePath);
        }


        [Fact]
        public async Task Profile_WithoutProfilePicture_ShouldReturnDefaultImage()
        {
            // Arrange
            var mockService = new Mock<IImageOverlayService>();

            var controller = new HomeController(mockService.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Jane Doe")
                // Notice: no profile-picture claim
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfileResult>(viewResult.Model);
            Assert.Equal("Jane Doe", model.Name);
            Assert.Equal("/images/default-profile.jpg", model.ImagePath);
        }

    }
}
