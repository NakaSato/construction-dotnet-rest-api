using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.DTOs;
using System.Diagnostics;

namespace dotnet_rest_api.Services;

public class QueryService : IQueryService
{
    public async Task<EnhancedPagedResult<T>> ExecuteQueryAsync<T>(
        IQueryable<T> query,
        BaseQueryParameters parameters) where T : class
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Apply filtering if available
        if (parameters is IFilterableQuery filterableQuery && filterableQuery.Filters.Any())
        {
            query = ApplyFilters(query, filterableQuery.Filters);
        }
        
        // Apply sorting
        query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        stopwatch.Stop();
        
        // Apply field selection if specified
        var selectedItems = ApplyFieldSelection(items, parameters.Fields);
        
        var metadata = new QueryMetadata
        {
            ExecutionTime = stopwatch.Elapsed,
            FiltersApplied = parameters is IFilterableQuery fq ? fq.Filters.Count : 0,
            QueryComplexity = DetermineComplexity(parameters),
            QueryExecutedAt = DateTime.UtcNow,
            CacheStatus = "Miss" // Can be enhanced with caching later
        };
        
        return new EnhancedPagedResult<T>
        {
            Items = selectedItems.Any() ? selectedItems.Cast<T>().ToList() : items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder,
            RequestedFields = string.IsNullOrEmpty(parameters.Fields) 
                ? new List<string>() 
                : parameters.Fields.Split(',').Select(f => f.Trim()).ToList(),
            Metadata = metadata
        };
    }
    
    public IQueryable<T> ApplyFilters<T>(
        IQueryable<T> query,
        List<FilterParameter> filters) where T : class
    {
        foreach (var filter in filters)
        {
            query = ApplyFilter(query, filter);
        }
        return query;
    }
    
    private IQueryable<T> ApplyFilter<T>(
        IQueryable<T> query,
        FilterParameter filter) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, filter.Field);
        
        if (property == null) return query;
        
        var filterExpression = BuildFilterExpression(property, filter);
        if (filterExpression == null) return query;
        
        var lambda = Expression.Lambda<Func<T, bool>>(filterExpression, parameter);
        return query.Where(lambda);
    }
    
    private Expression? GetPropertyExpression(ParameterExpression parameter, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        Expression expression = parameter;
        
        foreach (var prop in properties)
        {
            var propertyInfo = expression.Type.GetProperty(prop, 
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            if (propertyInfo == null) return null;
            
            expression = Expression.Property(expression, propertyInfo);
        }
        
        return expression;
    }
    
    private Expression? BuildFilterExpression(Expression property, FilterParameter filter)
    {
        var propertyType = property.Type;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        object? convertedValue;
        try
        {
            convertedValue = ConvertValue(filter.Value, underlyingType);
        }
        catch
        {
            return null; // Invalid conversion, skip this filter
        }
        
        var constantValue = Expression.Constant(convertedValue, propertyType);
        
        return filter.Operator.ToLower() switch
        {
            "eq" => Expression.Equal(property, constantValue),
            "ne" => Expression.NotEqual(property, constantValue),
            "gt" => Expression.GreaterThan(property, constantValue),
            "gte" => Expression.GreaterThanOrEqual(property, constantValue),
            "lt" => Expression.LessThan(property, constantValue),
            "lte" => Expression.LessThanOrEqual(property, constantValue),
            "contains" when underlyingType == typeof(string) => 
                Expression.Call(property, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constantValue),
            "startswith" when underlyingType == typeof(string) => 
                Expression.Call(property, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, constantValue),
            "endswith" when underlyingType == typeof(string) => 
                Expression.Call(property, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, constantValue),
            "in" => BuildInExpression(property, filter.Value),
            _ => null
        };
    }
    
    private Expression? BuildInExpression(Expression property, string values)
    {
        var valueArray = values.Split(',').Select(v => v.Trim()).ToArray();
        var propertyType = property.Type;
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        
        try
        {
            var convertedValues = valueArray.Select(v => ConvertValue(v, underlyingType)).ToArray();
            var listType = typeof(List<>).MakeGenericType(underlyingType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            
            foreach (var value in convertedValues)
            {
                addMethod?.Invoke(list, new[] { value });
            }
            
            var constantList = Expression.Constant(list);
            var containsMethod = listType.GetMethod("Contains");
            
            return Expression.Call(constantList, containsMethod!, property);
        }
        catch
        {
            return null;
        }
    }
    
    private object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value)) return null;
        
        if (targetType == typeof(string)) return value;
        if (targetType == typeof(Guid)) return Guid.Parse(value);
        if (targetType == typeof(DateTime)) return DateTime.Parse(value);
        if (targetType == typeof(bool)) return bool.Parse(value);
        if (targetType == typeof(int)) return int.Parse(value);
        if (targetType == typeof(long)) return long.Parse(value);
        if (targetType == typeof(double)) return double.Parse(value);
        if (targetType == typeof(decimal)) return decimal.Parse(value);
        
        return Convert.ChangeType(value, targetType);
    }
    
    public IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        string? sortBy,
        string? sortOrder) where T : class
    {
        if (string.IsNullOrEmpty(sortBy)) return query;
        
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, sortBy);
        
        if (property == null) return query;
        
        var lambda = Expression.Lambda(property, parameter);
        var methodName = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase) 
            ? "OrderByDescending" 
            : "OrderBy";
        
        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type);
        
        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }
    
    public List<object> ApplyFieldSelection<T>(
        List<T> items,
        string? fields) where T : class
    {
        if (string.IsNullOrEmpty(fields) || !items.Any())
            return new List<object>();
        
        var requestedFields = fields.Split(',')
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();
        
        if (!requestedFields.Any())
            return new List<object>();
        
        var sourceType = typeof(T);
        var properties = sourceType.GetProperties()
            .Where(p => requestedFields.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        
        var result = new List<object>();
        
        foreach (var item in items)
        {
            var selectedItem = new Dictionary<string, object?>();
            
            foreach (var property in properties)
            {
                selectedItem[property.Name] = property.GetValue(item);
            }
            
            result.Add(selectedItem);
        }
        
        return result;
    }
    
    public PaginationLinks GeneratePaginationLinks(
        string baseUrl,
        int currentPage,
        int totalPages,
        int pageSize,
        Dictionary<string, string>? queryParams = null)
    {
        var links = new PaginationLinks();
        
        // Build query string from parameters
        var queryString = BuildQueryString(queryParams, pageSize);
        
        // First page
        links.First = $"{baseUrl}?page=1{queryString}";
        
        // Previous page
        if (currentPage > 1)
        {
            links.Previous = $"{baseUrl}?page={currentPage - 1}{queryString}";
        }
        
        // Current page
        links.Current = $"{baseUrl}?page={currentPage}{queryString}";
        
        // Next page
        if (currentPage < totalPages)
        {
            links.Next = $"{baseUrl}?page={currentPage + 1}{queryString}";
        }
        
        // Last page
        if (totalPages > 0)
        {
            links.Last = $"{baseUrl}?page={totalPages}{queryString}";
        }
        
        return links;
    }
    
    public ApiResponseWithPagination<T> CreateRichPaginatedResponse<T>(
        List<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string baseUrl,
        Dictionary<string, string> queryParams,
        string message = "Data retrieved successfully")
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        var pagination = new PaginationInfo
        {
            TotalItems = totalCount,
            TotalPages = totalPages,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            Links = GeneratePaginationLinks(baseUrl, pageNumber, totalPages, pageSize, queryParams)
        };
        
        return new ApiResponseWithPagination<T>
        {
            Success = true,
            Message = message,
            Data = new ApiDataWithPagination<T>
            {
                Items = items,
                Pagination = pagination
            }
        };
    }
    
    private string BuildQueryString(Dictionary<string, string>? queryParams, int pageSize)
    {
        var queryString = $"&pageSize={pageSize}";
        
        if (queryParams != null)
        {
            foreach (var param in queryParams)
            {
                if (!string.Equals(param.Key, "page", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(param.Key, "pageSize", StringComparison.OrdinalIgnoreCase))
                {
                    queryString += $"&{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}";
                }
            }
        }
        
        return queryString;
    }
    
    private string DetermineComplexity(BaseQueryParameters parameters)
    {
        var complexity = 0;
        
        // Base complexity for pagination
        complexity += 1;
        
        // Add complexity for sorting
        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            complexity += 1;
        }
        
        // Add complexity for field selection
        if (!string.IsNullOrEmpty(parameters.Fields))
        {
            complexity += 1;
        }
        
        // Add complexity for filters
        if (parameters is IFilterableQuery filterableQuery)
        {
            complexity += filterableQuery.Filters.Count;
        }
        
        return complexity switch
        {
            <= 2 => "Simple",
            <= 5 => "Medium",
            _ => "Complex"
        };
    }
}
    
// Extension methods for query parameter parsing
public static class QueryParameterExtensions
{
    public static List<FilterParameter> ParseFilters(this BaseQueryParameters parameters, string? filterString)
    {
        if (string.IsNullOrEmpty(filterString))
            return new List<FilterParameter>();
        
        var filters = new List<FilterParameter>();
        var filterPairs = filterString.Split('&');
        
        foreach (var pair in filterPairs)
        {
            var parts = pair.Split('=');
            if (parts.Length != 2) continue;
            
            var fieldAndOperator = parts[0].Split("__");
            var field = fieldAndOperator[0];
            var operatorName = fieldAndOperator.Length > 1 ? fieldAndOperator[1] : "eq";
            var value = Uri.UnescapeDataString(parts[1]);
            
            filters.Add(new FilterParameter
            {
                Field = field,
                Operator = operatorName,
                Value = value
            });
        }
        
        return filters;
    }
}
