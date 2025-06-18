namespace dotnet_rest_api.DTOs;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual data payload (null if error or no data)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of error messages (empty if successful)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Additional error information for debugging
    /// </summary>
    public object? Error { get; set; }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Creates an error response with message and errors
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates a validation error response
    /// </summary>
    public static ApiResponse<T> ValidationErrorResponse(List<string> validationErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Data = default,
            Errors = validationErrors,
            Error = new { ValidationErrors = validationErrors }
        };
    }

    /// <summary>
    /// Creates a rate limit error response
    /// </summary>
    public static ApiResponse<T> CreateRateLimitErrorResponse(int limit, DateTimeOffset resetTime, TimeSpan retryAfter, string? endpoint = null, string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Rate limit exceeded",
            Data = default,
            Errors = new List<string> { "Too many requests" },
            Error = new 
            { 
                RateLimit = new 
                {
                    Limit = limit,
                    ResetTime = resetTime,
                    RetryAfter = retryAfter,
                    Endpoint = endpoint,
                    TraceId = traceId
                }
            }
        };
    }
}

/// <summary>
/// Non-generic version of ApiResponse for operations that don't return data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Data = null,
            Errors = new List<string>()
        };
    }

    /// <summary>
    /// Creates an error response without data
    /// </summary>
    public static new ApiResponse ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Data = null,
            Errors = errors ?? new List<string>()
        };
    }
}
