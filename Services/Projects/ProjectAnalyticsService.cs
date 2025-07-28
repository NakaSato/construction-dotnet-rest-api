using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Service for project analytics and statistics operations
/// Separated from main ProjectService for better organization
/// </summary>
public class ProjectAnalyticsService : IProjectAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProjectAnalyticsService> _logger;

    public ProjectAnalyticsService(ApplicationDbContext context, ILogger<ProjectAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<ProjectStatistics>> GetProjectAnalyticsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving comprehensive project analytics");

            var allProjectsQuery = _context.Projects.AsQueryable();
            
            var projectStats = new ProjectStatistics
            {
                TotalProjects = await allProjectsQuery.CountAsync(),
                ActiveProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.InProgress),
                CompletedProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Completed),
                PlanningProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Planning),
                OnHoldProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.OnHold),
                CancelledProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Cancelled),
                TotalCapacityKw = await allProjectsQuery.SumAsync(p => p.TotalCapacityKw ?? 0),
                TotalPvModules = await allProjectsQuery.SumAsync(p => p.PvModuleCount ?? 0),
                TotalFtsValue = await allProjectsQuery.SumAsync(p => p.FtsValue ?? 0),
                TotalRevenueValue = await allProjectsQuery.SumAsync(p => p.RevenueValue ?? 0),
                TotalPqmValue = await allProjectsQuery.SumAsync(p => p.PqmValue ?? 0),
                ProjectManagerCount = await allProjectsQuery.Select(p => p.ProjectManagerId).Distinct().CountAsync(),
                GeographicCoverage = "Multiple provinces and regions",
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Project analytics retrieved successfully: {TotalProjects} total projects", projectStats.TotalProjects);
            return ServiceResult<ProjectStatistics>.SuccessResult(projectStats, "Project analytics retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project analytics");
            return ServiceResult<ProjectStatistics>.ErrorResult($"Error retrieving project analytics: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<Guid>>> GetAllProjectIdsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all project IDs from database");

            var projectIds = await _context.Projects
                .Select(p => p.ProjectId)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} project IDs", projectIds.Count);
            return ServiceResult<List<Guid>>.SuccessResult(projectIds, $"Retrieved {projectIds.Count} project IDs successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project IDs");
            return ServiceResult<List<Guid>>.ErrorResult($"Error retrieving project IDs: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetTotalProjectCountAsync()
    {
        try
        {
            var count = await _context.Projects.CountAsync();
            return ServiceResult<int>.SuccessResult(count, $"Total project count: {count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total project count");
            return ServiceResult<int>.ErrorResult($"Error getting total project count: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetProjectCountByStatusAsync(string status)
    {
        try
        {
            if (!Enum.TryParse<ProjectStatus>(status, true, out var statusEnum))
            {
                return ServiceResult<int>.ErrorResult($"Invalid status: {status}");
            }

            var count = await _context.Projects.CountAsync(p => p.Status == statusEnum);
            return ServiceResult<int>.SuccessResult(count, $"Projects with status '{status}': {count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project count by status {Status}", status);
            return ServiceResult<int>.ErrorResult($"Error getting project count by status: {ex.Message}");
        }
    }
}
