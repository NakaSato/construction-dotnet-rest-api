using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Interface for project business logic
/// </summary>
public interface IProjectService
{
    Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters);
    Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id);
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request);
    Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string userName);
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userName);
    Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string userName);
    Task<ServiceResult<bool>> DeleteProjectAsync(Guid id);
    Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, string userName);
    Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize);
}
