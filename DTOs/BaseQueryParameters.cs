namespace dotnet_rest_api.DTOs;

/// <summary>
/// Base query parameters for pagination and filtering
/// </summary>
public class BaseQueryParameters
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 10 : value;
    }

    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    public string? Search { get; set; }
    public string? Fields { get; set; }
}
