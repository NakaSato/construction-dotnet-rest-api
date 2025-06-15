using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

// Weekly Work Request DTOs
public class WeeklyWorkRequestDto
{
    public Guid WeeklyRequestId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OverallGoals { get; set; } = string.Empty;
    public List<string> KeyTasks { get; set; } = new();
    public WeeklyResourceForecastDto ResourceForecast { get; set; } = new();
    public UserDto RequestedBy { get; set; } = null!;
    public UserDto? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWeeklyWorkRequestDto
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }
    
    [Required(ErrorMessage = "Week start date is required")]
    public DateTime WeekStartDate { get; set; }
    
    [Required(ErrorMessage = "Overall goals are required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Overall goals must be between 10 and 2000 characters")]
    public string OverallGoals { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "At least one key task is required")]
    [MinLength(1, ErrorMessage = "At least one key task is required")]
    public List<string> KeyTasks { get; set; } = new();
    
    [StringLength(1000, ErrorMessage = "Personnel forecast cannot exceed 1000 characters")]
    public string? PersonnelForecast { get; set; }
    
    [StringLength(1000, ErrorMessage = "Major equipment cannot exceed 1000 characters")]
    public string? MajorEquipment { get; set; }
    
    [StringLength(1000, ErrorMessage = "Critical materials cannot exceed 1000 characters")]
    public string? CriticalMaterials { get; set; }
    
    [Required(ErrorMessage = "Requested by user ID is required")]
    public Guid RequestedById { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public int EstimatedHours { get; set; } = 0;
    
    [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
    public string? Priority { get; set; }
    
    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
    public string? Type { get; set; }
    
    public WeeklyResourceForecastDto? ResourceForecast { get; set; }
}

public class UpdateWeeklyWorkRequestDto
{
    [Required(ErrorMessage = "Week start date is required")]
    public DateTime WeekStartDate { get; set; }
    
    [Required(ErrorMessage = "Overall goals are required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Overall goals must be between 10 and 2000 characters")]
    public string OverallGoals { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "At least one key task is required")]
    public List<string> KeyTasks { get; set; } = new();
    
    [StringLength(1000, ErrorMessage = "Personnel forecast cannot exceed 1000 characters")]
    public string? PersonnelForecast { get; set; }
    
    [StringLength(1000, ErrorMessage = "Major equipment cannot exceed 1000 characters")]
    public string? MajorEquipment { get; set; }
    
    [StringLength(1000, ErrorMessage = "Critical materials cannot exceed 1000 characters")]
    public string? CriticalMaterials { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public int? EstimatedHours { get; set; }
    
    [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters")]
    public string? Priority { get; set; }
    
    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
    public string? Type { get; set; }
    
    public WeeklyResourceForecastDto? ResourceForecast { get; set; }
}

public class WeeklyResourceForecastDto
{
    [StringLength(1000, ErrorMessage = "Personnel forecast cannot exceed 1000 characters")]
    public string? Personnel { get; set; }
    
    [StringLength(1000, ErrorMessage = "Major equipment forecast cannot exceed 1000 characters")]
    public string? MajorEquipment { get; set; }
    
    [StringLength(1000, ErrorMessage = "Critical materials forecast cannot exceed 1000 characters")]
    public string? CriticalMaterials { get; set; }
}

// Weekly Report DTOs
public class WeeklyReportDto
{
    public Guid WeeklyReportId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime WeekStartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SummaryOfProgress { get; set; } = string.Empty;
    public WeeklyAggregatedMetricsDto AggregatedMetrics { get; set; } = new();
    public List<string> MajorAccomplishments { get; set; } = new();
    public List<WeeklyIssueDto> MajorIssues { get; set; } = new();
    public string? Lookahead { get; set; }
    public UserDto SubmittedBy { get; set; } = null!;
    public UserDto? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWeeklyReportDto
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }
    
    [Required(ErrorMessage = "Week start date is required")]
    public DateTime WeekStartDate { get; set; }
    
    [StringLength(2000, ErrorMessage = "Summary of progress cannot exceed 2000 characters")]
    public string? SummaryOfProgress { get; set; }
    
    public WeeklyAggregatedMetricsDto? AggregatedMetrics { get; set; }
    
    public List<string>? MajorAccomplishments { get; set; }
    
    public List<WeeklyIssueDto>? MajorIssues { get; set; }
    
    [StringLength(2000, ErrorMessage = "Lookahead cannot exceed 2000 characters")]
    public string? Lookahead { get; set; }
    
    [Required(ErrorMessage = "Submitted by user ID is required")]
    public Guid SubmittedById { get; set; }
    
    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public int CompletionPercentage { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "Total man hours must be non-negative")]
    public int TotalManHours { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "Panels installed must be non-negative")]
    public int PanelsInstalled { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "Safety incidents must be non-negative")]
    public int SafetyIncidents { get; set; } = 0;
    
    [Range(0, int.MaxValue, ErrorMessage = "Delays reported must be non-negative")]
    public int DelaysReported { get; set; } = 0;
}

public class UpdateWeeklyReportDto
{
    [Required(ErrorMessage = "Week start date is required")]
    public DateTime WeekStartDate { get; set; }
    
    [Required(ErrorMessage = "Summary of progress is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Summary must be between 10 and 2000 characters")]
    public string SummaryOfProgress { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Aggregated metrics are required")]
    public WeeklyAggregatedMetricsDto AggregatedMetrics { get; set; } = new();
    
    public List<string>? MajorAccomplishments { get; set; }
    
    public List<WeeklyIssueDto>? MajorIssues { get; set; }
    
    [StringLength(2000, ErrorMessage = "Lookahead cannot exceed 2000 characters")]
    public string? Lookahead { get; set; }
    
    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public int? CompletionPercentage { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Total man hours must be non-negative")]
    public int? TotalManHours { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Panels installed must be non-negative")]
    public int? PanelsInstalled { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Safety incidents must be non-negative")]
    public int? SafetyIncidents { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Delays reported must be non-negative")]
    public int? DelaysReported { get; set; }
}

public class WeeklyAggregatedMetricsDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Total man hours must be non-negative")]
    public int TotalManHours { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Panels installed must be non-negative")]
    public int PanelsInstalled { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Safety incidents must be non-negative")]
    public int SafetyIncidents { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Delays reported must be non-negative")]
    public int DelaysReported { get; set; }
}

public class WeeklyIssueDto
{
    public Guid IssueId { get; set; }
    
    [Required(ErrorMessage = "Issue description is required")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 1000 characters")]
    public string Description { get; set; } = string.Empty;
    
    public string? Impact { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

// Query DTOs
public class WeeklyWorkRequestQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public DateTime? WeekStartAfter { get; set; }
    public DateTime? WeekStartBefore { get; set; }
}

public class WeeklyReportQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public DateTime? WeekStartAfter { get; set; }
    public DateTime? WeekStartBefore { get; set; }
}

// Advanced query parameters for Weekly Work Requests
public class WeeklyWorkRequestQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public string? Status { get; set; }
    public DateTime? WeekStartAfter { get; set; }
    public DateTime? WeekStartBefore { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? RequestedById { get; set; }
    public int? MinEstimatedHours { get; set; }
    public int? MaxEstimatedHours { get; set; }
    public string? Priority { get; set; }
    public string? Type { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Advanced query parameters for Weekly Reports
public class WeeklyReportQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public string? Status { get; set; }
    public DateTime? WeekStartAfter { get; set; }
    public DateTime? WeekStartBefore { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? SubmittedById { get; set; }
    public int? MinActualHours { get; set; }
    public int? MaxActualHours { get; set; }
    public int? MinCompletionPercentage { get; set; }
    public int? MaxCompletionPercentage { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}
