using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;
using Microsoft.AspNetCore.Http;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IDailyReportService
{
    Task<Result<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<Result<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    Task<Result<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId);
    Task<Result<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string? userRole = null);
    Task<Result<bool>> DeleteDailyReportAsync(Guid id);
    Task<Result<DailyReportAttachmentDto>> AddAttachmentAsync(Guid id, IFormFile file, Guid userId);
    Task<Result<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate);
    Task<Result<byte[]>> ExportDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<Result<DailyReportDto>> SubmitDailyReportAsync(Guid id);
    Task<Result<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid userId);
    Task<Result<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid userId, string? rejectionReason = null);
    Task<Result<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request);
    Task<Result<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request);
    Task<Result<bool>> DeleteWorkProgressItemAsync(Guid itemId);
    Task<Result<EnhancedPagedResult<EnhancedDailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, EnhancedDailyReportQueryParameters parameters);
    Task<Result<bool>> ValidateProjectAccessAsync(Guid projectId, Guid userId);
    Task<Result<EnhancedDailyReportDto>> CreateEnhancedDailyReportAsync(EnhancedCreateDailyReportRequest request, Guid userId);
    Task<Result<DailyReportAnalyticsDto>> GetDailyReportAnalyticsAsync(Guid projectId, DateTime startDate, DateTime endDate);
    Task<Result<WeeklySummaryDto>> GetWeeklyProgressReportAsync(Guid projectId, DateTime weekStart);
    Task<Result<BulkOperationResultDto>> BulkApproveDailyReportsAsync(DailyReportBulkApprovalRequest request, Guid userId);
    Task<Result<BulkOperationResultDto>> BulkRejectDailyReportsAsync(DailyReportBulkRejectionRequest request, Guid userId);
    Task<Result<byte[]>> ExportEnhancedDailyReportsAsync(DailyReportExportRequest request);
    Task<Result<DailyReportInsightsDto>> GetDailyReportInsightsAsync(Guid projectId, Guid? reportId = null);
    Task<Result<DailyReportValidationResultDto>> ValidateDailyReportAsync(EnhancedCreateDailyReportRequest request);
    Task<Result<List<DailyReportTemplateDto>>> GetDailyReportTemplatesAsync(Guid projectId);
    Task<Result<EnhancedPagedResult<DailyReportDto>>> GetPendingApprovalsAsync(Guid? projectId = null, int pageNumber = 1, int pageSize = 20);
    Task<Result<List<ApprovalHistoryDto>>> GetApprovalHistoryAsync(Guid reportId);
}
