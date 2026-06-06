namespace BubbleShop.API.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var method = Sanitize(context.Request.Method);
        var path = Sanitize(context.Request.Path.ToString());
        logger.LogInformation("Incoming {Method} {Path}", method, path);
        await next(context);
    }

    private static string Sanitize(string value) => value.Replace("\r", string.Empty).Replace("\n", string.Empty);
}
