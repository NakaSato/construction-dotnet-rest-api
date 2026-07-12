using dotnet_rest_api.Common;
using dotnet_rest_api.Controllers;
using dotnet_rest_api.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace UnitTests;

/// <summary>
/// Pins the Result -> HTTP mapping in BaseApiController: success envelopes,
/// the 400-default / explicit-status-hint failure path, and 201-on-create.
/// A minimal concrete subclass exposes the protected helpers.
/// </summary>
public class BaseApiControllerTests
{
    private sealed class TestController : BaseApiController
    {
        public ActionResult<ApiResponse<T>> Ok<T>(Result<T> r) => ToApiResponse(r);
        public ActionResult<ApiResponse<T>> Created<T>(Result<T> r, string? loc = null) => ToCreatedResponse(r, loc);
        public string? Paging(int page, int size) => ValidatePaginationParameters(page, size);
    }

    private static ObjectResult AsObjectResult<T>(ActionResult<ApiResponse<T>> action)
        => Assert.IsAssignableFrom<ObjectResult>(action.Result);

    [Fact]
    public void ToApiResponse_Success_Returns200_WithData()
    {
        var res = new TestController().Ok(Result<string>.SuccessResult("hi", "ok"));

        var obj = AsObjectResult(res);
        Assert.Equal(StatusCodes.Status200OK, obj.StatusCode);
        var body = Assert.IsType<ApiResponse<string>>(obj.Value);
        Assert.True(body.Success);
        Assert.Equal("hi", body.Data);
    }

    [Fact]
    public void ToApiResponse_Failure_DefaultsTo400()
    {
        var res = new TestController().Ok(Result<string>.ErrorResult("nope"));

        var obj = AsObjectResult(res);
        Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        var body = Assert.IsType<ApiResponse<string>>(obj.Value);
        Assert.False(body.Success);
        Assert.Contains("nope", body.Errors);
    }

    [Fact]
    public void ToApiResponse_NotFound_HonorsStatusHint404()
    {
        var res = new TestController().Ok(Result<string>.NotFoundResult("gone"));

        var obj = AsObjectResult(res);
        Assert.Equal(StatusCodes.Status404NotFound, obj.StatusCode);
    }

    [Fact]
    public void ToCreatedResponse_Success_Returns201()
    {
        var res = new TestController().Created(Result<string>.SuccessResult("made"));

        var obj = AsObjectResult(res);
        Assert.Equal(StatusCodes.Status201Created, obj.StatusCode);
    }

    [Fact]
    public void ToCreatedResponse_Failure_Returns400()
    {
        var res = new TestController().Created(Result<string>.ErrorResult("bad"));

        var obj = AsObjectResult(res);
        Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
    }

    [Theory]
    [InlineData(0, 10)]     // page < 1
    [InlineData(1, 0)]      // pageSize < 1
    [InlineData(1, 101)]    // pageSize > 100
    public void ValidatePagination_RejectsOutOfRange(int page, int size)
    {
        Assert.NotNull(new TestController().Paging(page, size));
    }

    [Fact]
    public void ValidatePagination_AcceptsValidRange()
    {
        Assert.Null(new TestController().Paging(1, 20));
    }
}
