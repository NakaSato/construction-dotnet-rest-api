using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

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
    public DateTime? UpdatedAt { get; set; }
    public UserDto? ProjectManager { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    
    // Solar Project Specific Fields
    public string? Team { get; set; }
    public string? ConnectionType { get; set; }
    public string? ConnectionNotes { get; set; }
    public decimal? TotalCapacityKw { get; set; }
    public int? PvModuleCount { get; set; }
    
    // Equipment Details
    public EquipmentDetailsDto? EquipmentDetails { get; set; }
    
    // Business Values
    public int? FtsValue { get; set; }
    public int? RevenueValue { get; set; }
    public int? PqmValue { get; set; }
    
    // Location Coordinates
    public LocationCoordinatesDto? LocationCoordinates { get; set; }
    
    public DateTime CreatedAt { get; set; }
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

    // [Required(ErrorMessage = "Project manager ID is required")] // Temporarily disabled for local development
    public Guid? ProjectManagerId { get; set; }

    // Solar Project Specific Fields
    [StringLength(50, ErrorMessage = "Team name cannot exceed 50 characters")]
    public string? Team { get; set; }

    [StringLength(20, ErrorMessage = "Connection type cannot exceed 20 characters")]
    public string? ConnectionType { get; set; }

    [StringLength(1000, ErrorMessage = "Connection notes cannot exceed 1000 characters")]
    public string? ConnectionNotes { get; set; }

    [Range(0, 999999.99, ErrorMessage = "Total capacity must be between 0 and 999999.99 kW")]
    public decimal? TotalCapacityKw { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "PV module count must be a positive number")]
    public int? PvModuleCount { get; set; }

    // Equipment Details
    public EquipmentDetailsDto? EquipmentDetails { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "FTS value must be a positive number")]
    public int? FtsValue { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Revenue value must be a positive number")]
    public int? RevenueValue { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "PQM value must be a positive number")]
    public int? PqmValue { get; set; }

    // Location Coordinates
    public LocationCoordinatesDto? LocationCoordinates { get; set; }
}

public class EquipmentDetailsDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Inverter 125kW count must be a positive number")]
    public int Inverter125kw { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Inverter 80kW count must be a positive number")]
    public int Inverter80kw { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Inverter 60kW count must be a positive number")]
    public int Inverter60kw { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Inverter 40kW count must be a positive number")]
    public int Inverter40kw { get; set; } = 0;
}

public class LocationCoordinatesDto
{
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
    public decimal Latitude { get; set; }

    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
    public decimal Longitude { get; set; }
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

    // [Required(ErrorMessage = "Project manager ID is required")] // Temporarily disabled for local development
    public Guid? ProjectManagerId { get; set; }
}


// Project statistics summary
public class ProjectStatistics
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int PlanningProjects { get; set; }
    public int OnHoldProjects { get; set; }
    public int CancelledProjects { get; set; }
    public decimal TotalCapacityKw { get; set; }
    public int TotalPvModules { get; set; }
    public decimal TotalFtsValue { get; set; }
    public decimal TotalRevenueValue { get; set; }
    public decimal TotalPqmValue { get; set; }
    public int ProjectManagerCount { get; set; }
    public string GeographicCoverage { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

