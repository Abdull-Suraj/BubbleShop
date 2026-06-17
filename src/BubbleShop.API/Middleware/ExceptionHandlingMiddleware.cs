using System.Text.Json;
using BubbleShop.API.Common;
using BubbleShop.Domain.Exceptions;

namespace BubbleShop.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var apiResponse = exception switch
        {
            DomainException domainEx => new ApiResponse<object>
            {
                Success = false,
                Message = domainEx.Message,
                StatusCode = 400
            },
            KeyNotFoundException notFoundEx => new ApiResponse<object>
            {
                Success = false,
                Message = notFoundEx.Message,
                StatusCode = 404
            },
            UnauthorizedAccessException authEx => new ApiResponse<object>
            {
                Success = false,
                Message = authEx.Message ?? "Unauthorized access",
                StatusCode = 401
            },
            _ => new ApiResponse<object>
            {
                Success = false,
                Message = "An internal error occurred. Please try again later.",
                StatusCode = 500
            }
        };

        response.StatusCode = apiResponse.StatusCode ?? 500;
        var json = JsonSerializer.Serialize(apiResponse);
        await response.WriteAsync(json);
    }
}