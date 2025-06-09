using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.Common;

/// <summary>
/// Generic result class for service layer operations.
/// Decouples business logic from API response types.
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public List<ValidationError> ValidationErrors { get; private set; } = new();
    public ResultErrorType ErrorType { get; private set; } = ResultErrorType.None;

    protected Result(bool isSuccess, T? data, string message, List<string>? errors = null, 
                  List<ValidationError>? validationErrors = null, ResultErrorType errorType = ResultErrorType.None)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Errors = errors ?? new List<string>();
        ValidationErrors = validationErrors ?? new List<ValidationError>();
        ErrorType = errorType;
    }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static Result<T> Success(T data, string message = "Operation completed successfully")
    {
        return new Result<T>(true, data, message);
    }

    /// <summary>
    /// Creates a successful result without data
    /// </summary>
    public static Result<T> Success(string message = "Operation completed successfully")
    {
        return new Result<T>(true, default(T), message);
    }

    /// <summary>
    /// Creates a failure result with a general error message
    /// </summary>
    public static Result<T> Failure(string message, List<string>? errors = null, ResultErrorType errorType = ResultErrorType.BusinessLogic)
    {
        return new Result<T>(false, default(T), message, errors, null, errorType);
    }

    /// <summary>
    /// Creates a failure result with validation errors
    /// </summary>
    public static Result<T> ValidationFailure(List<ValidationError> validationErrors, string message = "Validation failed")
    {
        return new Result<T>(false, default(T), message, null, validationErrors, ResultErrorType.Validation);
    }

    /// <summary>
    /// Creates a failure result for when a resource is not found
    /// </summary>
    public static Result<T> NotFound(string resource, string identifier)
    {
        var message = $"{resource} with identifier '{identifier}' was not found.";
        return new Result<T>(false, default(T), message, new List<string> { message }, null, ResultErrorType.NotFound);
    }

    /// <summary>
    /// Creates a failure result for unauthorized access
    /// </summary>
    public static Result<T> Unauthorized(string message = "Authentication is required to access this resource.")
    {
        return new Result<T>(false, default(T), message, new List<string> { message }, null, ResultErrorType.Unauthorized);
    }

    /// <summary>
    /// Creates a failure result for forbidden access
    /// </summary>
    public static Result<T> Forbidden(string message = "You do not have permission to access this resource.")
    {
        return new Result<T>(false, default(T), message, new List<string> { message }, null, ResultErrorType.Forbidden);
    }

    /// <summary>
    /// Creates a failure result for server errors
    /// </summary>
    public static Result<T> ServerError(string message = "An internal server error occurred. Please try again later.")
    {
        return new Result<T>(false, default(T), message, new List<string> { message }, null, ResultErrorType.ServerError);
    }

    /// <summary>
    /// Creates a failure result for rate limiting
    /// </summary>
    public static Result<T> RateLimitExceeded(int limit, DateTime resetTime, TimeSpan retryAfter)
    {
        var message = $"Too many requests. You have exceeded the rate limit of {limit} requests. Please try again later.";
        return new Result<T>(false, default(T), message, new List<string> { message }, null, ResultErrorType.RateLimit);
    }
}

/// <summary>
/// Non-generic Result class for operations that don't return data
/// </summary>
public class Result : Result<object>
{
    private Result(bool isSuccess, string message, List<string>? errors = null, 
                  List<ValidationError>? validationErrors = null, ResultErrorType errorType = ResultErrorType.None)
        : base(isSuccess, null, message, errors, validationErrors, errorType)
    {
    }

    /// <summary>
    /// Creates a successful result without data
    /// </summary>
    public new static Result Success(string message = "Operation completed successfully")
    {
        return new Result(true, message);
    }

    /// <summary>
    /// Creates a failure result with a general error message
    /// </summary>
    public new static Result Failure(string message, List<string>? errors = null, ResultErrorType errorType = ResultErrorType.BusinessLogic)
    {
        return new Result(false, message, errors, null, errorType);
    }

    /// <summary>
    /// Creates a failure result with validation errors
    /// </summary>
    public new static Result ValidationFailure(List<ValidationError> validationErrors, string message = "Validation failed")
    {
        return new Result(false, message, null, validationErrors, ResultErrorType.Validation);
    }

    /// <summary>
    /// Creates a failure result for when a resource is not found
    /// </summary>
    public new static Result NotFound(string resource, string identifier)
    {
        var message = $"{resource} with identifier '{identifier}' was not found.";
        return new Result(false, message, new List<string> { message }, null, ResultErrorType.NotFound);
    }

    /// <summary>
    /// Creates a failure result for unauthorized access
    /// </summary>
    public new static Result Unauthorized(string message = "Authentication is required to access this resource.")
    {
        return new Result(false, message, new List<string> { message }, null, ResultErrorType.Unauthorized);
    }

    /// <summary>
    /// Creates a failure result for forbidden access
    /// </summary>
    public new static Result Forbidden(string message = "You do not have permission to access this resource.")
    {
        return new Result(false, message, new List<string> { message }, null, ResultErrorType.Forbidden);
    }

    /// <summary>
    /// Creates a failure result for server errors
    /// </summary>
    public new static Result ServerError(string message = "An internal server error occurred. Please try again later.")
    {
        return new Result(false, message, new List<string> { message }, null, ResultErrorType.ServerError);
    }

    /// <summary>
    /// Creates a failure result for rate limiting
    /// </summary>
    public new static Result RateLimitExceeded(int limit, DateTime resetTime, TimeSpan retryAfter)
    {
        var message = $"Too many requests. You have exceeded the rate limit of {limit} requests. Please try again later.";
        return new Result(false, message, new List<string> { message }, null, ResultErrorType.RateLimit);
    }
}

/// <summary>
/// Types of errors that can occur in service operations
/// </summary>
public enum ResultErrorType
{
    None,
    Validation,
    BusinessLogic,
    NotFound,
    Unauthorized,
    Forbidden,
    ServerError,
    RateLimit
}

/// <summary>
/// Validation error details for field-level validation failures
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}
