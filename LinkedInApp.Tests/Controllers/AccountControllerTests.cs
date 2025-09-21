using LinkedInApp.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LinkedInApp.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public void Login_ShouldReturnChallengeResult_WithLinkedInScheme()
        {
            // Arrange
            var controller = new AccountController();

            // Act
            var result = controller.Login();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.Equal("LinkedIn", challengeResult.AuthenticationSchemes.First());
            Assert.Equal("/", challengeResult.Properties.RedirectUri);
        }
    }
}
