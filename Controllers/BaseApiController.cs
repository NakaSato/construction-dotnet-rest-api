using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Controllers;

/// <summary>
/// Base API controller providing common functionality for all API controllers
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Parses filter parameters from query string
    /// </summary>
    /// <param name="filterString">Filter string from query parameters</param>
    /// <returns>List of parsed filter parameters</returns>
    protected List<FilterParameter> ParseFiltersFromQuery(string? filterString)
    {
        if (string.IsNullOrEmpty(filterString))
            return new List<FilterParameter>();
        
        var filters = new List<FilterParameter>();
        var filterPairs = filterString.Split('&');
        
        foreach (var pair in filterPairs)
        {
            var parts = pair.Split('=');
            if (parts.Length != 2) continue;
            
            var fieldAndOperator = parts[0].Split("__");
            var field = fieldAndOperator[0];
            var operatorName = fieldAndOperator.Length > 1 ? fieldAndOperator[1] : "eq";
            var value = Uri.UnescapeDataString(parts[1]);
            
            filters.Add(new FilterParameter
            {
                Field = field,
                Operator = operatorName,
                Value = value
            });
        }
        
        return filters;
    }

    /// <summary>
    /// Parses filter parameters from query string and applies them to query parameters
    /// </summary>
    /// <typeparam name="T">Type of query parameters that implements IFilterableQuery</typeparam>
    /// <param name="queryParameters">Query parameters to apply filters to</param>
    /// <param name="filterString">Filter string from query parameters</param>
    protected void ApplyFiltersFromQuery<T>(T queryParameters, string? filterString) 
        where T : BaseQueryParameters, IFilterableQuery
    {
        if (!string.IsNullOrEmpty(filterString))
        {
            var parsedFilters = ParseFiltersFromQuery(filterString);
            queryParameters.Filters.AddRange(parsedFilters);
        }
    }

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="maxPageSize">Maximum allowed page size (default: 100)</param>
    /// <returns>Validation result with error message if invalid</returns>
    protected ActionResult? ValidatePaginationParameters(int pageNumber, int pageSize, int maxPageSize = 100)
    {
        if (pageNumber < 1)
            return BadRequest("Page number must be greater than 0.");

        if (pageSize < 1)
            return BadRequest("Page size must be greater than 0.");

        if (pageSize > maxPageSize)
            return BadRequest($"Page size must not exceed {maxPageSize}.");

        return null;
    }

    /// <summary>
    /// Creates a standardized error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <returns>Error response</returns>
    protected ActionResult CreateErrorResponse(string message, int statusCode = 500)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        };

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <typeparam name="T">Type of response data</typeparam>
    /// <param name="data">Response data</param>
    /// <param name="message">Success message</param>
    /// <returns>Success response</returns>
    protected ActionResult<ApiResponse<T>> CreateSuccessResponse<T>(T data, string message = "Operation completed successfully")
    {
        var response = new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates a standardized not found response
    /// </summary>
    /// <param name="message">Not found message</param>
    /// <returns>Not found response</returns>
    protected ActionResult CreateNotFoundResponse(string message = "Resource not found")
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Data = null
        };

        return NotFound(response);
    }

    /// <summary>
    /// Gets the base URL for the current request
    /// </summary>
    /// <returns>Base URL string</returns>
    protected string GetBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}{Request.Path}";
    }

    /// <summary>
    /// Extracts query parameters from the current request
    /// </summary>
    /// <returns>Dictionary of query parameters</returns>
    protected Dictionary<string, string> GetQueryParameters()
    {
        var queryParams = new Dictionary<string, string>();
        
        foreach (var param in Request.Query)
        {
            if (!string.IsNullOrEmpty(param.Value))
            {
                queryParams[param.Key] = param.Value.ToString();
            }
        }
        
        return queryParams;
    }

    /// <summary>
    /// Logs controller action information for debugging
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="action">Action name</param>
    /// <param name="parameters">Action parameters (optional)</param>
    protected void LogControllerAction(ILogger logger, string action, object? parameters = null)
    {
        var controllerName = GetType().Name;
        var logMessage = $"Controller: {controllerName}, Action: {action}";
        
        if (parameters != null)
        {
            logMessage += $", Parameters: {System.Text.Json.JsonSerializer.Serialize(parameters)}";
        }
        
        logger.LogInformation(logMessage);
    }

    /// <summary>
    /// Handles exceptions and returns appropriate error responses
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="ex">Exception to handle</param>
    /// <param name="action">Action name where exception occurred</param>
    /// <returns>Error response</returns>
    protected ActionResult HandleException(ILogger logger, Exception ex, string action)
    {
        var controllerName = GetType().Name;
        logger.LogError(ex, "Error in {Controller}.{Action}: {Message}", controllerName, action, ex.Message);
        
        return CreateErrorResponse($"An error occurred while {action.ToLower()}.");
    }

    /// <summary>
    /// Handles exceptions and returns appropriate typed error responses for ApiResponse methods
    /// </summary>
    /// <typeparam name="T">The type parameter for the ApiResponse</typeparam>
    /// <param name="logger">Logger instance</param>
    /// <param name="ex">Exception to handle</param>
    /// <param name="action">Action name where exception occurred</param>
    /// <returns>Typed error response</returns>
    protected ActionResult<ApiResponse<T>> HandleException<T>(ILogger logger, Exception ex, string action)
    {
        var controllerName = GetType().Name;
        logger.LogError(ex, "Error in {Controller}.{Action}: {Message}", controllerName, action, ex.Message);
        
        return CreateErrorResponse($"An error occurred while {action.ToLower()}.");
    }

    /// <summary>
    /// Converts a Result<T> from the service layer to an ApiResponse<T> and appropriate HTTP status code
    /// </summary>
    /// <typeparam name="T">Type of data in the result</typeparam>
    /// <param name="result">Result from service layer</param>
    /// <returns>ActionResult with appropriate HTTP status code and ApiResponse<T></returns>
    protected ActionResult<ApiResponse<T>> ToApiResponse<T>(Result<T> result)
    {
        var apiResponse = new ApiResponse<T>
        {
            Success = result.IsSuccess,
            Message = result.Message,
            Data = result.Data,
            Errors = result.Errors
        };

        // Map to appropriate HTTP status codes based on error type
        if (result.IsSuccess)
        {
            return Ok(apiResponse);
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(apiResponse),
            ResultErrorType.Unauthorized => Unauthorized(apiResponse),
            ResultErrorType.Forbidden => StatusCode(403, apiResponse),
            ResultErrorType.Validation => BadRequest(apiResponse),
            ResultErrorType.BusinessLogic => BadRequest(apiResponse),
            ResultErrorType.RateLimit => StatusCode(429, apiResponse),
            ResultErrorType.ServerError => StatusCode(500, apiResponse),
            _ => StatusCode(500, apiResponse)
        };
    }

    /// <summary>
    /// Converts a non-generic Result from the service layer to an ApiResponse<bool> and appropriate HTTP status code
    /// </summary>
    /// <param name="result">Result from service layer</param>
    /// <returns>ActionResult with appropriate HTTP status code and ApiResponse<bool></returns>
    protected ActionResult<ApiResponse<bool>> ToApiResponse(Result result)
    {
        var apiResponse = new ApiResponse<bool>
        {
            Success = result.IsSuccess,
            Message = result.Message,
            Data = result.IsSuccess,
            Errors = result.Errors
        };

        // Map to appropriate HTTP status codes based on error type
        if (result.IsSuccess)
        {
            return Ok(apiResponse);
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(apiResponse),
            ResultErrorType.Unauthorized => Unauthorized(apiResponse),
            ResultErrorType.Forbidden => StatusCode(403, apiResponse),
            ResultErrorType.Validation => BadRequest(apiResponse),
            ResultErrorType.BusinessLogic => BadRequest(apiResponse),
            ResultErrorType.RateLimit => StatusCode(429, apiResponse),
            ResultErrorType.ServerError => StatusCode(500, apiResponse),
            _ => StatusCode(500, apiResponse)
        };
    }

    /// <summary>
    /// Enhanced ToApiResponse with validation error mapping for paginated results
    /// </summary>
    /// <typeparam name="T">Type of data in the result</typeparam>
    /// <param name="result">Result from service layer</param>
    /// <returns>ActionResult with appropriate HTTP status code and ApiResponse<T></returns>
    protected ActionResult<ApiResponse<T>> ToApiResponseWithValidation<T>(Result<T> result)
    {
        var apiResponse = new ApiResponse<T>
        {
            Success = result.IsSuccess,
            Message = result.Message,
            Data = result.Data,
            Errors = result.Errors
        };

        // Add validation-specific error mapping if needed
        if (result.IsFailure && result.ErrorType == ResultErrorType.Validation && result.ValidationErrors.Any())
        {
            apiResponse.Error = ApiResponse<T>.ValidationErrorResponse(result.ValidationErrors).Error;
        }

        return ToApiResponse(result);
    }
}
