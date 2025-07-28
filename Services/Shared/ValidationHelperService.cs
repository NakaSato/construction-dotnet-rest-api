using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Implementation of validation helper service for centralized validation logic
/// </summary>
public class ValidationHelperService : IValidationHelperService
{
    private readonly ILogger<ValidationHelperService> _logger;

    public ValidationHelperService(ILogger<ValidationHelperService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates model state and returns formatted error messages
    /// </summary>
    public ValidationResult ValidateModelState(ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
        {
            return ValidationResult.Success();
        }

        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        _logger.LogWarning("Model state validation failed with {ErrorCount} errors: {Errors}", 
            errors.Count, string.Join("; ", errors));

        return ValidationResult.Failure(errors);
    }

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    public ValidationResult ValidatePagination(int pageNumber, int pageSize, int maxPageSize = 100)
    {
        var errors = new List<string>();

        if (pageNumber < 1)
        {
            errors.Add("Page number must be greater than 0");
        }

        if (pageSize < 1)
        {
            errors.Add("Page size must be greater than 0");
        }
        else if (pageSize > maxPageSize)
        {
            errors.Add($"Page size cannot exceed {maxPageSize}");
        }

        if (errors.Any())
        {
            _logger.LogWarning("Pagination validation failed: PageNumber={PageNumber}, PageSize={PageSize}, Errors={Errors}", 
                pageNumber, pageSize, string.Join("; ", errors));
            return ValidationResult.Failure(errors);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates GUID parameter
    /// </summary>
    public ValidationResult ValidateGuid(Guid? id, string parameterName = "id")
    {
        if (id == null || id == Guid.Empty)
        {
            var error = $"Parameter '{parameterName}' must be a valid GUID";
            _logger.LogWarning("GUID validation failed: {Parameter}={Value}", parameterName, id);
            return ValidationResult.Failure(error);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates required string parameter
    /// </summary>
    public ValidationResult ValidateRequiredString(string? value, string parameterName, int minLength = 1, int maxLength = 255)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Parameter '{parameterName}' is required");
        }
        else
        {
            if (value.Length < minLength)
            {
                errors.Add($"Parameter '{parameterName}' must be at least {minLength} characters long");
            }

            if (value.Length > maxLength)
            {
                errors.Add($"Parameter '{parameterName}' cannot exceed {maxLength} characters");
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("String validation failed: {Parameter}={Value}, Errors={Errors}", 
                parameterName, value, string.Join("; ", errors));
            return ValidationResult.Failure(errors);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates date range
    /// </summary>
    public ValidationResult ValidateDateRange(DateTime? startDate, DateTime? endDate, string startParamName = "startDate", string endParamName = "endDate")
    {
        var errors = new List<string>();

        if (startDate == null)
        {
            errors.Add($"Parameter '{startParamName}' is required");
        }

        if (endDate != null && startDate != null && endDate <= startDate)
        {
            errors.Add($"Parameter '{endParamName}' must be after '{startParamName}'");
        }

        if (startDate != null && startDate < DateTime.Today.AddYears(-10))
        {
            errors.Add($"Parameter '{startParamName}' cannot be more than 10 years in the past");
        }

        if (endDate != null && endDate > DateTime.Today.AddYears(10))
        {
            errors.Add($"Parameter '{endParamName}' cannot be more than 10 years in the future");
        }

        if (errors.Any())
        {
            _logger.LogWarning("Date range validation failed: {StartParam}={StartDate}, {EndParam}={EndDate}, Errors={Errors}", 
                startParamName, startDate, endParamName, endDate, string.Join("; ", errors));
            return ValidationResult.Failure(errors);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates business rules for project creation
    /// </summary>
    public ValidationResult ValidateProjectBusinessRules(string projectName, DateTime startDate, DateTime? endDate, decimal? budget)
    {
        var errors = new List<string>();

        // Validate project name uniqueness (simplified - in real app would check database)
        if (string.IsNullOrWhiteSpace(projectName))
        {
            errors.Add("Project name is required");
        }
        else if (projectName.Length < 3)
        {
            errors.Add("Project name must be at least 3 characters long");
        }

        // Validate date logic
        if (startDate < DateTime.Today)
        {
            errors.Add("Project start date cannot be in the past");
        }

        if (endDate != null && endDate <= startDate)
        {
            errors.Add("Project end date must be after start date");
        }

        // Validate budget
        if (budget != null && budget <= 0)
        {
            errors.Add("Project budget must be greater than zero");
        }

        if (errors.Any())
        {
            _logger.LogWarning("Project business rules validation failed: ProjectName={ProjectName}, Errors={Errors}", 
                projectName, string.Join("; ", errors));
            return ValidationResult.Failure(errors);
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Creates a success validation result
    /// </summary>
    public ValidationResult CreateSuccess()
    {
        return ValidationResult.Success();
    }

    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    public ValidationResult CreateFailure(string errorMessage)
    {
        return ValidationResult.Failure(errorMessage);
    }

    /// <summary>
    /// Creates a failed validation result with multiple errors
    /// </summary>
    public ValidationResult CreateFailure(IEnumerable<string> errorMessages)
    {
        return ValidationResult.Failure(errorMessages);
    }
}
