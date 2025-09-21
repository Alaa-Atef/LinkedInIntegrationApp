using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace LinkedInApp.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing request");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Decide response format based on Accept header
                var accept = context.Request.Headers["Accept"].ToString();

                if (accept.Contains("text/html"))
                {
                    // Browser → render Razor error view
                    context.Response.Redirect("/Home/Error");
                }
                else
                {
                    // API client → return JSON
                    context.Response.ContentType = "application/json";

                    var response = new
                    {
                        error = "An unexpected error occurred",
                        details = _env.IsDevelopment() ? ex.Message : "Internal Server Error"
                    };

                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);
                }
            }
        }
    }
}
