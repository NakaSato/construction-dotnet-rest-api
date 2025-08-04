using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Stub implementation of IProjectService for development purposes
/// </summary>
public class ProjectService : IProjectService
{
    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
    {
        await Task.CompletedTask;
        var result = new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>(),
            TotalCount = 0,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
        return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result, "Projects retrieved successfully");
    }

    public async Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        await Task.CompletedTask;
        return ServiceResult<ProjectDto>.ErrorResult("ProjectService not implemented yet");
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        await Task.CompletedTask;
        return ServiceResult<ProjectDto>.ErrorResult("ProjectService not implemented yet");
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        await Task.CompletedTask;
        return ServiceResult<ProjectDto>.ErrorResult("ProjectService not implemented yet");
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id)
    {
        await Task.CompletedTask;
        return ServiceResult<bool>.ErrorResult("ProjectService not implemented yet");
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize)
    {
        await Task.CompletedTask;
        var result = new PagedResult<ProjectDto>
        {
            Items = new List<ProjectDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result, "User projects retrieved successfully");
    }
}
