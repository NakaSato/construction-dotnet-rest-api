using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for building standardized API responses with consistent formatting
/// </summary>
public interface IResponseBuilderService
{
    /// <summary>
    /// Creates a success response with data
    /// </summary>
    ApiResponse<T> CreateSuccessResponse<T>(T data, string? message = null);
    
    /// <summary>
    /// Creates an error response with message
    /// </summary>
    ApiResponse<T> CreateErrorResponse<T>(string message, int? statusCode = null);
    
    /// <summary>
    /// Creates a paginated response with HATEOAS links
    /// </summary>
    ApiResponse<ApiResponseWithPagination<T>> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string baseUrl,
        Dictionary<string, string>? queryParams = null,
        string? message = null);
    
    /// <summary>
    /// Creates a validation error response from ModelState
    /// </summary>
    ApiResponse<T> CreateValidationErrorResponse<T>(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState);
    
    /// <summary>
    /// Creates a not found response
    /// </summary>
    ApiResponse<T> CreateNotFoundResponse<T>(string resourceName, string? identifier = null);
    
    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    ApiResponse<T> CreateUnauthorizedResponse<T>(string? message = null);
    
    /// <summary>
    /// Creates a forbidden response
    /// </summary>
    ApiResponse<T> CreateForbiddenResponse<T>(string? message = null);
}
