using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;

namespace dotnet_rest_api.Services;

/// <summary>
/// Stub implementation for IDailyReportService to get a working build
/// </summary>
public class StubDailyReportService : IDailyReportService
{
    private readonly ApplicationDbContext _context;

    public StubDailyReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        var result = new EnhancedPagedResult<DailyReportDto>
        {
            Items = new List<DailyReportDto>(),
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0
        };
        return Task.FromResult(ServiceResult<EnhancedPagedResult<DailyReportDto>>.SuccessResult(result));
    }

    public Task<ServiceResult<DailyReportDto>> GetDailyReportByIdAsync(Guid id)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<bool>> DeleteDailyReportAsync(Guid id)
    {
        return Task.FromResult(ServiceResult<bool>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<EnhancedPagedResult<DailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, DailyReportQueryParameters parameters)
    {
        var result = new EnhancedPagedResult<DailyReportDto>
        {
            Items = new List<DailyReportDto>(),
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 0
        };
        return Task.FromResult(ServiceResult<EnhancedPagedResult<DailyReportDto>>.SuccessResult(result));
    }

    public Task<ServiceResult<DailyReportDto>> SubmitDailyReportAsync(Guid id)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid approverId)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid approverId, string reason)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<List<DailyReportAttachmentDto>>> GetDailyReportAttachmentsAsync(Guid reportId)
    {
        return Task.FromResult(ServiceResult<List<DailyReportAttachmentDto>>.SuccessResult(new List<DailyReportAttachmentDto>()));
    }

    public Task<ServiceResult<DailyReportAttachmentDto>> AddDailyReportAttachmentAsync(Guid reportId, string fileName, string filePath, string fileType, long fileSize, Guid userId)
    {
        return Task.FromResult(ServiceResult<DailyReportAttachmentDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<bool>> RemoveDailyReportAttachmentAsync(Guid attachmentId)
    {
        return Task.FromResult(ServiceResult<bool>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate)
    {
        return Task.FromResult(ServiceResult<WeeklySummaryDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<List<DailyReportDto>>> GetUserDailyReportsAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        return Task.FromResult(ServiceResult<List<DailyReportDto>>.SuccessResult(new List<DailyReportDto>()));
    }

    public Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string token)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportAttachmentDto>> AddAttachmentAsync(Guid reportId, IFormFile file, Guid userId)
    {
        return Task.FromResult(ServiceResult<DailyReportAttachmentDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<string>> ExportDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        return Task.FromResult(ServiceResult<string>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<DailyReportDto>> GetDailyReportByUserAndDateAsync(Guid userId, DateTime date)
    {
        return Task.FromResult(ServiceResult<DailyReportDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request)
    {
        return Task.FromResult(ServiceResult<WorkProgressItemDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request)
    {
        return Task.FromResult(ServiceResult<WorkProgressItemDto>.ErrorResult("Not implemented"));
    }

    public Task<ServiceResult<bool>> DeleteWorkProgressItemAsync(Guid itemId)
    {
        return Task.FromResult(ServiceResult<bool>.ErrorResult("Not implemented"));
    }
}
