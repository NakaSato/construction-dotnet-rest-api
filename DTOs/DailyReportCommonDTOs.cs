using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Daily Reports DTOs
public class DailyReportDto
{
    public Guid DailyReportId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public UserDto? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; } = string.Empty;
    public string? GeneralNotes { get; set; } = string.Empty;
    
    // Weather Information
    public string? WeatherCondition { get; set; }
    public double? TemperatureHigh { get; set; }
    public double? TemperatureLow { get; set; }
    public double? Temperature { get; set; }
    public int? Humidity { get; set; }
    public double? WindSpeed { get; set; }
    
    // Summary Information
    public string? Summary { get; set; } = string.Empty;
    public string? WorkAccomplished { get; set; } = string.Empty;
    public string? WorkPlanned { get; set; } = string.Empty;
    public string? Issues { get; set; } = string.Empty;
    
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
    public int ImageCount { get; set; }
    
    // HATEOAS Links
    public List<LinkDto> Links { get; set; } = new();
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

    [Range(-40, 50, ErrorMessage = "Temperature high must be between -40 and 50 degrees")]
    public double? TemperatureHigh { get; set; }

    [Range(-40, 50, ErrorMessage = "Temperature low must be between -40 and 50 degrees")]
    public double? TemperatureLow { get; set; }

    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100 percent")]
    public int? Humidity { get; set; }

    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double? WindSpeed { get; set; }

    [StringLength(2000, ErrorMessage = "Summary cannot exceed 2000 characters")]
    public string? Summary { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Work accomplished cannot exceed 2000 characters")]
    public string? WorkAccomplished { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Work planned cannot exceed 2000 characters")]
    public string? WorkPlanned { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Issues cannot exceed 2000 characters")]
    public string? Issues { get; set; } = string.Empty;

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

    [Range(-40, 50, ErrorMessage = "Temperature high must be between -40 and 50 degrees")]
    public double? TemperatureHigh { get; set; }

    [Range(-40, 50, ErrorMessage = "Temperature low must be between -40 and 50 degrees")]
    public double? TemperatureLow { get; set; }

    [Range(0, 100, ErrorMessage = "Humidity must be between 0 and 100 percent")]
    public int? Humidity { get; set; }

    [Range(0, 200, ErrorMessage = "Wind speed must be between 0 and 200 km/h")]
    public double? WindSpeed { get; set; }

    [StringLength(2000, ErrorMessage = "Summary cannot exceed 2000 characters")]
    public string? Summary { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Work accomplished cannot exceed 2000 characters")]
    public string? WorkAccomplished { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Work planned cannot exceed 2000 characters")]
    public string? WorkPlanned { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Issues cannot exceed 2000 characters")]
    public string? Issues { get; set; } = string.Empty;

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
    public Guid WorkProgressItemId { get; set; }
    public Guid ReportId { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskTitle { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double HoursWorked { get; set; }
    public int PercentageComplete { get; set; }
    public int WorkersAssigned { get; set; }
    public string? Notes { get; set; } = string.Empty;
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
    public int PercentageComplete { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Workers assigned must be non-negative")]
    public int WorkersAssigned { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Issues cannot exceed 500 characters")]
    public string? Issues { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Next steps cannot exceed 500 characters")]
    public string? NextSteps { get; set; } = string.Empty;
}

public class UpdateWorkProgressItemRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Activity must be between 3 and 200 characters")]
    public string? Activity { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(0, 24, ErrorMessage = "Hours worked must be between 0 and 24")]
    public double? HoursWorked { get; set; }

    [Range(0, 100, ErrorMessage = "Percent complete must be between 0 and 100")]
    public int? PercentageComplete { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Workers assigned must be non-negative")]
    public int? WorkersAssigned { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    [StringLength(500, ErrorMessage = "Issues cannot exceed 500 characters")]
    public string? Issues { get; set; }

    [StringLength(500, ErrorMessage = "Next steps cannot exceed 500 characters")]
    public string? NextSteps { get; set; }
}

public class PersonnelLogDto
{
    public Guid PersonnelLogId { get; set; }
    public Guid ReportId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public double HoursWorked { get; set; }
    public string? Position { get; set; } = string.Empty;
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
    public decimal? Cost { get; set; }
    public string? Supplier { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class EquipmentLogDto
{
    public Guid EquipmentLogId { get; set; }
    public Guid ReportId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public double HoursUsed { get; set; }
    public string? OperatorName { get; set; } = string.Empty;
    public bool MaintenanceRequired { get; set; }
    public string? MaintenanceNotes { get; set; } = string.Empty;
    public string? Purpose { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public string? Issues { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


