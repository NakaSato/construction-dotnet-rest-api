using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Interface for project analytics and statistics
/// </summary>
public interface IProjectAnalyticsService
{
    Task<Result<ProjectStatistics>> GetProjectAnalyticsAsync();
    Task<Result<List<Guid>>> GetAllProjectIdsAsync();
    Task<Result<int>> GetTotalProjectCountAsync();
    Task<Result<int>> GetProjectCountByStatusAsync(string status);
}
