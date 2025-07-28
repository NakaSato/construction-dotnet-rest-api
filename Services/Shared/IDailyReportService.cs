using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Interface for daily report service
/// </summary>
public interface IDailyReportService
{
    /// <summary>
    /// Get all daily reports with pagination
    /// </summary>
    Task<ServiceResult<IEnumerable<DailyReportDto>>> GetDailyReportsAsync(int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily report by ID
    /// </summary>
    Task<ServiceResult<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    
    /// <summary>
    /// Get daily reports by project ID
    /// </summary>
    Task<ServiceResult<IEnumerable<DailyReportDto>>> GetDailyReportsByProjectIdAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily reports by user ID
    /// </summary>
    Task<ServiceResult<IEnumerable<DailyReportDto>>> GetDailyReportsByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily reports by date range
    /// </summary>
    Task<ServiceResult<IEnumerable<DailyReportDto>>> GetDailyReportsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? projectId = null);
    
    /// <summary>
    /// Create new daily report
    /// </summary>
    Task<ServiceResult<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request);
    
    /// <summary>
    /// Update existing daily report
    /// </summary>
    Task<ServiceResult<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request);
    
    /// <summary>
    /// Delete daily report
    /// </summary>
    Task<ServiceResult<bool>> DeleteDailyReportAsync(Guid id);
    
    /// <summary>
    /// Get daily report statistics
    /// </summary>
    Task<ServiceResult<object>> GetDailyReportStatsAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null);
}
