namespace BubbleShop.API.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("Incoming {Method} {Path}", context.Request.Method, context.Request.Path);
        await next(context);
    }
}
