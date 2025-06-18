using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Request DTO for updating user information
/// </summary>
public class UpdateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}
