using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Document DTO for API responses
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DocumentCategory Category { get; set; }
    public DocumentStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid UploadedById { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Tags { get; set; }
    public int Version { get; set; }
}

/// <summary>
/// Create document request DTO
/// </summary>
public class CreateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public Guid ProjectId { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Update document request DTO
/// </summary>
public class UpdateDocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DocumentCategory? Category { get; set; }
    public DocumentStatus? Status { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Document query parameters
/// </summary>
public class DocumentQueryParameters : BaseQueryParameters
{
    public Guid? ProjectId { get; set; }
    public DocumentCategory? Category { get; set; }
    public DocumentStatus? Status { get; set; }
    public Guid? UploadedById { get; set; }
    public string? Tags { get; set; }
    public string? FileType { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}

/// <summary>
/// Resource DTO for API responses
/// </summary>
public class ResourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public ResourceStatus Status { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Create resource request DTO
/// </summary>
public class CreateResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
}

/// <summary>
/// Update resource request DTO
/// </summary>
public class UpdateResourceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ResourceType? Type { get; set; }
    public ResourceStatus? Status { get; set; }
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? UnitCost { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
}

/// <summary>
/// Resource query parameters
/// </summary>
public class ResourceQueryParameters : BaseQueryParameters
{
    public Guid? ProjectId { get; set; }
    public ResourceType? Type { get; set; }
    public ResourceStatus? Status { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
    public DateTime? OrderDateAfter { get; set; }
    public DateTime? OrderDateBefore { get; set; }
    public DateTime? DeliveryDateAfter { get; set; }
    public DateTime? DeliveryDateBefore { get; set; }
}

/// <summary>
/// Daily report attachment DTO
/// </summary>
public class DailyReportAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid DailyReportId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Weekly summary DTO
/// </summary>
public class WeeklySummaryDto
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int TotalReports { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal AverageProgress { get; set; }
    public List<string> TopIssues { get; set; } = new();
    public List<string> CompletedMilestones { get; set; } = new();
    public List<DailyReportDto> Reports { get; set; } = new();
}

/// <summary>
/// Document category enumeration
/// </summary>
public enum DocumentCategory
{
    Contract = 0,
    TechnicalDrawing = 1,
    Report = 2,
    Photo = 3,
    Manual = 4,
    Permit = 5,
    Specification = 6,
    Other = 7
}

/// <summary>
/// Document status enumeration
/// </summary>
public enum DocumentStatus
{
    Draft = 0,
    UnderReview = 1,
    Approved = 2,
    Rejected = 3,
    Archived = 4
}

/// <summary>
/// Resource status enumeration
/// </summary>
public enum ResourceStatus
{
    Planned = 0,
    Ordered = 1,
    InTransit = 2,
    Delivered = 3,
    InUse = 4,
    Completed = 5,
    Returned = 6,
    Damaged = 7
}
