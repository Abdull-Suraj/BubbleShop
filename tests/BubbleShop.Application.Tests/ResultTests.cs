using BubbleShop.Application.Common.Models;

namespace BubbleShop.Application.Tests;

public sealed class ResultTests
{
    [Fact]
    public void SuccessResult_ShouldContainValue()
    {
        var result = Result<int>.Success(12);
        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value);
    }

    [Fact]
    public void FailureResult_ShouldContainError()
    {
        var result = Result<int>.Failure("error");
        Assert.True(result.IsFailure);
        Assert.Equal("error", result.Error);
    }
}
