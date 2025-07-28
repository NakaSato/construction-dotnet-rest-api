using System.Net;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Shared;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Implementation of response builder service for creating standardized API responses
/// </summary>
public class ResponseBuilderService : IResponseBuilderService
{
    private readonly IQueryService _queryService;
    private readonly ILogger<ResponseBuilderService> _logger;

    public ResponseBuilderService(IQueryService queryService, ILogger<ResponseBuilderService> logger)
    {
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a success response with data
    /// </summary>
    public ApiResponse<T> CreateSuccessResponse<T>(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        };
    }

    /// <summary>
    /// Creates an error response with message
    /// </summary>
    public ApiResponse<T> CreateErrorResponse<T>(string message, int? statusCode = null)
    {
        _logger.LogWarning("Creating error response: {Message} (Status: {StatusCode})", message, statusCode);
        
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default(T)
        };
    }

    /// <summary>
    /// Creates a paginated response with HATEOAS links
    /// </summary>
    public ApiResponse<ApiResponseWithPagination<T>> CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string baseUrl,
        Dictionary<string, string>? queryParams = null,
        string? message = null)
    {
        try
        {
            var pagedResult = _queryService.CreateRichPaginatedResponse<T>(
                items.ToList(),
                totalCount,
                pageNumber,
                pageSize,
                baseUrl,
                queryParams ?? new Dictionary<string, string>(),
                message ?? "Data retrieved successfully"
            );

            return CreateSuccessResponse(pagedResult, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating paginated response");
            return CreateErrorResponse<ApiResponseWithPagination<T>>("Error creating paginated response");
        }
    }

    /// <summary>
    /// Creates a validation error response from ModelState
    /// </summary>
    public ApiResponse<T> CreateValidationErrorResponse<T>(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var errorMessage = errors.Count == 1 
            ? errors.First() 
            : $"Validation failed: {string.Join("; ", errors)}";

        _logger.LogWarning("Validation error: {ErrorMessage}", errorMessage);

        return new ApiResponse<T>
        {
            Success = false,
            Message = errorMessage,
            Data = default(T)
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    public ApiResponse<T> CreateNotFoundResponse<T>(string resourceName, string? identifier = null)
    {
        var message = identifier != null 
            ? $"{resourceName} with identifier '{identifier}' was not found"
            : $"{resourceName} was not found";

        _logger.LogInformation("Resource not found: {Message}", message);

        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default(T)
        };
    }

    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    public ApiResponse<T> CreateUnauthorizedResponse<T>(string? message = null)
    {
        var errorMessage = message ?? "Unauthorized access. Please ensure you are authenticated.";
        
        _logger.LogWarning("Unauthorized access attempt: {Message}", errorMessage);

        return new ApiResponse<T>
        {
            Success = false,
            Message = errorMessage,
            Data = default(T)
        };
    }

    /// <summary>
    /// Creates a forbidden response
    /// </summary>
    public ApiResponse<T> CreateForbiddenResponse<T>(string? message = null)
    {
        var errorMessage = message ?? "Access forbidden. You do not have permission to access this resource.";
        
        _logger.LogWarning("Forbidden access attempt: {Message}", errorMessage);

        return new ApiResponse<T>
        {
            Success = false,
            Message = errorMessage,
            Data = default(T)
        };
    }
}
