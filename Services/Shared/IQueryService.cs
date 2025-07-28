using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Shared;

/// <summary>
/// Interface for query service operations
/// </summary>
public interface IQueryService
{
    ApiResponseWithPagination<T> CreateRichPaginatedResponse<T>(
        List<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string baseUrl,
        Dictionary<string, string> queryParams,
        string message);
}
