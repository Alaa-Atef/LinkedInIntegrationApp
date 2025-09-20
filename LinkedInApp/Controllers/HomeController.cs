using LinkedInApp.Models;
using LinkedInApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkedInApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ImageOverlayService _imageService;

        public HomeController(ImageOverlayService imageService)
        {
            _imageService = imageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var name = User.Identity?.Name ?? "Unknown";
            var pictureUrl = User.Claims.FirstOrDefault(c => c.Type == "profile-picture")?.Value;

            if (string.IsNullOrEmpty(pictureUrl))
            {
                var placeholder = new ProfileResult
                {
                    Name = name,
                    ImagePath = "/images/default-profile.jpg" 
                };
                return View(placeholder);
            }

            var generatedPath = await _imageService.CreateProfileImageAsync(pictureUrl, name);

            var model = new ProfileResult
            {
                Name = name,
                ImagePath = generatedPath
            };

            return View(model);
        }
    }
}
