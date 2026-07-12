using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Interface for project business logic
/// </summary>
public interface IProjectService
{
    Task<Result<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteProjectAsync(Guid id, string userName, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    // Milestone Management
    Task<Result<PerformanceMilestoneDto>> AddMilestoneAsync(Guid projectId, CreateProjectMilestoneRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<PerformanceMilestoneDto>>> GetProjectMilestonesAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<PerformanceMilestoneDto>> UpdateMilestoneAsync(Guid projectId, Guid milestoneId, UpdateProjectMilestoneRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteMilestoneAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken = default);
}
