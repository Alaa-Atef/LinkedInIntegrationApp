# LinkedIn Profile Image Generator

A .NET 8 MVC web application that allows users to sign in with LinkedIn, fetch their profile information, and generate a customized profile image with a branded overlay.

## 🚀 Features

* **Authentication with LinkedIn (OAuth 2.0 / OpenID Connect)**

  * Sign in using your LinkedIn account
  * Retrieves user name, and profile picture

* **Custom Profile Image Generator**

  * Overlays user’s profile picture and name on a background template
  * Uses [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) for image manipulation
  * Includes custom fonts and assets

* **Clean Architecture & Best Practices**

  * Separation of concerns via service classes (`ImageOverlayService`, `LinkedInAuthService`)
  * Centralized error handling with custom middleware
  * Logging with [Serilog](https://serilog.net/) (console + rolling file logs)
  * Configurable via `appsettings.json` and environment variables

* **Unit Tests**

  * xUnit + Moq for testing services
  * Fake HTTP client responses for deterministic tests
  * Covers image generation logic

## 🛠️ Tech Stack

* **Backend:** ASP.NET Core 8 (MVC)
* **Frontend:** Razor Views (Bootstrap 5)
* **Authentication:** LinkedIn OAuth/OpenID Connect
* **Image Processing:** SixLabors.ImageSharp
* **Logging:** Serilog
* **Testing:** xUnit, Moq

## 📂 Project Structure

```
LinkedInApp/                # Main web application
 ├── Controllers/           # MVC Controllers
 ├── Services/              # Business logic & external API integrations
 ├── Middleware/            # Global exception handling
 ├── Views/                 # Razor views (UI)
 ├── wwwroot/               # Static files (backgrounds, fonts, generated images)
LinkedInApp.Tests/          # Unit tests project
```

## ⚡ Getting Started

### Prerequisites

* .NET 8 SDK
* Visual Studio 2022 / Rider / VS Code
* LinkedIn Developer Account + OAuth App

### Setup

1. Clone the repo:

   ```bash
   git clone https://github.com/Alaa-Atef/LinkedInIntegrationApp.git
   cd LinkedInApp
   ```

2. Configure LinkedIn OAuth credentials in `appsettings.json`:

   ```json
   "Authentication": {
     "LinkedIn": {
       "ClientId": "YOUR_CLIENT_ID",
       "ClientSecret": "YOUR_CLIENT_SECRET"
     }
   }
   ```

3. Run the app:

   ```bash
   dotnet run --project LinkedInApp
   ```

4. Navigate to:

   ```
   https://localhost:7220
   ```

## ✅ Unit Tests

Run all tests:

```bash
dotnet test
```

The tests cover:

* Profile image generation (`ImageOverlayService`)
* Mocked LinkedIn API responses (`LinkedInAuthService`)

## 🤝 Contribution

This project was built as part of a technical exercise and demonstrates best practices in:

* OAuth authentication
* Service-based architecture
* Error handling and logging
* Unit testing with xUnit
