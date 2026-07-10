// Application/Common/Models/Result.cs
namespace BubbleShop.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; protected set; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; protected set; }

    public string? ErrorCode { get; protected set; }

    public List<string> Errors { get; protected set; } = new();

    protected Result() { }

    public static Result Success()
        => new() { IsSuccess = true };

    public static Result Failure(string error)
        => new()
        {
            Error = error,
            Errors = new() { error }
        };

    public static Result Failure(string error, string errorCode)
        => new()
        {
            Error = error,
            ErrorCode = errorCode,
            Errors = new() { error }
        };

    public static Result Failure(List<string> errors)
        => new()
        {
            Error = errors.FirstOrDefault(),
            Errors = errors
        };
}

public class Result<T> : Result
{
    public T Value { get; private set; } = default!;

    private Result() { }

    public static Result<T> Success(T value)
        => new()
        {
            IsSuccess = true,
            Value = value
        };

    public static new Result<T> Failure(string error)
        => new()
        {
            Error = error,
            Errors = new() { error }
        };

    public static new Result<T> Failure(List<string> errors)
        => new()
        {
            Error = errors.FirstOrDefault(),
            Errors = errors
        };
}