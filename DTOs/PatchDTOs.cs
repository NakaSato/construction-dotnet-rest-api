using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// PATCH DTOs for partial updates
public class PatchProjectRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Project name must be between 3 and 200 characters")]
    public string? ProjectName { get; set; }

    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public string? Address { get; set; }

    [StringLength(1000, ErrorMessage = "Client info cannot exceed 1000 characters")]
    public string? ClientInfo { get; set; }

    [RegularExpression(@"^(Planning|InProgress|Completed|OnHold|Cancelled)$", ErrorMessage = "Status must be one of: Planning, InProgress, Completed, OnHold, Cancelled")]
    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public Guid? ProjectManagerId { get; set; }
}

public class PatchTaskRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Task title must be between 3 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(2000, ErrorMessage = "Task description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [RegularExpression(@"^(Pending|In Progress|Completed|Cancelled)$", ErrorMessage = "Status must be one of: Pending, In Progress, Completed, Cancelled")]
    public string? Status { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid? AssignedTechnicianId { get; set; }
}

public class PatchUserRequest
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string? Password { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string? FullName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive number")]
    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }
}


