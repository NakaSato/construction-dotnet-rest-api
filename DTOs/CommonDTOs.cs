using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

// Authentication DTOs
public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive number")]
    public int RoleId { get; set; }
}

// User DTOs
public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateUserRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive number")]
    public int RoleId { get; set; }
}

// Project DTOs
public class ProjectDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ClientInfo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public UserDto ProjectManager { get; set; } = null!;
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
}

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public string ProjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Client info cannot exceed 1000 characters")]
    public string ClientInfo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    [Required(ErrorMessage = "Project manager ID is required")]
    public Guid ProjectManagerId { get; set; }
}

public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public string ProjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Client info cannot exceed 1000 characters")]
    public string ClientInfo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression(@"^(Active|Completed|On Hold|Cancelled)$", ErrorMessage = "Status must be one of: Active, Completed, On Hold, Cancelled")]
    public string Status { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    [Required(ErrorMessage = "Project manager ID is required")]
    public Guid ProjectManagerId { get; set; }
}

// Task DTOs
public class TaskDto
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public UserDto? AssignedTechnician { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}

public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression(@"^(Pending|In Progress|Completed|Cancelled)$", ErrorMessage = "Status must be one of: Pending, In Progress, Completed, Cancelled")]
    public string Status { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}

// Image DTOs
public class ImageMetadataDto
{
    public Guid ImageId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public DateTime UploadTimestamp { get; set; }
    public DateTime? CaptureTimestamp { get; set; }
    public double? GPSLatitude { get; set; }
    public double? GPSLongitude { get; set; }
    public string? DeviceModel { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public UserDto UploadedBy { get; set; } = null!;
}

public class ImageUploadRequest
{
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime? CaptureTimestamp { get; set; }
    public double? GPSLatitude { get; set; }
    public double? GPSLongitude { get; set; }
    public string? DeviceModel { get; set; }
    public string? EXIFData { get; set; }
}

// Error Response DTOs
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public object? AttemptedValue { get; set; }
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Target { get; set; }
    public List<ValidationError> Details { get; set; } = new();
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}

public class ApiError
{
    public string Type { get; set; } = string.Empty; // ValidationError, BusinessLogicError, SystemError, etc.
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string? Instance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public List<ErrorDetail> Errors { get; set; } = new();
    public Dictionary<string, object> Extensions { get; set; } = new();
}

// Common DTOs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public ApiError? Error { get; set; }

    // Factory methods for creating consistent responses
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ValidationErrorResponse(List<ValidationError> validationErrors, string? instance = null, string? traceId = null)
    {
        var errorDetail = new ErrorDetail
        {
            Code = "VALIDATION_FAILED",
            Message = "One or more validation errors occurred.",
            Details = validationErrors
        };

        var apiError = new ApiError
        {
            Type = "ValidationError",
            Title = "Validation Failed",
            Status = 400,
            Detail = "The request contains invalid data. Please check the errors and try again.",
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Error = apiError,
            Errors = validationErrors.Select(v => $"{v.Field}: {v.Message}").ToList()
        };
    }

    public static ApiResponse<T> BusinessLogicErrorResponse(string code, string message, string? target = null, string? instance = null, string? traceId = null)
    {
        var errorDetail = new ErrorDetail
        {
            Code = code,
            Message = message,
            Target = target
        };

        var apiError = new ApiError
        {
            Type = "BusinessLogicError",
            Title = "Business Logic Error",
            Status = 400,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> NotFoundErrorResponse(string resource, string identifier, string? instance = null, string? traceId = null)
    {
        var message = $"{resource} with identifier '{identifier}' was not found.";
        
        var errorDetail = new ErrorDetail
        {
            Code = "RESOURCE_NOT_FOUND",
            Message = message,
            Target = resource
        };

        var apiError = new ApiError
        {
            Type = "NotFoundError",
            Title = "Resource Not Found",
            Status = 404,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> UnauthorizedErrorResponse(string? instance = null, string? traceId = null)
    {
        var message = "Authentication is required to access this resource.";
        
        var errorDetail = new ErrorDetail
        {
            Code = "UNAUTHORIZED",
            Message = message
        };

        var apiError = new ApiError
        {
            Type = "AuthenticationError",
            Title = "Unauthorized",
            Status = 401,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> ForbiddenErrorResponse(string? instance = null, string? traceId = null)
    {
        var message = "You do not have permission to access this resource.";
        
        var errorDetail = new ErrorDetail
        {
            Code = "FORBIDDEN",
            Message = message
        };

        var apiError = new ApiError
        {
            Type = "AuthorizationError",
            Title = "Forbidden",
            Status = 403,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> ServerErrorResponse(string? instance = null, string? traceId = null)
    {
        var message = "An internal server error occurred. Please try again later.";
        
        var errorDetail = new ErrorDetail
        {
            Code = "INTERNAL_SERVER_ERROR",
            Message = message
        };

        var apiError = new ApiError
        {
            Type = "SystemError",
            Title = "Internal Server Error",
            Status = 500,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }

    public static ApiResponse<T> CreateRateLimitErrorResponse(int limit, DateTime resetTime, TimeSpan retryAfter, string? instance = null, string? traceId = null)
    {
        var message = $"Too many requests. You have exceeded the rate limit of {limit} requests. Please try again later.";
        
        var errorDetail = new ErrorDetail
        {
            Code = "RATE_LIMIT_EXCEEDED",
            Message = message,
            AdditionalInfo = new Dictionary<string, object>
            {
                { "limit", limit },
                { "resetTime", resetTime },
                { "retryAfterSeconds", (int)retryAfter.TotalSeconds }
            }
        };

        var apiError = new ApiError
        {
            Type = "RateLimitError",
            Title = "Too Many Requests",
            Status = 429,
            Detail = message,
            Instance = instance,
            TraceId = traceId,
            Errors = new List<ErrorDetail> { errorDetail },
            Extensions = new Dictionary<string, object>
            {
                { "rateLimit", new
                    {
                        limit = limit,
                        resetTime = resetTime,
                        retryAfterSeconds = (int)retryAfter.TotalSeconds
                    }
                }
            }
        };

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Error = apiError,
            Errors = new List<string> { message }
        };
    }
}

// HATEOAS-style pagination links
public class PaginationLinks
{
    public string? First { get; set; }
    public string? Previous { get; set; }
    public string? Current { get; set; }
    public string? Next { get; set; }
    public string? Last { get; set; }
}

// Enhanced pagination metadata
public class PaginationInfo
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
    public PaginationLinks Links { get; set; } = new();
}

// Legacy PagedResult for backward compatibility
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// Enhanced paged result with rich pagination and HATEOAS links
public class EnhancedPagedResult<T> : PagedResult<T>
{
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public List<string> RequestedFields { get; set; } = new();
    public QueryMetadata Metadata { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

// New rich API response format with enhanced pagination
public class ApiResponseWithPagination<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ApiDataWithPagination<T>? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApiDataWithPagination<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

// Query execution metadata
public class QueryMetadata
{
    public TimeSpan ExecutionTime { get; set; }
    public int FiltersApplied { get; set; }
    public string QueryComplexity { get; set; } = "Simple"; // Simple, Medium, Complex
    public DateTime QueryExecutedAt { get; set; } = DateTime.UtcNow;
    public string CacheStatus { get; set; } = "Miss"; // Hit, Miss, Partial
}
