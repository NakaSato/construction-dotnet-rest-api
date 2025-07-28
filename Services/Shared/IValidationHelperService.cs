using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Service for centralized validation logic and error handling
/// </summary>
public interface IValidationHelperService
{
    /// <summary>
    /// Validates model state and returns formatted error messages
    /// </summary>
    ValidationResult ValidateModelState(ModelStateDictionary modelState);
    
    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    ValidationResult ValidatePagination(int pageNumber, int pageSize, int maxPageSize = 100);
    
    /// <summary>
    /// Validates GUID parameter
    /// </summary>
    ValidationResult ValidateGuid(Guid? id, string parameterName = "id");
    
    /// <summary>
    /// Validates required string parameter
    /// </summary>
    ValidationResult ValidateRequiredString(string? value, string parameterName, int minLength = 1, int maxLength = 255);
    
    /// <summary>
    /// Validates date range
    /// </summary>
    ValidationResult ValidateDateRange(DateTime? startDate, DateTime? endDate, string startParamName = "startDate", string endParamName = "endDate");
    
    /// <summary>
    /// Validates business rules for project creation
    /// </summary>
    ValidationResult ValidateProjectBusinessRules(string projectName, DateTime startDate, DateTime? endDate, decimal? budget);
    
    /// <summary>
    /// Creates a success validation result
    /// </summary>
    ValidationResult CreateSuccess();
    
    /// <summary>
    /// Creates a failed validation result
    /// </summary>
    ValidationResult CreateFailure(string errorMessage);
    
    /// <summary>
    /// Creates a failed validation result with multiple errors
    /// </summary>
    ValidationResult CreateFailure(IEnumerable<string> errorMessages);
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? FirstError => Errors.FirstOrDefault();
    public string CombinedErrors => string.Join("; ", Errors);
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string error) => new() { IsValid = false, Errors = { error } };
    public static ValidationResult Failure(IEnumerable<string> errors) => new() { IsValid = false, Errors = errors.ToList() };
}
