using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Request DTO for updating phase progress
/// </summary>
public class UpdatePhaseProgressRequest
{
    [Required]
    public Guid PhaseId { get; set; }

    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public decimal CompletionPercentage { get; set; }

    [Required]
    public PhaseStatus Status { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [StringLength(2000)]
    public string? ActivitiesCompleted { get; set; }

    [StringLength(2000)]
    public string? Issues { get; set; }
}
