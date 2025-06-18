using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Request DTO for updating a master plan
/// </summary>
public class UpdateMasterPlanRequest
{
    [StringLength(200, MinimumLength = 3)]
    public string? Name { get; set; }

    /// <summary>
    /// Title property for backward compatibility
    /// </summary>
    public string? Title 
    { 
        get => Name; 
        set => Name = value; 
    }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// StartDate property for backward compatibility
    /// </summary>
    public DateTime? StartDate 
    { 
        get => PlannedStartDate; 
        set => PlannedStartDate = value; 
    }

    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// EndDate property for backward compatibility
    /// </summary>
    public DateTime? EndDate 
    { 
        get => PlannedEndDate; 
        set => PlannedEndDate = value; 
    }

    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value")]
    public decimal? Budget { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }
}
