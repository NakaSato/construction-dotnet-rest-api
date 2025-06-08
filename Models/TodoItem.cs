namespace dotnet_rest_api.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Role Role { get; set; } = null!;
    public ICollection<Project> ManagedProjects { get; set; } = new List<Project>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<ImageMetadata> UploadedImages { get; set; } = new List<ImageMetadata>();
}

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class Project
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ClientInfo { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime StartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public Guid ProjectManagerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User ProjectManager { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}

public class TaskItem
{
    public Guid TaskId { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime? DueDate { get; set; }
    public Guid? AssignedTechnicianId { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Project Project { get; set; } = null!;
    public User? AssignedTechnician { get; set; }
    public ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}

public class ImageMetadata
{
    public Guid ImageId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string CloudStorageKey { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public DateTime UploadTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? CaptureTimestamp { get; set; }
    public double? GPSLatitude { get; set; }
    public double? GPSLongitude { get; set; }
    public string? DeviceModel { get; set; }
    public short? Orientation { get; set; }
    public string? EXIFData { get; set; } // JSON string for PostgreSQL JSONB

    // Navigation properties
    public Project Project { get; set; } = null!;
    public TaskItem? Task { get; set; }
    public User UploadedByUser { get; set; } = null!;
}

public enum ProjectStatus
{
    Planning = 1,
    InProgress = 2,
    Completed = 3,
    OnHold = 4
}

public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Blocked = 4
}

// Legacy Todo item for backwards compatibility
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime DueDate { get; set; }
}