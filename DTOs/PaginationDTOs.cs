using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Error Response DTOs
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
    
    // Project-specific statistics (only populated for project queries)
    public ProjectStatistics? Statistics { get; set; }
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


