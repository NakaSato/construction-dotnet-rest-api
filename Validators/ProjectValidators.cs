using FluentValidation;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Validators;

/// <summary>
/// FluentValidation validator for CreateProjectRequest
/// </summary>
public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(5).WithMessage("Address must be at least 5 characters")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.ClientInfo)
            .MaximumLength(1000).WithMessage("Client info cannot exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThan(DateTime.MinValue).WithMessage("Start date must be a valid date");

        RuleFor(x => x.EstimatedEndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EstimatedEndDate.HasValue)
            .WithMessage("Estimated end date must be after start date");

        RuleFor(x => x.TotalCapacityKw)
            .InclusiveBetween(0, 999999.99m)
            .When(x => x.TotalCapacityKw.HasValue)
            .WithMessage("Total capacity must be between 0 and 999999.99 kW");

        RuleFor(x => x.PvModuleCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PvModuleCount.HasValue)
            .WithMessage("PV module count must be non-negative");

        // Location validation
        When(x => x.LocationCoordinates != null, () =>
        {
            RuleFor(x => x.LocationCoordinates!.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("Latitude must be between -90 and 90");

            RuleFor(x => x.LocationCoordinates!.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Longitude must be between -180 and 180");
        });
    }
}

/// <summary>
/// FluentValidation validator for UpdateProjectRequest
/// </summary>
public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    private static readonly string[] ValidStatuses = { "Planning", "InProgress", "Completed", "OnHold", "Cancelled" };

    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty().WithMessage("Project name is required")
            .MinimumLength(3).WithMessage("Project name must be at least 3 characters")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(5).WithMessage("Address must be at least 5 characters")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EstimatedEndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EstimatedEndDate.HasValue)
            .WithMessage("Estimated end date must be after start date");

        RuleFor(x => x.ActualEndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.ActualEndDate.HasValue)
            .WithMessage("Actual end date must be after start date");
    }
}

/// <summary>
/// FluentValidation validator for PatchProjectRequest
/// </summary>
public class PatchProjectRequestValidator : AbstractValidator<PatchProjectRequest>
{
    private static readonly string[] ValidStatuses = { "Planning", "InProgress", "Completed", "OnHold", "Cancelled" };

    public PatchProjectRequestValidator()
    {
        RuleFor(x => x.ProjectName)
            .MinimumLength(3).When(x => !string.IsNullOrEmpty(x.ProjectName))
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.ProjectName));

        RuleFor(x => x.Address)
            .MinimumLength(5).When(x => !string.IsNullOrEmpty(x.Address))
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.Status)
            .Must(status => ValidStatuses.Contains(status!))
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
