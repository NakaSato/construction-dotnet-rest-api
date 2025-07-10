namespace dotnet_rest_api.DTOs;

/// <summary>
/// DTO for project real-time status information
/// </summary>
public class ProjectStatusDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal OverallCompletionPercentage { get; set; }
    public bool IsOnSchedule { get; set; }
    public bool IsOnBudget { get; set; }
    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<LinkDto>? Links { get; set; }
}

/// <summary>
/// Mobile-optimized project DTO with minimal payload
/// </summary>
public class ProjectMobileDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public DateTime? LastModified { get; set; }
    public decimal? ProgressPercentage { get; set; }
    public int? TaskCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Bulk status update request for admin operations
/// </summary>
public class BulkStatusUpdateRequest
{
    public List<Guid> ProjectIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// Result of bulk update operations
/// </summary>
public class BulkUpdateResult
{
    public int TotalRequested { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Guid> SuccessfulIds { get; set; } = new();
    public List<Guid> FailedIds { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Enhanced location DTO with additional mobile features
/// </summary>
public class LocationDto
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public double? Accuracy { get; set; } // GPS accuracy in meters
}
