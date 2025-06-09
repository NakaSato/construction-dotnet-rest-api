using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Common;

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
    Task<ApiResponse<UserDto>> PatchUserAsync(Guid userId, PatchUserRequest request);
    Task<ApiResponse<bool>> DeleteUserAsync(Guid userId);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid userId, bool isActive);
}

public interface IProjectService
{
    Task<Result<ProjectDto>> GetProjectByIdAsync(Guid projectId);
    Task<Result<PagedResult<ProjectDto>>> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null);
    Task<Result<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters);
    Task<Result<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null);
    Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request);
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request);
    Task<Result<ProjectDto>> PatchProjectAsync(Guid projectId, PatchProjectRequest request);
    Task<Result<bool>> DeleteProjectAsync(Guid projectId);
    Task<Result<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
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
    Task<ApiResponse<TaskDto>> PatchTaskAsync(Guid taskId, PatchTaskRequest request);
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

public interface IDailyReportService
{
    Task<ApiResponse<DailyReportDto>> GetDailyReportByIdAsync(Guid reportId);
    Task<ApiResponse<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<ApiResponse<PagedResult<DailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid reporterId);
    Task<ApiResponse<DailyReportDto>> UpdateDailyReportAsync(Guid reportId, UpdateDailyReportRequest request);
    Task<ApiResponse<bool>> DeleteDailyReportAsync(Guid reportId);
    Task<ApiResponse<bool>> SubmitDailyReportAsync(Guid reportId);
    Task<ApiResponse<bool>> ApproveDailyReportAsync(Guid reportId);
    Task<ApiResponse<bool>> RejectDailyReportAsync(Guid reportId, string? rejectionReason);

    // Work Progress Items
    Task<ApiResponse<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request);
    Task<ApiResponse<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, CreateWorkProgressItemRequest request);
    Task<ApiResponse<bool>> DeleteWorkProgressItemAsync(Guid itemId);

    // Personnel Logs
    Task<ApiResponse<PersonnelLogDto>> AddPersonnelLogAsync(Guid reportId, PersonnelLogDto request);
    Task<ApiResponse<bool>> DeletePersonnelLogAsync(Guid logId);

    // Material Usage
    Task<ApiResponse<MaterialUsageDto>> AddMaterialUsageAsync(Guid reportId, MaterialUsageDto request);
    Task<ApiResponse<bool>> DeleteMaterialUsageAsync(Guid usageId);

    // Equipment Logs
    Task<ApiResponse<EquipmentLogDto>> AddEquipmentLogAsync(Guid reportId, EquipmentLogDto request);
    Task<ApiResponse<bool>> DeleteEquipmentLogAsync(Guid logId);
}

public interface IWorkRequestService
{
    Task<ApiResponse<WorkRequestDto>> GetWorkRequestByIdAsync(Guid requestId);
    Task<ApiResponse<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters);
    Task<ApiResponse<PagedResult<WorkRequestDto>>> GetProjectWorkRequestsAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<PagedResult<WorkRequestDto>>> GetUserWorkRequestsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<PagedResult<WorkRequestDto>>> GetAssignedWorkRequestsAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    Task<ApiResponse<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid requestedById);
    Task<ApiResponse<WorkRequestDto>> UpdateWorkRequestAsync(Guid requestId, UpdateWorkRequestRequest request);
    Task<ApiResponse<bool>> DeleteWorkRequestAsync(Guid requestId);
    Task<ApiResponse<bool>> AssignWorkRequestAsync(Guid requestId, Guid assignedToId);
    Task<ApiResponse<bool>> UpdateWorkRequestStatusAsync(Guid requestId, dotnet_rest_api.Models.WorkRequestStatus status);
    Task<ApiResponse<bool>> UpdateWorkRequestPriorityAsync(Guid requestId, dotnet_rest_api.Models.WorkRequestPriority priority);
    Task<ApiResponse<WorkRequestDto>> CompleteWorkRequestAsync(Guid requestId);

    // Work Request Tasks
    Task<ApiResponse<WorkRequestTaskDto>> AddWorkRequestTaskAsync(Guid requestId, CreateWorkRequestTaskRequest request);
    Task<ApiResponse<WorkRequestTaskDto>> UpdateWorkRequestTaskAsync(Guid taskId, UpdateWorkRequestTaskRequest request);
    Task<ApiResponse<bool>> DeleteWorkRequestTaskAsync(Guid taskId);
    Task<ApiResponse<bool>> UpdateWorkRequestTaskStatusAsync(Guid taskId, dotnet_rest_api.Models.WorkRequestStatus status);

    // Work Request Comments
    Task<ApiResponse<WorkRequestCommentDto>> AddWorkRequestCommentAsync(Guid requestId, CreateWorkRequestCommentRequest request, Guid authorId);
    Task<ApiResponse<bool>> DeleteWorkRequestCommentAsync(Guid commentId);
}

public interface ICloudStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<string> GetFileUrlAsync(string key, TimeSpan? expiration = null);
    Task<bool> DeleteFileAsync(string key);
}
