using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services;

/// <summary>
/// Interface for project business logic
/// </summary>
public interface IProjectService
{
    Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters);
    Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber, int pageSize, Guid? managerId);
    Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id);
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request);
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request);
    Task<ServiceResult<bool>> DeleteProjectAsync(Guid id);
    Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize);
}
