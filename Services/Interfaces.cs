using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services;



public interface IUserService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string? role);
    Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid id);
    Task<ServiceResult<UserDto>> GetUserByUsernameAsync(string username);
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ServiceResult<UserDto>> PatchUserAsync(Guid id, PatchUserRequest request);
    Task<ServiceResult<bool>> ActivateUserAsync(Guid id, bool isActive);
    Task<ServiceResult<bool>> DeleteUserAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
}

public interface ITaskService
{
    Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId);
    Task<ServiceResult<TaskDto>> GetTaskByIdAsync(Guid id);
    Task<ServiceResult<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    Task<ServiceResult<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request);
    Task<ServiceResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    Task<ServiceResult<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request);
    Task<ServiceResult<bool>> UpdateTaskStatusAsync(Guid id, dotnet_rest_api.Models.TaskStatus status);
    Task<ServiceResult<bool>> DeleteTaskAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters);
    Task<ServiceResult<PagedResult<TaskDto>>> GetPhaseTasksAsync(Guid phaseId, int pageNumber, int pageSize);
    Task<ServiceResult<TaskDto>> CreatePhaseTaskAsync(Guid phaseId, CreateTaskRequest request);
    Task<ServiceResult<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize);
    Task<ServiceResult<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
}

public interface IImageService
{
    Task<ServiceResult<ImageMetadataDto>> UploadImageAsync(IFormFile file, ImageUploadRequest request, Guid userId);
    Task<ServiceResult<ImageMetadataDto>> GetImageMetadataAsync(Guid id);
    Task<ServiceResult<string>> GetImageUrlAsync(Guid id);
    Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetTaskImagesAsync(Guid taskId, int pageNumber, int pageSize);
    Task<ServiceResult<bool>> DeleteImageAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters);
}

public interface IWorkRequestService
{
    Task<ServiceResult<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters);
    Task<ServiceResult<WorkRequestDto>> GetWorkRequestByIdAsync(Guid id);
    Task<ServiceResult<PagedResult<WorkRequestDto>>> GetProjectWorkRequestsAsync(Guid projectId, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<WorkRequestDto>>> GetAssignedWorkRequestsAsync(Guid userId, int pageNumber, int pageSize);
    Task<ServiceResult<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid userId);
    Task<ServiceResult<WorkRequestDto>> UpdateWorkRequestAsync(Guid id, UpdateWorkRequestRequest request);
    Task<ServiceResult<bool>> DeleteWorkRequestAsync(Guid id);
    Task<ServiceResult<bool>> AssignWorkRequestAsync(Guid id, Guid userId);
    Task<ServiceResult<WorkRequestDto>> CompleteWorkRequestAsync(Guid id);
    Task<ServiceResult<WorkRequestTaskDto>> AddWorkRequestTaskAsync(Guid workRequestId, CreateWorkRequestTaskRequest request);
    Task<ServiceResult<WorkRequestTaskDto>> UpdateWorkRequestTaskAsync(Guid taskId, UpdateWorkRequestTaskRequest request);
    Task<ServiceResult<bool>> DeleteWorkRequestTaskAsync(Guid taskId);
    Task<ServiceResult<WorkRequestCommentDto>> AddWorkRequestCommentAsync(Guid workRequestId, CreateWorkRequestCommentRequest request, Guid userId);
    Task<ServiceResult<bool>> DeleteWorkRequestCommentAsync(Guid commentId);
}

public interface IWeeklyWorkRequestService
{
    Task<ServiceResult<WeeklyWorkRequestDto>> GetWeeklyWorkRequestByIdAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetWeeklyWorkRequestsAsync(WeeklyWorkRequestQueryParameters parameters);
    Task<ServiceResult<WeeklyWorkRequestDto>> CreateWeeklyWorkRequestAsync(CreateWeeklyWorkRequestDto request);
    Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestAsync(Guid id, UpdateWeeklyWorkRequestDto request);
    Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestStatusAsync(Guid id, dotnet_rest_api.Models.WeeklyRequestStatus status);
    Task<ServiceResult<bool>> DeleteWeeklyWorkRequestAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetProjectWeeklyWorkRequestsAsync(Guid projectId, WeeklyWorkRequestQueryParameters parameters);
}

public interface IWeeklyReportService
{
    Task<ServiceResult<WeeklyReportDto>> GetWeeklyReportByIdAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetWeeklyReportsAsync(WeeklyReportQueryParameters parameters);
    Task<ServiceResult<WeeklyReportDto>> CreateWeeklyReportAsync(CreateWeeklyReportDto request);
    Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportAsync(Guid id, UpdateWeeklyReportDto request);
    Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportStatusAsync(Guid id, dotnet_rest_api.Models.WeeklyReportStatus status);
    Task<ServiceResult<bool>> DeleteWeeklyReportAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetProjectWeeklyReportsAsync(Guid projectId, WeeklyReportQueryParameters parameters);
}

public interface IWorkRequestApprovalService
{
    Task<ServiceResult<bool>> SubmitForApprovalAsync(Guid workRequestId, SubmitForApprovalRequest request, Guid userId);
    Task<ServiceResult<bool>> ProcessApprovalAsync(Guid workRequestId, ApprovalRequest request, Guid userId);
    Task<ServiceResult<ApprovalWorkflowStatusDto>> GetApprovalStatusAsync(Guid workRequestId);
    Task<ServiceResult<PagedResult<WorkRequestApprovalDto>>> GetApprovalHistoryAsync(Guid workRequestId, int pageNumber, int pageSize);
    Task<ServiceResult<bool>> EscalateApprovalAsync(Guid workRequestId, Guid escalateToUserId, string reason, Guid userId);
    Task<ServiceResult<PagedResult<WorkRequestDto>>> GetPendingApprovalsAsync(Guid userId, int pageNumber, int pageSize);
    Task<ServiceResult<ApprovalStatisticsDto>> GetApprovalStatisticsAsync(Guid? userId);
    Task<ServiceResult<bool>> BulkApprovalAsync(BulkApprovalRequest request, Guid userId);
    Task<ServiceResult<bool>> SendApprovalRemindersAsync();
}

public interface INotificationService
{
    Task SendNotificationAsync(string message, Guid userId);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface ICalendarService
{
    Task<ServiceResult<PaginatedCalendarEventsDto>> GetEventsAsync(CalendarQueryDto query);
    Task<ServiceResult<CalendarEventResponseDto>> GetEventByIdAsync(Guid id);
    Task<ServiceResult<CalendarEventResponseDto>> CreateEventAsync(CreateCalendarEventDto request, Guid userId);
    Task<ServiceResult<CalendarEventResponseDto>> UpdateEventAsync(Guid id, UpdateCalendarEventDto request);
    Task<ServiceResult<bool>> DeleteEventAsync(Guid id);
    Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetProjectEventsAsync(Guid projectId, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetTaskEventsAsync(Guid taskId, int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetUserEventsAsync(Guid userId, int pageNumber, int pageSize);
    Task<ServiceResult<IEnumerable<CalendarEventSummaryDto>>> GetUpcomingEventsAsync(int days, Guid? userId);
    Task<ServiceResult<ConflictCheckResult>> CheckConflictsAsync(DateTime startDateTime, DateTime endDateTime, Guid userId, Guid? excludeEventId);
    Task<ServiceResult<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventsAsync();
    Task<ServiceResult<CalendarEventResponseDto>> CreateRecurringEventAsync(CreateCalendarEventDto request, Guid userId);
    Task<ServiceResult<bool>> UpdateRecurringEventAsync(Guid seriesId, UpdateCalendarEventDto request, bool updateAll);
    Task<ServiceResult<bool>> DeleteRecurringEventAsync(Guid seriesId, bool deleteAll);
}





public interface ICloudStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileName);
    Task<string> GetFileUrlAsync(string fileName);
}

public interface IDataSeeder
{
    Task SeedSampleDataAsync();
}

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken);
    bool ValidateTokenAsync(string token);
}

public interface IRateLimitService
{
    string GetClientIdentifier(HttpContext context);
    Task<RateLimitResult> CheckRateLimit(string clientId, string endpoint, string method);
}

public interface IRateLimitMonitoringService
{
    Task RecordRateLimitHit(string clientId, string rule, string endpoint, bool isAllowed);
    Task<RateLimitStatistics> GetStatistics(TimeSpan period);
    Task<List<ClientRateLimitInfo>> GetTopClients(int count);
    Task<List<RateLimitViolation>> GetRecentViolations(TimeSpan period);
    Task<Dictionary<string, RateLimitRule>> GetActiveRules();
    Task ClearClientLimits(string clientId);
    Task ClearAllLimits();
    Task UpdateRuleConfiguration(string ruleName, RateLimitRule rule);
}

public interface IRateLimitStorage
{
    Task<int> GetRequestCountAsync(string key);
    Task IncrementRequestCountAsync(string key, TimeSpan expiration);
    Task ResetRequestCountAsync(string key);
}

public interface IResourceService
{
    Task<ServiceResult<EnhancedPagedResult<ResourceDto>>> GetResourcesAsync(ResourceQueryParameters parameters);
    Task<ServiceResult<ResourceDto>> GetResourceByIdAsync(Guid id);
    Task<ServiceResult<ResourceDto>> CreateResourceAsync(CreateResourceRequest request);
    Task<ServiceResult<ResourceDto>> UpdateResourceAsync(Guid id, UpdateResourceRequest request);
    Task<ServiceResult<bool>> DeleteResourceAsync(Guid id);
}

public interface IDocumentService
{
    Task<ServiceResult<EnhancedPagedResult<DocumentDto>>> GetDocumentsAsync(DocumentQueryParameters parameters);
    Task<ServiceResult<DocumentDto>> GetDocumentByIdAsync(Guid id);
    Task<ServiceResult<DocumentDto>> CreateDocumentAsync(CreateDocumentRequest request);
    Task<ServiceResult<DocumentDto>> UpdateDocumentAsync(Guid id, UpdateDocumentRequest request);
    Task<ServiceResult<bool>> DeleteDocumentAsync(Guid id);
}

public interface IDailyReportService
{
    Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<ServiceResult<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    Task<ServiceResult<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId);
    Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request);
    Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string token);
    Task<ServiceResult<bool>> DeleteDailyReportAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, DailyReportQueryParameters parameters);
    Task<ServiceResult<DailyReportDto>> SubmitDailyReportAsync(Guid id);
    Task<ServiceResult<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid approverId);
    Task<ServiceResult<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid approverId, string reason);
    Task<ServiceResult<List<DailyReportAttachmentDto>>> GetDailyReportAttachmentsAsync(Guid reportId);
    Task<ServiceResult<DailyReportAttachmentDto>> AddDailyReportAttachmentAsync(Guid reportId, string fileName, string filePath, string fileType, long fileSize, Guid userId);
    Task<ServiceResult<DailyReportAttachmentDto>> AddAttachmentAsync(Guid reportId, IFormFile file, Guid userId);
    Task<ServiceResult<bool>> RemoveDailyReportAttachmentAsync(Guid attachmentId);
    Task<ServiceResult<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate);
    Task<ServiceResult<List<DailyReportDto>>> GetUserDailyReportsAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<ServiceResult<string>> ExportDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<ServiceResult<DailyReportDto>> GetDailyReportByUserAndDateAsync(Guid userId, DateTime date);
    Task<ServiceResult<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request);
    Task<ServiceResult<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request);
    Task<ServiceResult<bool>> DeleteWorkProgressItemAsync(Guid itemId);
}
