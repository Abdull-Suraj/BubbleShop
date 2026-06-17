using System.Diagnostics;

namespace BubbleShop.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        _logger.LogInformation("HTTP {Method} {Path} started", request.Method, request.Path);

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation("HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                request.Method, request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP {Method} {Path} failed after {ElapsedMs}ms",
                request.Method, request.Path, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}