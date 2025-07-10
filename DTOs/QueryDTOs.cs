namespace dotnet_rest_api.DTOs;

// Advanced filtering parameters
public class FilterParameter
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = "eq"; // eq, ne, gt, gte, lt, lte, contains, startswith, endswith, in
    public string Value { get; set; } = string.Empty;
}

// Interface to identify query parameters that support filtering
public interface IFilterableQuery
{
    List<FilterParameter> Filters { get; set; }
}

// Project-specific query parameters
public class ProjectQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public string? ProjectName { get; set; }
    public string? Status { get; set; }
    public string? ClientInfo { get; set; }
    public DateTime? StartDateAfter { get; set; }
    public DateTime? StartDateBefore { get; set; }
    public DateTime? EstimatedEndDateAfter { get; set; }
    public DateTime? EstimatedEndDateBefore { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? UserId { get; set; } // Used for mobile app to filter by assigned user
    public string? Address { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// User-specific query parameters
public class UserQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Task-specific query parameters
public class TaskQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDateAfter { get; set; }
    public DateTime? DueDateBefore { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? CompletedAfter { get; set; }
    public DateTime? CompletedBefore { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Image-specific query parameters
public class ImageQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? UploadedById { get; set; }
    public DateTime? UploadedAfter { get; set; }
    public DateTime? UploadedBefore { get; set; }
    public DateTime? CapturedAfter { get; set; }
    public DateTime? CapturedBefore { get; set; }
    public string? ContentType { get; set; }
    public string? DeviceModel { get; set; }
    public long? MinFileSize { get; set; }
    public long? MaxFileSize { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Daily Report query parameters
public class DailyReportQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public Guid? ProjectId { get; set; }
    public Guid? ReporterId { get; set; }
    public string? Status { get; set; }
    public DateTime? ReportDateAfter { get; set; }
    public DateTime? ReportDateBefore { get; set; }
    public string? WeatherCondition { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public bool? HasWorkProgress { get; set; }
    public bool? HasIssues { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Work Request query parameters
public class WorkRequestQueryParameters : BaseQueryParameters, IFilterableQuery
{
    public Guid? ProjectId { get; set; }
    public Guid? RequestedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Type { get; set; }
    public string? Title { get; set; }
    public DateTime? DueDateAfter { get; set; }
    public DateTime? DueDateBefore { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? UpdatedAfter { get; set; }
    public DateTime? UpdatedBefore { get; set; }
    public DateTime? CompletedDateAfter { get; set; }
    public DateTime? CompletedDateBefore { get; set; }
    public bool? HasTasks { get; set; }
    public bool? HasComments { get; set; }
    
    // Generic filtering support
    public List<FilterParameter> Filters { get; set; } = new();
}

// Query result with metadata
public class QueryResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public List<string> RequestedFields { get; set; } = new();
    public QueryMetadata Metadata { get; set; } = new();
}

// Dynamic filter builder for complex queries
public class DynamicFilterBuilder
{
    public string BuildWhereClause(List<FilterParameter> filters)
    {
        if (!filters.Any()) return string.Empty;
        
        var conditions = filters.Select(BuildCondition);
        return string.Join(" AND ", conditions);
    }
    
    private string BuildCondition(FilterParameter filter)
    {
        return filter.Operator.ToLower() switch
        {
            "eq" => $"{filter.Field} = '{filter.Value}'",
            "ne" => $"{filter.Field} != '{filter.Value}'",
            "gt" => $"{filter.Field} > '{filter.Value}'",
            "gte" => $"{filter.Field} >= '{filter.Value}'",
            "lt" => $"{filter.Field} < '{filter.Value}'",
            "lte" => $"{filter.Field} <= '{filter.Value}'",
            "contains" => $"{filter.Field}.Contains(\"{filter.Value}\")",
            "startswith" => $"{filter.Field}.StartsWith(\"{filter.Value}\")",
            "endswith" => $"{filter.Field}.EndsWith(\"{filter.Value}\")",
            "in" => $"{filter.Value}.Split(',').Contains({filter.Field})",
            _ => $"{filter.Field} = '{filter.Value}'"
        };
    }
}

// Field selection helper for sparse fieldsets
public class FieldSelector
{
    public static object SelectFields<T>(T source, string? fields)
    {
        if (string.IsNullOrEmpty(fields) || source == null)
            return source;

        var requestedFields = fields.Split(',')
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();

        if (!requestedFields.Any())
            return source;

        var sourceType = typeof(T);
        var properties = sourceType.GetProperties();
        var result = new Dictionary<string, object?>();

        foreach (var fieldName in requestedFields)
        {
            var property = properties.FirstOrDefault(p => 
                string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
            
            if (property != null)
            {
                result[property.Name] = property.GetValue(source);
            }
        }

        return result;
    }
    
    public static List<object> SelectFields<T>(List<T> source, string? fields)
    {
        return source.Select(item => SelectFields(item, fields)).ToList();
    }
}