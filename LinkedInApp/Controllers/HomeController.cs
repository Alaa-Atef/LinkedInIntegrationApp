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

            if (pictureUrl == null)
            {
                return Content("No profile picture claim found.");
            }

            var generatedPath = await _imageService.CreateProfileImageAsync(pictureUrl, name);
            ViewBag.ImagePath = generatedPath;
            return View();
        }
    }
}
