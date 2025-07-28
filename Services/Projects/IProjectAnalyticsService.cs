using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Interface for project analytics and statistics
/// </summary>
public interface IProjectAnalyticsService
{
    Task<ServiceResult<ProjectStatistics>> GetProjectAnalyticsAsync();
    Task<ServiceResult<List<Guid>>> GetAllProjectIdsAsync();
    Task<ServiceResult<int>> GetTotalProjectCountAsync();
    Task<ServiceResult<int>> GetProjectCountByStatusAsync(string status);
}
