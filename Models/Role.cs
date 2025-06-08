using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public class Role
{
    [Key]
    public int RoleId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
