using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Interface for project business logic
/// </summary>
public interface IProjectService
{
    Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string userName, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, string userName, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    // Milestone Management
    Task<ServiceResult<PerformanceMilestoneDto>> AddMilestoneAsync(Guid projectId, CreateProjectMilestoneRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<List<PerformanceMilestoneDto>>> GetProjectMilestonesAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PerformanceMilestoneDto>> UpdateMilestoneAsync(Guid projectId, Guid milestoneId, UpdateProjectMilestoneRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteMilestoneAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken = default);
}
