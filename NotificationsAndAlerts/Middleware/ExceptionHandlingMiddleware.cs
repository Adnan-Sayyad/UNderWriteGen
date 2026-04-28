using System.Net;
using System.Text.Json;
using NotificationsAndAlerts.Models.DTOs;

namespace NotificationsAndAlerts.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Notifications service");
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var body = JsonSerializer.Serialize(
                    ApiResponse<object>.Fail("An unexpected error occurred", new List<string> { ex.Message }));

                await ctx.Response.WriteAsync(body);
            }
        }
    }
}
