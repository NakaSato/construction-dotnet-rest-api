using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Task DTOs
public class TaskDto
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public UserDto? AssignedTechnician { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}

public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression(@"^(Pending|In Progress|Completed|Cancelled)$", ErrorMessage = "Status must be one of: Pending, In Progress, Completed, Cancelled")]
    public string Status { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}


