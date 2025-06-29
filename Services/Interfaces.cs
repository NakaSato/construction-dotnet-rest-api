using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using TaskModel = dotnet_rest_api.Models.Task;

namespace dotnet_rest_api.Services;



public interface IUserService
{
    System.Threading.Tasks.Task<ServiceResult<PagedResult<UserDto>>> GetUsersAsync(int pageNumber, int pageSize, string? role);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> GetUserByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> GetUserByUsernameAsync(string username);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> PatchUserAsync(Guid id, PatchUserRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> ActivateUserAsync(Guid id, bool isActive);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteUserAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<UserDto>>> GetUsersAsync(UserQueryParameters parameters);
}

public interface ITaskService
{
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> GetTaskByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> UpdateTaskStatusAsync(Guid id, dotnet_rest_api.Models.TaskStatus status);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteTaskAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskDto>>> GetPhaseTasksAsync(Guid phaseId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> CreatePhaseTaskAsync(Guid phaseId, CreateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
}

public interface IImageService
{
    System.Threading.Tasks.Task<ServiceResult<ImageMetadataDto>> UploadImageAsync(IFormFile file, ImageUploadRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<ImageMetadataDto>> GetImageMetadataAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<string>> GetImageUrlAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<ImageMetadataDto>>> GetTaskImagesAsync(Guid taskId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteImageAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<ImageMetadataDto>>> GetProjectImagesAsync(Guid projectId, ImageQueryParameters parameters);
}

public interface IWorkRequestService
{
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestDto>> GetWorkRequestByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<WorkRequestDto>>> GetProjectWorkRequestsAsync(Guid projectId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<WorkRequestDto>>> GetAssignedWorkRequestsAsync(Guid userId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestDto>> UpdateWorkRequestAsync(Guid id, UpdateWorkRequestRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWorkRequestAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<bool>> AssignWorkRequestAsync(Guid id, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestDto>> CompleteWorkRequestAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestTaskDto>> AddWorkRequestTaskAsync(Guid workRequestId, CreateWorkRequestTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestTaskDto>> UpdateWorkRequestTaskAsync(Guid taskId, UpdateWorkRequestTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWorkRequestTaskAsync(Guid taskId);
    System.Threading.Tasks.Task<ServiceResult<WorkRequestCommentDto>> AddWorkRequestCommentAsync(Guid workRequestId, CreateWorkRequestCommentRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWorkRequestCommentAsync(Guid commentId);
}

public interface IWeeklyWorkRequestService
{
    System.Threading.Tasks.Task<ServiceResult<WeeklyWorkRequestDto>> GetWeeklyWorkRequestByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetWeeklyWorkRequestsAsync(WeeklyWorkRequestQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<WeeklyWorkRequestDto>> CreateWeeklyWorkRequestAsync(CreateWeeklyWorkRequestDto request);
    System.Threading.Tasks.Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestAsync(Guid id, UpdateWeeklyWorkRequestDto request);
    System.Threading.Tasks.Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestStatusAsync(Guid id, dotnet_rest_api.Models.WeeklyRequestStatus status);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWeeklyWorkRequestAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetProjectWeeklyWorkRequestsAsync(Guid projectId, WeeklyWorkRequestQueryParameters parameters);
}

public interface IWeeklyReportService
{
    System.Threading.Tasks.Task<ServiceResult<WeeklyReportDto>> GetWeeklyReportByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetWeeklyReportsAsync(WeeklyReportQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<WeeklyReportDto>> CreateWeeklyReportAsync(CreateWeeklyReportDto request);
    System.Threading.Tasks.Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportAsync(Guid id, UpdateWeeklyReportDto request);
    System.Threading.Tasks.Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportStatusAsync(Guid id, dotnet_rest_api.Models.WeeklyReportStatus status);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWeeklyReportAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetProjectWeeklyReportsAsync(Guid projectId, WeeklyReportQueryParameters parameters);
}

public interface IWorkRequestApprovalService
{
    System.Threading.Tasks.Task<ServiceResult<bool>> SubmitForApprovalAsync(Guid workRequestId, SubmitForApprovalRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> ProcessApprovalAsync(Guid workRequestId, ApprovalRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<ApprovalWorkflowStatusDto>> GetApprovalStatusAsync(Guid workRequestId);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<WorkRequestApprovalDto>>> GetApprovalHistoryAsync(Guid workRequestId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<bool>> EscalateApprovalAsync(Guid workRequestId, Guid escalateToUserId, string reason, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<WorkRequestDto>>> GetPendingApprovalsAsync(Guid userId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<ApprovalStatisticsDto>> GetApprovalStatisticsAsync(Guid? userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> BulkApprovalAsync(BulkApprovalRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> SendApprovalRemindersAsync();
}

public interface INotificationService
{
    System.Threading.Tasks.Task SendNotificationAsync(string message, Guid userId);
    System.Threading.Tasks.Task SendWorkRequestNotificationAsync(Guid workRequestId, NotificationType type, Guid recipientId, string message, Guid? senderId = null);
    System.Threading.Tasks.Task SendDailyReportNotificationAsync(Guid dailyReportId, string type, Guid projectId, string message);
    System.Threading.Tasks.Task SendProjectStatusUpdateAsync(Guid projectId, string statusUpdate, decimal? completionPercentage = null);
    System.Threading.Tasks.Task SendApprovalRequiredNotificationAsync(Guid workRequestId, string title, decimal? estimatedCost);
    System.Threading.Tasks.Task SendNotificationCountUpdateAsync(Guid userId);
    System.Threading.Tasks.Task SendSystemAnnouncementAsync(string title, string message, string priority = "Info");
    System.Threading.Tasks.Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
}

public interface IEmailService
{
    System.Threading.Tasks.Task SendEmailAsync(string to, string subject, string body);
}

public interface ICalendarService
{
    System.Threading.Tasks.Task<ServiceResult<PaginatedCalendarEventsDto>> GetEventsAsync(CalendarQueryDto query);
    System.Threading.Tasks.Task<ServiceResult<CalendarEventResponseDto>> GetEventByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<CalendarEventResponseDto>> CreateEventAsync(CreateCalendarEventDto request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<CalendarEventResponseDto>> UpdateEventAsync(Guid id, UpdateCalendarEventDto request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteEventAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetProjectEventsAsync(Guid projectId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetTaskEventsAsync(Guid taskId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<CalendarEventSummaryDto>>> GetUserEventsAsync(Guid userId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<IEnumerable<CalendarEventSummaryDto>>> GetUpcomingEventsAsync(int days, Guid? userId);
    System.Threading.Tasks.Task<ServiceResult<ConflictCheckResult>> CheckConflictsAsync(DateTime startDateTime, DateTime endDateTime, Guid userId, Guid? excludeEventId);
    System.Threading.Tasks.Task<ServiceResult<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventsAsync();
    System.Threading.Tasks.Task<ServiceResult<CalendarEventResponseDto>> CreateRecurringEventAsync(CreateCalendarEventDto request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> UpdateRecurringEventAsync(Guid seriesId, UpdateCalendarEventDto request, bool updateAll);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteRecurringEventAsync(Guid seriesId, bool deleteAll);
}





public interface ICloudStorageService
{
    System.Threading.Tasks.Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    System.Threading.Tasks.Task<bool> DeleteFileAsync(string fileName);
    System.Threading.Tasks.Task<string> GetFileUrlAsync(string fileName);
}

public interface IDataSeeder
{
    Task SeedSampleDataAsync();
}

public interface IAuthService
{
    System.Threading.Tasks.Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    System.Threading.Tasks.Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request);
    System.Threading.Tasks.Task<ServiceResult<string>> RefreshTokenAsync(string refreshToken);
    System.Threading.Tasks.Task<ServiceResult<bool>> LogoutAsync(string token);
    bool ValidateTokenAsync(string token);
}

public interface IRateLimitService
{
    string GetClientIdentifier(HttpContext context);
    System.Threading.Tasks.Task<RateLimitResult> CheckRateLimit(string clientId, string endpoint, string method);
}

public interface IRateLimitMonitoringService
{
    Task RecordRateLimitHit(string clientId, string rule, string endpoint, bool isAllowed);
    System.Threading.Tasks.Task<RateLimitStatistics> GetStatistics(TimeSpan period);
    System.Threading.Tasks.Task<List<ClientRateLimitInfo>> GetTopClients(int count);
    System.Threading.Tasks.Task<List<RateLimitViolation>> GetRecentViolations(TimeSpan period);
    System.Threading.Tasks.Task<Dictionary<string, RateLimitRule>> GetActiveRules();
    Task ClearClientLimits(string clientId);
    Task ClearAllLimits();
    Task UpdateRuleConfiguration(string ruleName, RateLimitRule rule);
}

public interface IRateLimitStorage
{
    System.Threading.Tasks.Task<int> GetRequestCountAsync(string key);
    Task IncrementRequestCountAsync(string key, TimeSpan expiration);
    Task ResetRequestCountAsync(string key);
}

public interface IResourceService
{
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<ResourceDto>>> GetResourcesAsync(ResourceQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<ResourceDto>> GetResourceByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<ResourceDto>> CreateResourceAsync(CreateResourceRequest request);
    System.Threading.Tasks.Task<ServiceResult<ResourceDto>> UpdateResourceAsync(Guid id, UpdateResourceRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteResourceAsync(Guid id);
}

public interface IDocumentService
{
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<DocumentDto>>> GetDocumentsAsync(DocumentQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<DocumentDto>> GetDocumentByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<DocumentDto>> CreateDocumentAsync(CreateDocumentRequest request);
    System.Threading.Tasks.Task<ServiceResult<DocumentDto>> UpdateDocumentAsync(Guid id, UpdateDocumentRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteDocumentAsync(Guid id);
}

public interface IDailyReportService
{
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string token);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteDailyReportAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, DailyReportQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> SubmitDailyReportAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid approverId);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid approverId, string reason);
    System.Threading.Tasks.Task<ServiceResult<List<DailyReportAttachmentDto>>> GetDailyReportAttachmentsAsync(Guid reportId);
    System.Threading.Tasks.Task<ServiceResult<DailyReportAttachmentDto>> AddDailyReportAttachmentAsync(Guid reportId, string fileName, string filePath, string fileType, long fileSize, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<DailyReportAttachmentDto>> AddAttachmentAsync(Guid reportId, IFormFile file, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> RemoveDailyReportAttachmentAsync(Guid attachmentId);
    System.Threading.Tasks.Task<ServiceResult<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate);
    System.Threading.Tasks.Task<ServiceResult<List<DailyReportDto>>> GetUserDailyReportsAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    System.Threading.Tasks.Task<ServiceResult<string>> ExportDailyReportsAsync(DailyReportQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<DailyReportDto>> GetDailyReportByUserAndDateAsync(Guid userId, DateTime date);
    System.Threading.Tasks.Task<ServiceResult<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request);
    System.Threading.Tasks.Task<ServiceResult<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteWorkProgressItemAsync(Guid itemId);

    // Enhanced Daily Report Operations
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<EnhancedDailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, EnhancedDailyReportQueryParameters parameters);
    System.Threading.Tasks.Task<ServiceResult<EnhancedDailyReportDto>> CreateEnhancedDailyReportAsync(EnhancedCreateDailyReportRequest request, Guid userId);
    System.Threading.Tasks.Task<ServiceResult<bool>> ValidateProjectAccessAsync(Guid projectId, Guid userId);
    
    // Analytics and Reporting
    System.Threading.Tasks.Task<ServiceResult<DailyReportAnalyticsDto>> GetDailyReportAnalyticsAsync(Guid projectId, DateTime startDate, DateTime endDate);
    System.Threading.Tasks.Task<ServiceResult<WeeklySummaryDto>> GetWeeklyProgressReportAsync(Guid projectId, DateTime weekStartDate);
    System.Threading.Tasks.Task<ServiceResult<DailyReportInsightsDto>> GetDailyReportInsightsAsync(Guid projectId, Guid? reportId = null);
    
    // Bulk Operations
    System.Threading.Tasks.Task<ServiceResult<BulkOperationResultDto>> BulkApproveDailyReportsAsync(DailyReportBulkApprovalRequest request, Guid approverId);
    System.Threading.Tasks.Task<ServiceResult<BulkOperationResultDto>> BulkRejectDailyReportsAsync(DailyReportBulkRejectionRequest request, Guid approverId);
    
    // Enhanced Export
    System.Threading.Tasks.Task<ServiceResult<byte[]>> ExportEnhancedDailyReportsAsync(DailyReportExportRequest request);
    
    // Validation and Templates
    System.Threading.Tasks.Task<ServiceResult<DailyReportValidationResultDto>> ValidateDailyReportAsync(EnhancedCreateDailyReportRequest request);
    System.Threading.Tasks.Task<ServiceResult<List<DailyReportTemplateDto>>> GetDailyReportTemplatesAsync(Guid projectId);
    
    // Workflow Management
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetPendingApprovalsAsync(Guid? projectId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<List<ApprovalHistoryDto>>> GetApprovalHistoryAsync(Guid reportId);
}
