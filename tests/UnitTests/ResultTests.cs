using System.Collections.Generic;
using dotnet_rest_api.Common;
using Xunit;

namespace UnitTests;

/// <summary>
/// Behavior of the canonical service-layer result type (Common/Result.cs).
/// Pins the success/failure shape and error-type mapping that BaseApiController
/// relies on when translating results into HTTP responses.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_WithData_IsSuccess_And_CarriesData()
    {
        var r = Result<int>.Success(42, "ok");

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Equal(42, r.Data);
        Assert.Equal("ok", r.Message);
        Assert.Equal(ResultErrorType.None, r.ErrorType);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Failure_IsFailure_And_DefaultsToBusinessLogic()
    {
        var r = Result<int>.Failure("bad");

        Assert.True(r.IsFailure);
        Assert.False(r.IsSuccess);
        Assert.Equal(ResultErrorType.BusinessLogic, r.ErrorType);
        Assert.Equal("bad", r.Message);
    }

    [Fact]
    public void NotFound_SetsNotFoundErrorType_And_DescriptiveMessage()
    {
        var r = Result<string>.NotFound("Project", "abc");

        Assert.True(r.IsFailure);
        Assert.Equal(ResultErrorType.NotFound, r.ErrorType);
        Assert.Contains("Project", r.Message);
        Assert.Contains("abc", r.Message);
    }

    [Theory]
    [InlineData(ResultErrorType.Unauthorized)]
    [InlineData(ResultErrorType.Forbidden)]
    [InlineData(ResultErrorType.ServerError)]
    public void Failure_Factories_SetMatchingErrorType(ResultErrorType expected)
    {
        Result<string> r = expected switch
        {
            ResultErrorType.Unauthorized => Result<string>.Unauthorized(),
            ResultErrorType.Forbidden => Result<string>.Forbidden(),
            _ => Result<string>.ServerError(),
        };

        Assert.Equal(expected, r.ErrorType);
        Assert.True(r.IsFailure);
    }

    [Fact]
    public void ValidationFailure_CarriesFieldErrors()
    {
        var errors = new List<ValidationError>
        {
            new() { Field = "Name", Message = "required" },
        };

        var r = Result<string>.ValidationFailure(errors);

        Assert.Equal(ResultErrorType.Validation, r.ErrorType);
        Assert.Single(r.ValidationErrors);
        Assert.Equal("Name", r.ValidationErrors[0].Field);
    }

    [Fact]
    public void NonGeneric_Result_Success_HasNoData()
    {
        var r = Result.Success("done");

        Assert.True(r.IsSuccess);
        Assert.Null(r.Data);
        Assert.Equal("done", r.Message);
    }
}
