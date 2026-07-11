using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;
using Microsoft.AspNetCore.Http;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IDailyReportService
{
    Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<ServiceResult<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    Task<ServiceResult<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId);
    Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string? userRole = null);
    Task<ServiceResult<bool>> DeleteDailyReportAsync(Guid id);
    Task<ServiceResult<DailyReportAttachmentDto>> AddAttachmentAsync(Guid id, IFormFile file, Guid userId);
    Task<ServiceResult<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate);
    Task<ServiceResult<byte[]>> ExportDailyReportsAsync(DailyReportQueryParameters parameters);
    Task<ServiceResult<DailyReportDto>> SubmitDailyReportAsync(Guid id);
    Task<ServiceResult<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid userId);
    Task<ServiceResult<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid userId, string? rejectionReason = null);
    Task<ServiceResult<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request);
    Task<ServiceResult<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request);
    Task<ServiceResult<bool>> DeleteWorkProgressItemAsync(Guid itemId);
    Task<ServiceResult<EnhancedPagedResult<EnhancedDailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, EnhancedDailyReportQueryParameters parameters);
    Task<ServiceResult<bool>> ValidateProjectAccessAsync(Guid projectId, Guid userId);
    Task<ServiceResult<EnhancedDailyReportDto>> CreateEnhancedDailyReportAsync(EnhancedCreateDailyReportRequest request, Guid userId);
    Task<ServiceResult<DailyReportAnalyticsDto>> GetDailyReportAnalyticsAsync(Guid projectId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<WeeklySummaryDto>> GetWeeklyProgressReportAsync(Guid projectId, DateTime weekStart);
    Task<ServiceResult<BulkOperationResultDto>> BulkApproveDailyReportsAsync(DailyReportBulkApprovalRequest request, Guid userId);
    Task<ServiceResult<BulkOperationResultDto>> BulkRejectDailyReportsAsync(DailyReportBulkRejectionRequest request, Guid userId);
    Task<ServiceResult<byte[]>> ExportEnhancedDailyReportsAsync(DailyReportExportRequest request);
    Task<ServiceResult<DailyReportInsightsDto>> GetDailyReportInsightsAsync(Guid projectId, Guid? reportId = null);
    Task<ServiceResult<DailyReportValidationResultDto>> ValidateDailyReportAsync(EnhancedCreateDailyReportRequest request);
    Task<ServiceResult<List<DailyReportTemplateDto>>> GetDailyReportTemplatesAsync(Guid projectId);
    Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetPendingApprovalsAsync(Guid? projectId = null, int pageNumber = 1, int pageSize = 20);
    Task<ServiceResult<List<ApprovalHistoryDto>>> GetApprovalHistoryAsync(Guid reportId);
}
