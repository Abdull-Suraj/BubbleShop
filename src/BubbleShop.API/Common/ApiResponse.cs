namespace BubbleShop.API.Common;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int? StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Created(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message ?? "Resource created successfully",
            Data = data,
            StatusCode = 201
        };
    }

    public static ApiResponse<T> Fail(string error, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = error,
            Errors = new List<string> { error },
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> NotFound(string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Resource not found",
            StatusCode = 404
        };
    }

    public static ApiResponse<T> Unauthorized(string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message ?? "Unauthorized access",
            StatusCode = 401
        };
    }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}