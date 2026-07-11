using System.Text.Json;
using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IWeeklyReportService
{
    Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetWeeklyReportsAsync(WeeklyReportQueryParameters parameters);
    Task<ServiceResult<WeeklyReportDto>> GetWeeklyReportByIdAsync(Guid id);
    Task<ServiceResult<WeeklyReportDto>> CreateWeeklyReportAsync(CreateWeeklyReportDto request);
    Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportAsync(Guid id, UpdateWeeklyReportDto request);
    Task<ServiceResult<bool>> DeleteWeeklyReportAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetProjectWeeklyReportsAsync(Guid projectId, WeeklyReportQueryParameters parameters);
    Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportStatusAsync(Guid id, WeeklyReportStatus status);
}

public interface IWeeklyWorkRequestService
{
    Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetWeeklyWorkRequestsAsync(WeeklyWorkRequestQueryParameters parameters);
    Task<ServiceResult<WeeklyWorkRequestDto>> GetWeeklyWorkRequestByIdAsync(Guid id);
    Task<ServiceResult<WeeklyWorkRequestDto>> CreateWeeklyWorkRequestAsync(CreateWeeklyWorkRequestDto request);
    Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestAsync(Guid id, UpdateWeeklyWorkRequestDto request);
    Task<ServiceResult<bool>> DeleteWeeklyWorkRequestAsync(Guid id);
    Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetProjectWeeklyWorkRequestsAsync(Guid projectId, WeeklyWorkRequestQueryParameters parameters);
    Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestStatusAsync(Guid id, WeeklyRequestStatus status);
}

/// <summary>
/// Real EF Core-backed implementation of <see cref="IWeeklyReportService"/>.
/// Replaces the former StubWeeklyReportService. JSON-array columns
/// (MajorAccomplishments, MajorIssues) are (de)serialized via the existing
/// AutoMapper profile on read and here on write.
/// </summary>
public class WeeklyReportService : IWeeklyReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WeeklyReportService> _logger;

    public WeeklyReportService(ApplicationDbContext context, IMapper mapper, ILogger<WeeklyReportService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetWeeklyReportsAsync(WeeklyReportQueryParameters parameters)
        => await QueryAsync(parameters, null);

    public async Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> GetProjectWeeklyReportsAsync(Guid projectId, WeeklyReportQueryParameters parameters)
        => await QueryAsync(parameters, projectId);

    private async Task<ServiceResult<EnhancedPagedResult<WeeklyReportDto>>> QueryAsync(WeeklyReportQueryParameters parameters, Guid? projectId)
    {
        var query = BuildBaseQuery();

        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);
        else if (parameters.ProjectId.HasValue)
            query = query.Where(r => r.ProjectId == parameters.ProjectId.Value);

        if (parameters.SubmittedById.HasValue)
            query = query.Where(r => r.SubmittedById == parameters.SubmittedById.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            Enum.TryParse<WeeklyReportStatus>(parameters.Status, true, out var status))
            query = query.Where(r => r.Status == status);
        if (parameters.WeekStartAfter.HasValue)
            query = query.Where(r => r.WeekStartDate >= parameters.WeekStartAfter.Value);
        if (parameters.WeekStartBefore.HasValue)
            query = query.Where(r => r.WeekStartDate <= parameters.WeekStartBefore.Value);
        if (parameters.MinCompletionPercentage.HasValue)
            query = query.Where(r => r.CompletionPercentage >= parameters.MinCompletionPercentage.Value);
        if (parameters.MaxCompletionPercentage.HasValue)
            query = query.Where(r => r.CompletionPercentage <= parameters.MaxCompletionPercentage.Value);
        // Actual-hours filters map to the report's aggregated man-hours.
        if (parameters.MinActualHours.HasValue)
            query = query.Where(r => r.TotalManHours >= parameters.MinActualHours.Value);
        if (parameters.MaxActualHours.HasValue)
            query = query.Where(r => r.TotalManHours <= parameters.MaxActualHours.Value);

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLowerInvariant()) switch
        {
            "status" => descending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "createdat" => descending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "completionpercentage" => descending ? query.OrderByDescending(r => r.CompletionPercentage) : query.OrderBy(r => r.CompletionPercentage),
            _ => descending ? query.OrderByDescending(r => r.WeekStartDate) : query.OrderBy(r => r.WeekStartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<WeeklyReportDto>
        {
            Items = _mapper.Map<List<WeeklyReportDto>>(items),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };
        return ServiceResult<EnhancedPagedResult<WeeklyReportDto>>.SuccessResult(result, "Weekly reports retrieved successfully");
    }

    public async Task<ServiceResult<WeeklyReportDto>> GetWeeklyReportByIdAsync(Guid id)
    {
        var report = await BuildBaseQuery().FirstOrDefaultAsync(r => r.WeeklyReportId == id);
        if (report == null)
            return ServiceResult<WeeklyReportDto>.ErrorResult("Weekly report not found");
        return ServiceResult<WeeklyReportDto>.SuccessResult(_mapper.Map<WeeklyReportDto>(report), "Weekly report retrieved successfully");
    }

    public async Task<ServiceResult<WeeklyReportDto>> CreateWeeklyReportAsync(CreateWeeklyReportDto request)
    {
        if (!await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
            return ServiceResult<WeeklyReportDto>.ErrorResult("Project not found");
        if (!await _context.Users.AnyAsync(u => u.UserId == request.SubmittedById))
            return ServiceResult<WeeklyReportDto>.ErrorResult("Submitting user not found");

        var report = _mapper.Map<WeeklyReport>(request);
        report.WeeklyReportId = Guid.NewGuid();
        report.CreatedAt = DateTime.UtcNow;
        // The create mapping reads metrics from the nested AggregatedMetrics and forces
        // CompletionPercentage to 0; honor the DTO's flat metric fields when no nested
        // object was supplied, and always take the flat completion value.
        if (request.AggregatedMetrics == null)
        {
            report.TotalManHours = request.TotalManHours;
            report.PanelsInstalled = request.PanelsInstalled;
            report.SafetyIncidents = request.SafetyIncidents;
            report.DelaysReported = request.DelaysReported;
        }
        report.CompletionPercentage = request.CompletionPercentage;

        _context.WeeklyReports.Add(report);
        await _context.SaveChangesAsync();

        return await ReloadAndMapAsync(report.WeeklyReportId, "Weekly report created successfully");
    }

    public async Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportAsync(Guid id, UpdateWeeklyReportDto request)
    {
        var report = await _context.WeeklyReports.FirstOrDefaultAsync(r => r.WeeklyReportId == id);
        if (report == null)
            return ServiceResult<WeeklyReportDto>.ErrorResult("Weekly report not found");

        report.WeekStartDate = request.WeekStartDate;
        report.SummaryOfProgress = request.SummaryOfProgress;
        if (request.AggregatedMetrics != null)
        {
            report.TotalManHours = request.AggregatedMetrics.TotalManHours;
            report.PanelsInstalled = request.AggregatedMetrics.PanelsInstalled;
            report.SafetyIncidents = request.AggregatedMetrics.SafetyIncidents;
            report.DelaysReported = request.AggregatedMetrics.DelaysReported;
        }
        if (request.MajorAccomplishments != null)
            report.MajorAccomplishments = JsonSerializer.Serialize(request.MajorAccomplishments);
        if (request.MajorIssues != null)
            report.MajorIssues = JsonSerializer.Serialize(request.MajorIssues);
        if (request.Lookahead != null)
            report.Lookahead = request.Lookahead;
        if (request.CompletionPercentage.HasValue)
            report.CompletionPercentage = request.CompletionPercentage.Value;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await ReloadAndMapAsync(id, "Weekly report updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteWeeklyReportAsync(Guid id)
    {
        var report = await _context.WeeklyReports.FirstOrDefaultAsync(r => r.WeeklyReportId == id);
        if (report == null)
            return ServiceResult<bool>.ErrorResult("Weekly report not found");

        _context.WeeklyReports.Remove(report);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true, "Weekly report deleted successfully");
    }

    public async Task<ServiceResult<WeeklyReportDto>> UpdateWeeklyReportStatusAsync(Guid id, WeeklyReportStatus status)
    {
        var report = await _context.WeeklyReports.FirstOrDefaultAsync(r => r.WeeklyReportId == id);
        if (report == null)
            return ServiceResult<WeeklyReportDto>.ErrorResult("Weekly report not found");

        report.Status = status;
        if (status == WeeklyReportStatus.Approved)
            report.ApprovedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await ReloadAndMapAsync(id, $"Weekly report status set to {status}");
    }

    private IQueryable<WeeklyReport> BuildBaseQuery()
        => _context.WeeklyReports
            .Include(r => r.Project)
            .Include(r => r.SubmittedByUser)
            .Include(r => r.ApprovedByUser)
            .AsQueryable();

    private async Task<ServiceResult<WeeklyReportDto>> ReloadAndMapAsync(Guid id, string message)
    {
        var report = await BuildBaseQuery().FirstOrDefaultAsync(r => r.WeeklyReportId == id);
        if (report == null)
            return ServiceResult<WeeklyReportDto>.ErrorResult("Weekly report not found");
        return ServiceResult<WeeklyReportDto>.SuccessResult(_mapper.Map<WeeklyReportDto>(report), message);
    }
}

/// <summary>
/// Real EF Core-backed implementation of <see cref="IWeeklyWorkRequestService"/>.
/// Replaces the former StubWeeklyWorkRequestService.
/// </summary>
public class WeeklyWorkRequestService : IWeeklyWorkRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WeeklyWorkRequestService> _logger;

    public WeeklyWorkRequestService(ApplicationDbContext context, IMapper mapper, ILogger<WeeklyWorkRequestService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetWeeklyWorkRequestsAsync(WeeklyWorkRequestQueryParameters parameters)
        => await QueryAsync(parameters, null);

    public async Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetProjectWeeklyWorkRequestsAsync(Guid projectId, WeeklyWorkRequestQueryParameters parameters)
        => await QueryAsync(parameters, projectId);

    private async Task<ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>> QueryAsync(WeeklyWorkRequestQueryParameters parameters, Guid? projectId)
    {
        var query = BuildBaseQuery();

        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);
        else if (parameters.ProjectId.HasValue)
            query = query.Where(r => r.ProjectId == parameters.ProjectId.Value);

        if (parameters.RequestedById.HasValue)
            query = query.Where(r => r.RequestedById == parameters.RequestedById.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            Enum.TryParse<WeeklyRequestStatus>(parameters.Status, true, out var status))
            query = query.Where(r => r.Status == status);
        if (parameters.WeekStartAfter.HasValue)
            query = query.Where(r => r.WeekStartDate >= parameters.WeekStartAfter.Value);
        if (parameters.WeekStartBefore.HasValue)
            query = query.Where(r => r.WeekStartDate <= parameters.WeekStartBefore.Value);
        if (parameters.MinEstimatedHours.HasValue)
            query = query.Where(r => r.EstimatedHours >= parameters.MinEstimatedHours.Value);
        if (parameters.MaxEstimatedHours.HasValue)
            query = query.Where(r => r.EstimatedHours <= parameters.MaxEstimatedHours.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Priority))
            query = query.Where(r => r.Priority == parameters.Priority);
        if (!string.IsNullOrWhiteSpace(parameters.Type))
            query = query.Where(r => r.Type == parameters.Type);

        var descending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = (parameters.SortBy?.ToLowerInvariant()) switch
        {
            "status" => descending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "createdat" => descending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "estimatedhours" => descending ? query.OrderByDescending(r => r.EstimatedHours) : query.OrderBy(r => r.EstimatedHours),
            _ => descending ? query.OrderByDescending(r => r.WeekStartDate) : query.OrderBy(r => r.WeekStartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<WeeklyWorkRequestDto>
        {
            Items = _mapper.Map<List<WeeklyWorkRequestDto>>(items),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };
        return ServiceResult<EnhancedPagedResult<WeeklyWorkRequestDto>>.SuccessResult(result, "Weekly work requests retrieved successfully");
    }

    public async Task<ServiceResult<WeeklyWorkRequestDto>> GetWeeklyWorkRequestByIdAsync(Guid id)
    {
        var request = await BuildBaseQuery().FirstOrDefaultAsync(r => r.WeeklyRequestId == id);
        if (request == null)
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Weekly work request not found");
        return ServiceResult<WeeklyWorkRequestDto>.SuccessResult(_mapper.Map<WeeklyWorkRequestDto>(request), "Weekly work request retrieved successfully");
    }

    public async Task<ServiceResult<WeeklyWorkRequestDto>> CreateWeeklyWorkRequestAsync(CreateWeeklyWorkRequestDto request)
    {
        if (!await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Project not found");
        if (!await _context.Users.AnyAsync(u => u.UserId == request.RequestedById))
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Requesting user not found");

        var entity = _mapper.Map<WeeklyWorkRequest>(request);
        entity.WeeklyRequestId = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        // The create mapping forces EstimatedHours/Priority/Type defaults; honor the
        // caller's values here where supplied.
        entity.EstimatedHours = request.EstimatedHours;
        if (!string.IsNullOrWhiteSpace(request.Priority)) entity.Priority = request.Priority;
        if (!string.IsNullOrWhiteSpace(request.Type)) entity.Type = request.Type;

        _context.WeeklyWorkRequests.Add(entity);
        await _context.SaveChangesAsync();

        return await ReloadAndMapAsync(entity.WeeklyRequestId, "Weekly work request created successfully");
    }

    public async Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestAsync(Guid id, UpdateWeeklyWorkRequestDto request)
    {
        var entity = await _context.WeeklyWorkRequests.FirstOrDefaultAsync(r => r.WeeklyRequestId == id);
        if (entity == null)
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Weekly work request not found");

        entity.WeekStartDate = request.WeekStartDate;
        entity.OverallGoals = request.OverallGoals;
        entity.KeyTasks = JsonSerializer.Serialize(request.KeyTasks ?? new List<string>());
        if (request.ResourceForecast != null)
        {
            entity.PersonnelForecast = request.ResourceForecast.Personnel;
            entity.MajorEquipment = request.ResourceForecast.MajorEquipment;
            entity.CriticalMaterials = request.ResourceForecast.CriticalMaterials;
        }
        else
        {
            entity.PersonnelForecast = request.PersonnelForecast;
            entity.MajorEquipment = request.MajorEquipment;
            entity.CriticalMaterials = request.CriticalMaterials;
        }
        if (request.EstimatedHours.HasValue) entity.EstimatedHours = request.EstimatedHours.Value;
        if (request.Priority != null) entity.Priority = request.Priority;
        if (request.Type != null) entity.Type = request.Type;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await ReloadAndMapAsync(id, "Weekly work request updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteWeeklyWorkRequestAsync(Guid id)
    {
        var entity = await _context.WeeklyWorkRequests.FirstOrDefaultAsync(r => r.WeeklyRequestId == id);
        if (entity == null)
            return ServiceResult<bool>.ErrorResult("Weekly work request not found");

        _context.WeeklyWorkRequests.Remove(entity);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true, "Weekly work request deleted successfully");
    }

    public async Task<ServiceResult<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestStatusAsync(Guid id, WeeklyRequestStatus status)
    {
        var entity = await _context.WeeklyWorkRequests.FirstOrDefaultAsync(r => r.WeeklyRequestId == id);
        if (entity == null)
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Weekly work request not found");

        entity.Status = status;
        if (status == WeeklyRequestStatus.Approved)
            entity.ApprovedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await ReloadAndMapAsync(id, $"Weekly work request status set to {status}");
    }

    private IQueryable<WeeklyWorkRequest> BuildBaseQuery()
        => _context.WeeklyWorkRequests
            .Include(r => r.Project)
            .Include(r => r.RequestedByUser)
            .Include(r => r.ApprovedByUser)
            .AsQueryable();

    private async Task<ServiceResult<WeeklyWorkRequestDto>> ReloadAndMapAsync(Guid id, string message)
    {
        var entity = await BuildBaseQuery().FirstOrDefaultAsync(r => r.WeeklyRequestId == id);
        if (entity == null)
            return ServiceResult<WeeklyWorkRequestDto>.ErrorResult("Weekly work request not found");
        return ServiceResult<WeeklyWorkRequestDto>.SuccessResult(_mapper.Map<WeeklyWorkRequestDto>(entity), message);
    }
}
