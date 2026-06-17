// Application/Common/Models/Result.cs
namespace BubbleShop.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }
    public string? Value { get; protected set; }
    public string? ErrorCode { get; protected set; }
    public List<string> Errors { get; protected set; } = new();
    public bool IsFailure => !IsSuccess;

    protected Result() { }

    public static Result Success() => new Result { IsSuccess = true };

    // 1 argument: error message only
    public static Result Failure(string error)
    {
        return new Result
        {
            IsSuccess = false,
            Error = error,
            Errors = new List<string> { error }
        };
    }

    // 2 arguments: error message + error code
    public static Result Failure(string error, string errorCode)
    {
        return new Result
        {
            IsSuccess = false,
            Error = error,
            ErrorCode = errorCode,
            Errors = new List<string> { error }
        };
    }

    // List of errors
    public static Result Failure(List<string> errors)
    {
        return new Result
        {
            IsSuccess = false,
            Error = errors.FirstOrDefault(),
            Errors = errors
        };
    }
}

public class Result<T> : Result
{
    public T? Value { get; private set; }

    private Result() { }

    public static Result<T> Success(T value)
    {
        return new Result<T> { IsSuccess = true, Value = value };
    }

    // 1 argument: error message only
    public static new Result<T> Failure(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            Errors = new List<string> { error }
        };
    }

    // 2 arguments: error message + error code
    public static Result<T> Failure(string error, string errorCode)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorCode = errorCode,
            Errors = new List<string> { error }
        };
    }

    // List of errors
    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = errors.FirstOrDefault(),
            Errors = errors
        };
    }
}