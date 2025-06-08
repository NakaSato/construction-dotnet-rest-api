using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<string>> RefreshTokenAsync(string refreshToken);
    bool ValidateTokenAsync(string token);
}

public interface IUserService
{
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? role = null);
    Task<ApiResponse<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
    Task<ApiResponse<PagedResult<UserDto>>> GetUsersLegacyAsync(int pageNumber = 1, int pageSize = 10, string? role = null);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, CreateUserRequest request);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid userId);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId, bool isActive);
}

public interface IProjectService
{
    Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid projectId);
    Task<ApiResponse<PagedResult<ProjectDto>>> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null);
    Task<ApiResponse<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters);
    Task<ApiResponse<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null);
    Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectRequest request);
    Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request);
    Task<ApiResponse<bool>> DeleteProjectAsync(Guid projectId);
    Task<ApiResponse<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
}

public interface ITaskService
{
    Task<ApiResponse<TaskDto>> GetTaskByIdAsync(Guid taskId);
    Task<ApiResponse<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber = 1, int pageSize = 10, Guid? projectId = null, Guid? assignedTo = null);
    Task<ApiResponse<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters);
    Task<ApiResponse<PagedResult<TaskDto>>> GetTasksLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? projectId = null, Guid? assignedTo = null);
    Task<ApiResponse<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request);
    Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request);
    Task<ApiResponse<bool>> DeleteTaskAsync(Guid taskId);
    Task<ApiResponse<bool>> UpdateTaskStatusAsync(Guid taskId, dotnet_rest_api.Models.TaskStatus status);
}

public interface IImageService
{
    Task<ApiResponse<ImageMetadataDto>> UploadImageAsync(IFormFile file, ImageUploadRequest request, Guid uploadedByUserId);
    Task<ApiResponse<ImageMetadataDto>> GetImageMetadataAsync(Guid imageId);
    Task<ApiResponse<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters);
    Task<ApiResponse<PagedResult<ImageMetadataDto>>> GetTaskImagesAsync(Guid taskId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<string>> GetImageUrlAsync(Guid imageId);
    Task<ApiResponse<bool>> DeleteImageAsync(Guid imageId);
}

public interface ICloudStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<string> GetFileUrlAsync(string key, TimeSpan? expiration = null);
    Task<bool> DeleteFileAsync(string key);
}
