using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ST.Funds.Middleware
{
    public class ApiLogging
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLogging> _logger;

        public ApiLogging(RequestDelegate next, ILogger<ApiLogging> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Incoming Request: {method} {url}",
                context.Request.Method,
                context.Request.Path);

            await _next(context);

            _logger.LogInformation("Outgoing Response: {statusCode}",
                context.Response.StatusCode);
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogging>();
        }
    }

}
