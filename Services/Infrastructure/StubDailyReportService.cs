using dotnet_rest_api.DTOs;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IDailyReportService
{
    Task<ServiceResult<PagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters);
}

public class StubDailyReportService : IDailyReportService
{
    public Task<ServiceResult<PagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        var result = new PagedResult<DailyReportDto>
        {
            Items = new List<DailyReportDto>(),
            TotalCount = 0,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };

        return Task.FromResult(ServiceResult<PagedResult<DailyReportDto>>.SuccessResult(result, "Daily reports retrieved successfully"));
    }
}
