namespace dotnet_rest_api.DTOs;

/// <summary>
/// Extension methods for ProjectDto to support Flutter app
/// </summary>
public static class ProjectDtoExtensions
{
    /// <summary>
    /// Get the project ID from a ProjectDto
    /// </summary>
    public static Guid Id(this ProjectDto dto) => dto.ProjectId;

    /// <summary>
    /// Get the project location from a ProjectDto
    /// </summary>
    public static string Location(this ProjectDto dto) => dto.Address;

    /// <summary>
    /// Get the client name from a ProjectDto
    /// </summary>
    public static string ClientName(this ProjectDto dto) => dto.ClientInfo;

    /// <summary>
    /// Get project description
    /// </summary>
    public static string Description(this ProjectDto dto)
    {
        return !string.IsNullOrEmpty(dto.ConnectionNotes) ? dto.ConnectionNotes : 
            "Solar project with " + dto.TotalCapacityKw.GetValueOrDefault(0) + " kW capacity";
    }

    /// <summary>
    /// Get project budget
    /// </summary>
    public static decimal Budget(this ProjectDto dto) => dto.RevenueValue.GetValueOrDefault(0);

    /// <summary>
    /// Get project thumbnail URL (dummy implementation for now)
    /// </summary>
    public static string? ThumbnailUrl(this ProjectDto dto) => null;

    /// <summary>
    /// Get project image URLs (dummy implementation for now)
    /// </summary>
    public static List<string> ImageUrls(this ProjectDto dto) => new List<string>();
}
