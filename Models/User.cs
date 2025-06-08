using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [ForeignKey("Role")]
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual ICollection<Project> ManagedProjects { get; set; } = new List<Project>();
    public virtual ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ImageMetadata> UploadedImages { get; set; } = new List<ImageMetadata>();
}
