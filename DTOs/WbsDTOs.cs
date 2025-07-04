using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// DTO for creating a new WBS task
/// </summary>
public class CreateWbsTaskDto
{
    /// <summary>
    /// The unique identifier for the task in the hierarchical structure (e.g., '4.3.2.3')
    /// </summary>
    [Required]
    [StringLength(50)]
    public string WbsId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the parent task in the hierarchy
    /// </summary>
    [StringLength(50)]
    public string? ParentWbsId { get; set; }

    /// <summary>
    /// The name of the task in English
    /// </summary>
    [Required]
    [StringLength(200)]
    public string TaskNameEN { get; set; } = string.Empty;

    /// <summary>
    /// The name of the task in Thai
    /// </summary>
    [Required]
    [StringLength(200)]
    public string TaskNameTH { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the task's scope
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task
    /// </summary>
    public WbsTaskStatus Status { get; set; } = WbsTaskStatus.NotStarted;

    /// <summary>
    /// The weight of the task relative to its parent or the total project (percentage)
    /// </summary>
    [Range(0, 100)]
    public double WeightPercent { get; set; }

    /// <summary>
    /// The installation area related to this task
    /// </summary>
    [StringLength(100)]
    public string? InstallationArea { get; set; }

    /// <summary>
    /// The criteria used to determine if the task is complete
    /// </summary>
    [StringLength(500)]
    public string? AcceptanceCriteria { get; set; }

    /// <summary>
    /// The planned start date for the task
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// The planned end date for the task
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// Project ID this WBS task belongs to
    /// </summary>
    [Required]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// User ID of the person assigned to this task
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// List of prerequisite task IDs that must be completed before this task
    /// </summary>
    public List<string>? Dependencies { get; set; }
}

/// <summary>
/// DTO for updating an existing WBS task
/// </summary>
public class UpdateWbsTaskDto
{
    /// <summary>
    /// The name of the task in English
    /// </summary>
    [StringLength(200)]
    public string? TaskNameEN { get; set; }

    /// <summary>
    /// The name of the task in Thai
    /// </summary>
    [StringLength(200)]
    public string? TaskNameTH { get; set; }

    /// <summary>
    /// A detailed description of the task's scope
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task
    /// </summary>
    public WbsTaskStatus? Status { get; set; }

    /// <summary>
    /// The weight of the task relative to its parent or the total project (percentage)
    /// </summary>
    [Range(0, 100)]
    public double? WeightPercent { get; set; }

    /// <summary>
    /// The installation area related to this task
    /// </summary>
    [StringLength(100)]
    public string? InstallationArea { get; set; }

    /// <summary>
    /// The criteria used to determine if the task is complete
    /// </summary>
    [StringLength(500)]
    public string? AcceptanceCriteria { get; set; }

    /// <summary>
    /// The planned start date for the task
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// The actual start date of the task
    /// </summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>
    /// The planned end date for the task
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// The actual end date of the task
    /// </summary>
    public DateTime? ActualEndDate { get; set; }

    /// <summary>
    /// User ID of the person assigned to this task
    /// </summary>
    public Guid? AssignedUserId { get; set; }
}

/// <summary>
/// DTO for WBS task response data
/// </summary>
public class WbsTaskDto
{
    /// <summary>
    /// The unique identifier for the task in the hierarchical structure
    /// </summary>
    public string WbsId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the parent task in the hierarchy
    /// </summary>
    public string? ParentWbsId { get; set; }

    /// <summary>
    /// The name of the task in English
    /// </summary>
    public string TaskNameEN { get; set; } = string.Empty;

    /// <summary>
    /// The name of the task in Thai
    /// </summary>
    public string TaskNameTH { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the task's scope
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task
    /// </summary>
    public WbsTaskStatus Status { get; set; }

    /// <summary>
    /// The weight of the task relative to its parent or the total project (percentage)
    /// </summary>
    public double WeightPercent { get; set; }

    /// <summary>
    /// The installation area related to this task
    /// </summary>
    public string? InstallationArea { get; set; }

    /// <summary>
    /// The criteria used to determine if the task is complete
    /// </summary>
    public string? AcceptanceCriteria { get; set; }

    /// <summary>
    /// The planned start date for the task
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// The actual start date of the task
    /// </summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>
    /// The planned end date for the task
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// The actual end date of the task
    /// </summary>
    public DateTime? ActualEndDate { get; set; }

    /// <summary>
    /// Project ID this WBS task belongs to
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// User ID of the person assigned to this task
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// Name of the assigned user
    /// </summary>
    public string? AssignedUserName { get; set; }

    /// <summary>
    /// List of prerequisite task IDs
    /// </summary>
    public List<string> Dependencies { get; set; } = new List<string>();

    /// <summary>
    /// Number of evidence items attached to this task
    /// </summary>
    public int EvidenceCount { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for WBS task summary (minimal information)
/// </summary>
public class WbsTaskSummaryDto
{
    public string WbsId { get; set; } = string.Empty;
    public string TaskNameEN { get; set; } = string.Empty;
    public string TaskNameTH { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double WeightPercent { get; set; }
    public string? InstallationArea { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
}

/// <summary>
/// DTO for project progress based on WBS tasks
/// </summary>
public class WbsProjectProgressDto
{
    /// <summary>
    /// Project ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Total number of WBS tasks
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Number of completed tasks
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Overall progress percentage (weighted)
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Summary of tasks by status
    /// </summary>
    public Dictionary<string, int> StatusSummary { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Progress breakdown by installation area
    /// </summary>
    public List<WbsAreaProgressDto> AreaProgress { get; set; } = new List<WbsAreaProgressDto>();
}

/// <summary>
/// DTO for installation area progress
/// </summary>
public class WbsAreaProgressDto
{
    /// <summary>
    /// Installation area name
    /// </summary>
    public string InstallationArea { get; set; } = string.Empty;

    /// <summary>
    /// Total number of tasks in this area
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Number of completed tasks in this area
    /// </summary>
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Progress percentage for this area
    /// </summary>
    public double ProgressPercentage { get; set; }
}

/// <summary>
/// DTO for WBS task evidence
/// </summary>
public class WbsTaskEvidenceDto
{
    public Guid Id { get; set; }
    public string WbsTaskId { get; set; } = string.Empty;
    public string EvidenceType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
}

/// <summary>
/// DTO for creating WBS task evidence
/// </summary>
public class CreateWbsTaskEvidenceDto
{
    [Required]
    [StringLength(50)]
    public string EvidenceType { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }
}

/// <summary>
/// DTO for WBS task hierarchy (tree structure)
/// </summary>
public class WbsTaskHierarchyDto
{
    public string WbsId { get; set; } = string.Empty;
    public string? ParentWbsId { get; set; }
    public string TaskNameEN { get; set; } = string.Empty;
    public string TaskNameTH { get; set; } = string.Empty;
    public WbsTaskStatus Status { get; set; }
    public double WeightPercent { get; set; }
    public string? InstallationArea { get; set; }
    public List<WbsTaskHierarchyDto> Children { get; set; } = new List<WbsTaskHierarchyDto>();
    public int Level { get; set; }
}

/// <summary>
/// DTO for WBS task dependency
/// </summary>
public class WbsTaskDependencyDto
{
    public Guid Id { get; set; }
    public string DependentTaskId { get; set; } = string.Empty;
    public string PrerequisiteTaskId { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public string? DependentTaskName { get; set; }
    public string? PrerequisiteTaskName { get; set; }
}
