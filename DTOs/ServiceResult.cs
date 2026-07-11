namespace dotnet_rest_api.DTOs;

/// <summary>
/// Generic service result wrapper for API responses
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public bool IsSuccess => Success;
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Optional HTTP status hint for failures. When null the controller layer
    /// defaults to 400. Set to 404 for "resource addressed by the URL does not
    /// exist" so BaseApiController.ToApiResponse can map it correctly.
    /// </summary>
    public int? StatusCode { get; set; }

    public ServiceResult()
    {
        Errors = new List<string>();
    }

    public ServiceResult(T data, string? message = null)
    {
        Success = true;
        Data = data;
        Message = message;
        Errors = new List<string>();
    }

    public ServiceResult(string error)
    {
        Success = false;
        Errors = new List<string> { error };
    }

    public ServiceResult(List<string> errors)
    {
        Success = false;
        Errors = errors;
    }

    public static ServiceResult<T> SuccessResult(T data, string? message = null)
    {
        return new ServiceResult<T>(data, message);
    }

    public static ServiceResult<T> ErrorResult(string error)
    {
        return new ServiceResult<T>(error);
    }

    public static ServiceResult<T> ErrorResult(List<string> errors)
    {
        return new ServiceResult<T>(errors);
    }

    /// <summary>Failure representing a missing resource (maps to HTTP 404).</summary>
    public static ServiceResult<T> NotFoundResult(string error)
    {
        return new ServiceResult<T>(error) { StatusCode = 404 };
    }
}