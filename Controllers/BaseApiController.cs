using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;

namespace dotnet_rest_api.Controllers;

/// <summary>
/// Base controller for all API controllers with enhanced functionality
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    private readonly ILogger<BaseApiController>? _logger;
    protected readonly IUserContextService? _userContextService;
    protected readonly IResponseBuilderService? _responseBuilderService;

    protected BaseApiController(
        ILogger<BaseApiController>? logger = null,
        IUserContextService? userContextService = null,
        IResponseBuilderService? responseBuilderService = null)
    {
        _logger = logger;
        _userContextService = userContextService;
        _responseBuilderService = responseBuilderService;
    }

    /// <summary>
    /// Gets the current user ID from the authentication context
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        if (_userContextService == null)
        {
            // Fallback to original method if service is not available
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return userIdString != null && Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        return _userContextService.GetCurrentUserId(User);
    }

    /// <summary>
    /// Gets the current user details from the authentication context
    /// </summary>
    protected UserContext? GetCurrentUserContext()
    {
        if (_userContextService == null)
        {
            return null;
        }

        return _userContextService.GetCurrentUserContext(User);
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreateSuccessResponse<T>(T data, string? message = null)
    {
        if (_responseBuilderService != null)
        {
            return Ok(_responseBuilderService.CreateSuccessResponse(data, message));
        }

        // Fallback to original method
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully"
        });
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreateErrorResponse<T>(string message, int statusCode = 400)
    {
        if (_responseBuilderService != null)
        {
            var response = _responseBuilderService.CreateErrorResponse<T>(message, statusCode);
            return StatusCode(statusCode, response);
        }

        // Fallback to original method
        return StatusCode(statusCode, new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default(T)
        });
    }

    /// <summary>
    /// Creates a validation error response from ModelState
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreateValidationErrorResponse<T>()
    {
        if (_responseBuilderService != null)
        {
            return BadRequest(_responseBuilderService.CreateValidationErrorResponse<T>(ModelState));
        }

        // Fallback to original method
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        return BadRequest(new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Data = default(T),
            Errors = errors
        });
    }

    /// <summary>
    /// Logs controller action
    /// </summary>
    /// <param name="action">The action being performed</param>
    /// <param name="parameters">Optional parameters</param>
    protected void LogControllerAction(string action, object? parameters = null)
    {
        _logger?.LogInformation("Controller Action: {Action}, Parameters: {Parameters}", action, parameters);
    }

    /// <summary>
    /// Logs controller action with custom logger
    /// </summary>
    /// <param name="logger">Custom logger</param>
    /// <param name="action">The action being performed</param>
    /// <param name="parameters">Optional parameters</param>
    protected void LogControllerAction(ILogger logger, string action, object? parameters = null)
    {
        logger.LogInformation("Controller Action: {Action}, Parameters: {Parameters}", action, parameters);
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="ex">The exception</param>
    /// <param name="action">The action that failed</param>
    /// <returns>Error response</returns>
    protected ActionResult<ApiResponse<T>> HandleException<T>(Exception ex, string action)
    {
        _logger?.LogError(ex, "Error in {Action}: {Message}", action, ex.Message);
        return StatusCode(500, new ApiResponse<T>
        {
            Success = false,
            Message = "An internal error occurred",
            Errors = new List<string> { ex.Message }
        });
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error response with custom logger
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="logger">Custom logger</param>
    /// <param name="ex">The exception</param>
    /// <param name="action">The action that failed</param>
    /// <returns>Error response</returns>
    protected ActionResult<ApiResponse<T>> HandleException<T>(ILogger logger, Exception ex, string action)
    {
        logger.LogError(ex, "Error in {Action}: {Message}", action, ex.Message);
        return StatusCode(500, new ApiResponse<T>
        {
            Success = false,
            Message = "An internal error occurred",
            Errors = new List<string> { ex.Message }
        });
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error response (non-generic)
    /// </summary>
    /// <param name="logger">Custom logger</param>
    /// <param name="ex">The exception</param>
    /// <param name="action">The action that failed</param>
    /// <returns>Error response</returns>
    protected IActionResult HandleException(ILogger logger, Exception ex, string action)
    {
        logger.LogError(ex, "Error in {Action}: {Message}", action, ex.Message);
        return StatusCode(500, new
        {
            Success = false,
            Message = "An internal error occurred",
            Data = (object?)null,
            Errors = new List<string> { ex.Message }
        });
    }

    /// <summary>
    /// Converts service result to API response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="serviceResult">Service result</param>
    /// <returns>API response</returns>
    protected ActionResult<ApiResponse<T>> ToApiResponse<T>(ServiceResult<T> serviceResult)
    {
        if (serviceResult.Success)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Data = serviceResult.Data,
                Message = serviceResult.Message ?? "Success"
            });
        }

        return BadRequest(new ApiResponse<T>
        {
            Success = false,
            Message = serviceResult.Message ?? "Operation failed",
            Errors = serviceResult.Errors ?? new List<string>()
        });
    }

    /// <summary>
    /// Creates a success response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="data">Data to return</param>
    /// <summary>
    /// Creates an error response (non-generic)
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Error response</returns>
    protected IActionResult CreateErrorResponse(string message, int statusCode = 400)
    {
        var response = new
        {
            Success = false,
            Message = message,
            Data = (object?)null,
            Errors = new List<string>()
        };

        return statusCode switch
        {
            404 => NotFound(response),
            401 => Unauthorized(response),
            403 => StatusCode(403, response),
            500 => StatusCode(500, response),
            _ => BadRequest(response)
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="message">Not found message</param>
    /// <returns>Not found response</returns>
    protected ActionResult<ApiResponse<T>> CreateNotFoundResponse<T>(string message = "Resource not found")
    {
        return NotFound(new ApiResponse<T>
        {
            Success = false,
            Message = message
        });
    }

    /// <summary>
    /// Creates a not found response (non-generic)
    /// </summary>
    /// <param name="message">Not found message</param>
    /// <returns>Not found response</returns>
    protected IActionResult CreateNotFoundResponse(string message = "Resource not found")
    {
        return NotFound(new
        {
            Success = false,
            Message = message,
            Data = (object?)null,
            Errors = new List<string>()
        });
    }

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Validation error message or null if valid</returns>
    protected string? ValidatePaginationParameters(int page, int pageSize)
    {
        if (page < 1)
            return "Page number must be greater than 0";
        
        if (pageSize < 1 || pageSize > 100)
            return "Page size must be between 1 and 100";
        
        return null;
    }

    /// <summary>
    /// Validates pagination parameters (generic version)
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Validation error message or null if valid</returns>
    protected string? ValidatePaginationParameters<T>(int page, int pageSize)
    {
        return ValidatePaginationParameters(page, pageSize);
    }

    /// <summary>
    /// Applies filters from query parameters (stub implementation)
    /// </summary>
    /// <param name="query">Query parameters</param>
    protected void ApplyFiltersFromQuery(object query)
    {
        // Stub implementation - to be implemented as needed
        _logger?.LogDebug("Applying filters from query: {Query}", query);
    }

    /// <summary>
    /// Applies filters from query parameters with filter string (stub implementation)
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <param name="filterString">Filter string</param>
    protected void ApplyFiltersFromQuery(object query, string? filterString)
    {
        // Stub implementation - to be implemented as needed
        _logger?.LogDebug("Applying filters from query: {Query}, FilterString: {FilterString}", query, filterString);
    }
}