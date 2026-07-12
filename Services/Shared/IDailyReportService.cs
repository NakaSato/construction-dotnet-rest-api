using dotnet_rest_api.Common;
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
    Task<Result<IEnumerable<DailyReportDto>>> GetDailyReportsAsync(int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily report by ID
    /// </summary>
    Task<Result<DailyReportDto>> GetDailyReportByIdAsync(Guid id);
    
    /// <summary>
    /// Get daily reports by project ID
    /// </summary>
    Task<Result<IEnumerable<DailyReportDto>>> GetDailyReportsByProjectIdAsync(Guid projectId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily reports by user ID
    /// </summary>
    Task<Result<IEnumerable<DailyReportDto>>> GetDailyReportsByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
    
    /// <summary>
    /// Get daily reports by date range
    /// </summary>
    Task<Result<IEnumerable<DailyReportDto>>> GetDailyReportsByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? projectId = null);
    
    /// <summary>
    /// Create new daily report
    /// </summary>
    Task<Result<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request);
    
    /// <summary>
    /// Update existing daily report
    /// </summary>
    Task<Result<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request);
    
    /// <summary>
    /// Delete daily report
    /// </summary>
    Task<Result<bool>> DeleteDailyReportAsync(Guid id);
    
    /// <summary>
    /// Get daily report statistics
    /// </summary>
    Task<Result<object>> GetDailyReportStatsAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null);
}
