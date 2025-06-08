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
    [RegularExpression(@"^(Planning|InProgress|Completed|OnHold|Cancelled)$", ErrorMessage = "Status must be one of: Planning, InProgress, Completed, OnHold, Cancelled")]
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

// PATCH DTOs for partial updates
public class PatchProjectRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public string? ProjectName { get; set; }

    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessage = "Client info cannot exceed 1000 characters")]
    public string? ClientInfo { get; set; }

    [RegularExpression(@"^(Planning|InProgress|Completed|OnHold|Cancelled)$", ErrorMessage = "Status must be one of: Planning, InProgress, Completed, OnHold, Cancelled")]
    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public Guid? ProjectManagerId { get; set; }
}

public class PatchTaskRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [RegularExpression(@"^(Pending|In Progress|Completed|Cancelled)$", ErrorMessage = "Status must be one of: Pending, In Progress, Completed, Cancelled")]
    public string? Status { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}

public class PatchUserRequest
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string? Password { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string? FullName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive number")]
    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }
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

// Daily Reports DTOs
public class DailyReportDto
{
    public Guid ReportId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public UserDto CreatedBy { get; set; } = null!;
    public UserDto? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? GeneralNotes { get; set; } = string.Empty;
    
    // Weather Information
    public string? WeatherCondition { get; set; }
    public double? Temperature { get; set; }
    public int? Humidity { get; set; }
    public double? WindSpeed { get; set; }
    
    // Work Progress Summary
    public int TotalWorkHours { get; set; }
    public int PersonnelOnSite { get; set; }
    public string? SafetyIncidents { get; set; } = string.Empty;
    public string? QualityIssues { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related data
    public List<WorkProgressItemDto> WorkProgressItems { get; set; } = new();
    public List<PersonnelLogDto> PersonnelLogs { get; set; } = new();
    public List<MaterialUsageDto> MaterialUsages { get; set; } = new();
    public List<EquipmentLogDto> EquipmentLogs { get; set; } = new();
    public List<ImageMetadataDto> Images { get; set; } = new();
}

public class CreateDailyReportRequest
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Report date is required")]
    public DateTime ReportDate { get; set; }

    [StringLength(2000, ErrorMessage = "General notes cannot exceed 2000 characters")]
    public string? GeneralNotes { get; set; } = string.Empty;

    [RegularExpression(@"^(Sunny|PartlyCloudy|Cloudy|Rainy|Stormy|Foggy|Snow|Windy)$", 
        ErrorMessage = "Weather condition must be one of: Sunny, PartlyCloudy, Cloudy, Rainy, Stormy, Foggy, Snow, Windy")]
    public string? WeatherCondition { get; set; }

    [Range(-40, 50, ErrorMessage = "Temperature must be between -40 and 50 degrees")]
    public double? Temperature { get; set; }

    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100 percent")]
    public int? Humidity { get; set; }

    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double? WindSpeed { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Total work hours must be non-negative")]
    public int TotalWorkHours { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Personnel on site must be non-negative")]
    public int PersonnelOnSite { get; set; }

    [StringLength(1000, ErrorMessage = "Safety incidents cannot exceed 1000 characters")]
    public string? SafetyIncidents { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Quality issues cannot exceed 1000 characters")]
    public string? QualityIssues { get; set; } = string.Empty;
}

public class UpdateDailyReportRequest
{
    [StringLength(2000, ErrorMessage = "General notes cannot exceed 2000 characters")]
    public string? GeneralNotes { get; set; } = string.Empty;

    [RegularExpression(@"^(Sunny|PartlyCloudy|Cloudy|Rainy|Stormy|Foggy|Snow|Windy)$", 
        ErrorMessage = "Weather condition must be one of: Sunny, PartlyCloudy, Cloudy, Rainy, Stormy, Foggy, Snow, Windy")]
    public string? WeatherCondition { get; set; }

    [Range(-40, 50, ErrorMessage = "Temperature must be between -40 and 50 degrees")]
    public double? Temperature { get; set; }

    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100 percent")]
    public int? Humidity { get; set; }

    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double? WindSpeed { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Total work hours must be non-negative")]
    public int TotalWorkHours { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Personnel on site must be non-negative")]
    public int PersonnelOnSite { get; set; }

    [StringLength(1000, ErrorMessage = "Safety incidents cannot exceed 1000 characters")]
    public string? SafetyIncidents { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Quality issues cannot exceed 1000 characters")]
    public string? QualityIssues { get; set; } = string.Empty;
}

public class WorkProgressItemDto
{
    public Guid WorkProgressId { get; set; }
    public Guid ReportId { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double HoursWorked { get; set; }
    public int PercentComplete { get; set; }
    public string? Issues { get; set; } = string.Empty;
    public string? NextSteps { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateWorkProgressItemRequest
{
    [Required(ErrorMessage = "Report ID is required")]
    public Guid ReportId { get; set; }

    public Guid? TaskId { get; set; }

    [Required(ErrorMessage = "Activity is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Activity must be between 3 and 200 characters")]
    public string Activity { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 24, ErrorMessage = "Hours worked must be between 0 and 24")]
    public double HoursWorked { get; set; }

    [Range(0, 100, ErrorMessage = "Percent complete must be between 0 and 100")]
    public int PercentComplete { get; set; }

    [StringLength(500, ErrorMessage = "Issues cannot exceed 500 characters")]
    public string? Issues { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Next steps cannot exceed 500 characters")]
    public string? NextSteps { get; set; } = string.Empty;
}

public class PersonnelLogDto
{
    public Guid PersonnelLogId { get; set; }
    public Guid ReportId { get; set; }
    public UserDto User { get; set; } = null!;
    public double HoursWorked { get; set; }
    public string? Role { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MaterialUsageDto
{
    public Guid MaterialUsageId { get; set; }
    public Guid ReportId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public double QuantityUsed { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class EquipmentLogDto
{
    public Guid EquipmentLogId { get; set; }
    public Guid ReportId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public double HoursUsed { get; set; }
    public string? Purpose { get; set; } = string.Empty;
    public string? Issues { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Work Requests DTOs
public class WorkRequestDto
{
    public Guid RequestId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public UserDto RequestedBy { get; set; } = null!;
    public UserDto? AssignedTo { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Resolution { get; set; } = string.Empty;
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public string? Location { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Related data
    public List<WorkRequestTaskDto> Tasks { get; set; } = new();
    public List<WorkRequestCommentDto> Comments { get; set; } = new();
    public List<ImageMetadataDto> Images { get; set; } = new();
}

public class CreateWorkRequestRequest
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Maintenance|Repair|Installation|Inspection|Documentation|Other)$", 
        ErrorMessage = "Type must be one of: Maintenance, Repair, Installation, Inspection, Documentation, Other")]
    public string Type { get; set; } = "Other";

    [RegularExpression(@"^(Low|Medium|High|Critical)$", 
        ErrorMessage = "Priority must be one of: Low, Medium, High, Critical")]
    public string Priority { get; set; } = "Medium";

    public Guid? AssignedToUserId { get; set; }

    public DateTime? RequiredByDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated cost must be non-negative")]
    public decimal? EstimatedCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
    public string? Location { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class UpdateWorkRequestRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Maintenance|Repair|Installation|Inspection|Documentation|Other)$", 
        ErrorMessage = "Type must be one of: Maintenance, Repair, Installation, Inspection, Documentation, Other")]
    public string Type { get; set; } = "Other";

    [RegularExpression(@"^(Low|Medium|High|Critical)$", 
        ErrorMessage = "Priority must be one of: Low, Medium, High, Critical")]
    public string Priority { get; set; } = "Medium";

    [RegularExpression(@"^(Pending|InProgress|Completed|Cancelled|OnHold)$", 
        ErrorMessage = "Status must be one of: Pending, InProgress, Completed, Cancelled, OnHold")]
    public string Status { get; set; } = "Pending";

    public Guid? AssignedToUserId { get; set; }

    public DateTime? RequiredByDate { get; set; }

    [StringLength(1000, ErrorMessage = "Resolution cannot exceed 1000 characters")]
    public string? Resolution { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Estimated cost must be non-negative")]
    public decimal? EstimatedCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual cost must be non-negative")]
    public decimal? ActualCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
    public double? ActualHours { get; set; }

    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
    public string? Location { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class WorkRequestTaskDto
{
    public Guid TaskId { get; set; }
    public Guid RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public UserDto? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWorkRequestTaskRequest
{
    [Required(ErrorMessage = "Request ID is required")]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class UpdateWorkRequestTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Pending|InProgress|Completed|Cancelled|OnHold)$", 
        ErrorMessage = "Status must be one of: Pending, InProgress, Completed, Cancelled, OnHold")]
    public string Status { get; set; } = "Pending";

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
    public double? ActualHours { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class WorkRequestCommentDto
{
    public Guid CommentId { get; set; }
    public Guid RequestId { get; set; }
    public UserDto User { get; set; } = null!;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateWorkRequestCommentRequest
{
    [Required(ErrorMessage = "Request ID is required")]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Comment is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 2000 characters")]
    public string Comment { get; set; } = string.Empty;
}
