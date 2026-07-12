using System.Text;
using System.Text.Json;
using AutoMapper;
using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Real EF Core-backed implementation of <see cref="IDailyReportService"/>.
/// Replaces the former StubDailyReportService. Covers CRUD, approval workflow,
/// work-progress items, attachments, analytics, insights, weekly summaries,
/// validation, bulk operations and export.
/// </summary>
public class DailyReportService : IDailyReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DailyReportService> _logger;

    public DailyReportService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<DailyReportService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // ---------------------------------------------------------------- CRUD --

    public async Task<Result<EnhancedPagedResult<DailyReportDto>>> GetDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        var query = BuildBaseQuery();

        if (parameters.ProjectId.HasValue)
            query = query.Where(r => r.ProjectId == parameters.ProjectId.Value);
        if (parameters.ReporterId.HasValue)
            query = query.Where(r => r.ReporterId == parameters.ReporterId.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            Enum.TryParse<DailyReportStatus>(parameters.Status, true, out var status))
            query = query.Where(r => r.Status == status);
        if (parameters.ReportDateAfter.HasValue)
            query = query.Where(r => r.ReportDate >= parameters.ReportDateAfter.Value);
        if (parameters.ReportDateBefore.HasValue)
            query = query.Where(r => r.ReportDate <= parameters.ReportDateBefore.Value);
        if (!string.IsNullOrWhiteSpace(parameters.WeatherCondition) &&
            Enum.TryParse<WeatherCondition>(parameters.WeatherCondition, true, out var weather))
            query = query.Where(r => r.WeatherCondition == weather);
        if (parameters.CreatedAfter.HasValue)
            query = query.Where(r => r.CreatedAt >= parameters.CreatedAfter.Value);
        if (parameters.CreatedBefore.HasValue)
            query = query.Where(r => r.CreatedAt <= parameters.CreatedBefore.Value);
        if (parameters.UpdatedAfter.HasValue)
            query = query.Where(r => r.UpdatedAt >= parameters.UpdatedAfter.Value);
        if (parameters.UpdatedBefore.HasValue)
            query = query.Where(r => r.UpdatedAt <= parameters.UpdatedBefore.Value);
        if (parameters.HasWorkProgress == true)
            query = query.Where(r => r.WorkProgressItems.Any());
        if (parameters.HasIssues == true)
            query = query.Where(r => r.Issues != null && r.Issues != string.Empty);

        query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<DailyReportDto>
        {
            Items = await ToDtosAsync(items),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };

        return Result<EnhancedPagedResult<DailyReportDto>>.SuccessResult(result, "Daily reports retrieved successfully");
    }

    public async Task<Result<DailyReportDto>> GetDailyReportByIdAsync(Guid id)
    {
        var report = await BuildBaseQuery().FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");

        return Result<DailyReportDto>.SuccessResult(await ToDtoAsync(report), "Daily report retrieved successfully");
    }

    public async Task<Result<DailyReportDto>> CreateDailyReportAsync(CreateDailyReportRequest request, Guid userId)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId);
        if (!projectExists)
            return Result<DailyReportDto>.ErrorResult("Project not found");

        var report = _mapper.Map<DailyReport>(request);
        report.DailyReportId = Guid.NewGuid();
        report.ReporterId = userId;
        report.Status = DailyReportStatus.Draft;
        report.CreatedAt = DateTime.UtcNow;

        _context.DailyReports.Add(report);
        await _context.SaveChangesAsync();

        var created = await BuildBaseQuery().FirstOrDefaultAsync(r => r.DailyReportId == report.DailyReportId);
        if (created == null)
            return Result<DailyReportDto>.ErrorResult("Failed to load created daily report");
        return Result<DailyReportDto>.SuccessResult(await ToDtoAsync(created), "Daily report created successfully");
    }

    public async Task<Result<DailyReportDto>> UpdateDailyReportAsync(Guid id, UpdateDailyReportRequest request, Guid userId, string? userRole = null)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");

        // Map non-null request fields onto the tracked entity.
        _mapper.Map(request, report);
        report.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updated = await BuildBaseQuery().FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (updated == null)
            return Result<DailyReportDto>.ErrorResult("Failed to load updated daily report");
        return Result<DailyReportDto>.SuccessResult(await ToDtoAsync(updated), "Daily report updated successfully");
    }

    public async Task<Result<bool>> DeleteDailyReportAsync(Guid id)
    {
        var report = await _context.DailyReports
            .Include(r => r.WorkProgressItems)
            .Include(r => r.PersonnelLogs)
            .Include(r => r.MaterialUsages)
            .Include(r => r.EquipmentLogs)
            .FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<bool>.ErrorResult("Daily report not found");

        var attachments = await _context.DailyReportAttachments.Where(a => a.DailyReportId == id).ToListAsync();
        _context.DailyReportAttachments.RemoveRange(attachments);
        _context.WorkProgressItems.RemoveRange(report.WorkProgressItems);
        _context.PersonnelLogs.RemoveRange(report.PersonnelLogs);
        _context.MaterialUsages.RemoveRange(report.MaterialUsages);
        _context.EquipmentLogs.RemoveRange(report.EquipmentLogs);
        _context.DailyReports.Remove(report);
        await _context.SaveChangesAsync();

        return Result<bool>.SuccessResult(true, "Daily report deleted successfully");
    }

    // ---------------------------------------------------------- Attachments --

    public async Task<Result<DailyReportAttachmentDto>> AddAttachmentAsync(Guid id, IFormFile file, Guid userId)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportAttachmentDto>.ErrorResult("Daily report not found");

        if (file == null || file.Length == 0)
            return Result<DailyReportAttachmentDto>.ErrorResult("No file provided");

        var relativeDir = Path.Combine("uploads", "daily-reports", id.ToString());
        var absoluteDir = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);
        Directory.CreateDirectory(absoluteDir);

        var safeName = Path.GetFileName(file.FileName);
        var storedName = $"{Guid.NewGuid()}_{safeName}";
        var absolutePath = Path.Combine(absoluteDir, storedName);

        await using (var stream = new FileStream(absolutePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new DailyReportAttachment
        {
            Id = Guid.NewGuid(),
            FileName = safeName,
            // Static files map /files -> uploads/, so expose via the public /files path.
            FilePath = $"/files/daily-reports/{id}/{storedName}",
            FileType = file.ContentType ?? "application/octet-stream",
            FileSize = file.Length,
            DailyReportId = id,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId
        };

        _context.DailyReportAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var creatorName = await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync() ?? string.Empty;

        var dto = new DailyReportAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            FilePath = attachment.FilePath,
            FileType = attachment.FileType,
            FileSize = attachment.FileSize,
            DailyReportId = attachment.DailyReportId,
            CreatedAt = attachment.CreatedAt,
            CreatedById = attachment.CreatedById,
            CreatedByName = creatorName
        };

        return Result<DailyReportAttachmentDto>.SuccessResult(dto, "Attachment added successfully");
    }

    // ------------------------------------------------------------- Workflow --

    public async Task<Result<DailyReportDto>> SubmitDailyReportAsync(Guid id)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");

        if (report.Status != DailyReportStatus.Draft && report.Status != DailyReportStatus.RevisionRequested)
            return Result<DailyReportDto>.ErrorResult($"Cannot submit a report with status '{report.Status}'");

        report.Status = DailyReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;
        report.SubmittedByUserId = report.ReporterId;
        report.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await ReloadAndMapAsync(id, "Daily report submitted successfully");
    }

    public async Task<Result<DailyReportDto>> ApproveDailyReportAsync(Guid id, Guid userId)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");

        if (report.Status != DailyReportStatus.Submitted)
            return Result<DailyReportDto>.ErrorResult($"Cannot approve a report with status '{report.Status}'");

        report.Status = DailyReportStatus.Approved;
        report.ApprovedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await ReloadAndMapAsync(id, "Daily report approved successfully");
    }

    public async Task<Result<DailyReportDto>> RejectDailyReportAsync(Guid id, Guid userId, string? rejectionReason = null)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");

        if (report.Status != DailyReportStatus.Submitted)
            return Result<DailyReportDto>.ErrorResult($"Cannot reject a report with status '{report.Status}'");

        report.Status = DailyReportStatus.Rejected;
        report.RejectionReason = rejectionReason ?? "No reason provided";
        report.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await ReloadAndMapAsync(id, "Daily report rejected successfully");
    }

    // ------------------------------------------------ Work-progress items --

    public async Task<Result<WorkProgressItemDto>> AddWorkProgressItemAsync(Guid reportId, CreateWorkProgressItemRequest request)
    {
        var reportExists = await _context.DailyReports.AnyAsync(r => r.DailyReportId == reportId);
        if (!reportExists)
            return Result<WorkProgressItemDto>.ErrorResult("Daily report not found");

        var item = _mapper.Map<WorkProgressItem>(request);
        item.WorkProgressItemId = Guid.NewGuid();
        item.DailyReportId = reportId;
        item.CreatedAt = DateTime.UtcNow;

        _context.WorkProgressItems.Add(item);
        await _context.SaveChangesAsync();

        var created = await _context.WorkProgressItems
            .Include(w => w.Task)
            .FirstOrDefaultAsync(w => w.WorkProgressItemId == item.WorkProgressItemId) ?? item;

        return Result<WorkProgressItemDto>.SuccessResult(_mapper.Map<WorkProgressItemDto>(created), "Work progress item added successfully");
    }

    public async Task<Result<WorkProgressItemDto>> UpdateWorkProgressItemAsync(Guid itemId, UpdateWorkProgressItemRequest request)
    {
        var item = await _context.WorkProgressItems.FirstOrDefaultAsync(w => w.WorkProgressItemId == itemId);
        if (item == null)
            return Result<WorkProgressItemDto>.ErrorResult("Work progress item not found");

        if (request.Activity != null) item.Activity = request.Activity;
        if (request.Description != null) item.Description = request.Description;
        if (request.HoursWorked.HasValue) item.HoursWorked = request.HoursWorked.Value;
        if (request.PercentageComplete.HasValue) item.PercentageComplete = request.PercentageComplete.Value;
        if (request.WorkersAssigned.HasValue) item.WorkersAssigned = request.WorkersAssigned.Value;
        if (request.Notes != null) item.Notes = request.Notes;
        if (request.Issues != null) item.Issues = request.Issues;
        if (request.NextSteps != null) item.NextSteps = request.NextSteps;

        await _context.SaveChangesAsync();

        var updated = await _context.WorkProgressItems
            .Include(w => w.Task)
            .FirstOrDefaultAsync(w => w.WorkProgressItemId == itemId) ?? item;

        return Result<WorkProgressItemDto>.SuccessResult(_mapper.Map<WorkProgressItemDto>(updated), "Work progress item updated successfully");
    }

    public async Task<Result<bool>> DeleteWorkProgressItemAsync(Guid itemId)
    {
        var item = await _context.WorkProgressItems.FirstOrDefaultAsync(w => w.WorkProgressItemId == itemId);
        if (item == null)
            return Result<bool>.ErrorResult("Work progress item not found");

        _context.WorkProgressItems.Remove(item);
        await _context.SaveChangesAsync();

        return Result<bool>.SuccessResult(true, "Work progress item deleted successfully");
    }

    // ----------------------------------------------- Enhanced / project ops --

    public async Task<Result<EnhancedPagedResult<EnhancedDailyReportDto>>> GetProjectDailyReportsAsync(Guid projectId, EnhancedDailyReportQueryParameters parameters)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectManager)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project == null)
            return Result<EnhancedPagedResult<EnhancedDailyReportDto>>.ErrorResult("Project not found");

        var query = BuildBaseQuery().Where(r => r.ProjectId == projectId);

        if (parameters.StartDate.HasValue)
            query = query.Where(r => r.ReportDate >= parameters.StartDate.Value);
        if (parameters.EndDate.HasValue)
            query = query.Where(r => r.ReportDate <= parameters.EndDate.Value);
        if (parameters.ExactDate.HasValue)
            query = query.Where(r => r.ReportDate.Date == parameters.ExactDate.Value.Date);
        if (parameters.ReporterIds.Any())
            query = query.Where(r => parameters.ReporterIds.Contains(r.ReporterId));
        if (parameters.ApprovalStatuses.Any())
        {
            var statuses = parameters.ApprovalStatuses
                .Select(s => Enum.TryParse<DailyReportStatus>(s, true, out var st) ? (DailyReportStatus?)st : null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();
            if (statuses.Any())
                query = query.Where(r => statuses.Contains(r.Status));
        }
        if (parameters.WeatherConditions.Any())
        {
            var conditions = parameters.WeatherConditions
                .Select(s => Enum.TryParse<WeatherCondition>(s, true, out var wc) ? (WeatherCondition?)wc : null)
                .Where(w => w.HasValue)
                .Select(w => w!.Value)
                .ToList();
            if (conditions.Any())
                query = query.Where(r => r.WeatherCondition != null && conditions.Contains(r.WeatherCondition.Value));
        }
        if (parameters.MinWorkHours.HasValue)
            query = query.Where(r => r.TotalWorkHours >= parameters.MinWorkHours.Value);
        if (parameters.MaxWorkHours.HasValue)
            query = query.Where(r => r.TotalWorkHours <= parameters.MaxWorkHours.Value);

        query = ApplySorting(query, parameters.SortBy, parameters.IsSortDescending ? "desc" : "asc");

        var totalCount = await query.CountAsync();
        var reports = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var reportIds = reports.Select(r => r.DailyReportId).ToList();
        var attachments = await _context.DailyReportAttachments
            .Where(a => reportIds.Contains(a.DailyReportId))
            .ToListAsync();

        var items = new List<EnhancedDailyReportDto>();
        foreach (var report in reports)
        {
            var dto = _mapper.Map<EnhancedDailyReportDto>(report);
            dto.ProjectName = project.ProjectName;
            dto.ProjectCode = project.ProjectId.ToString();
            dto.ProjectLocation = project.Address;
            dto.ProjectManagerName = project.ProjectManager?.FullName ?? string.Empty;
            dto.ClientName = project.ClientInfo;
            dto.ApprovalStatus = report.Status.ToString();
            dto.ApprovalDate = report.ApprovedAt;

            var reportAttachments = attachments.Where(a => a.DailyReportId == report.DailyReportId).ToList();
            dto.Attachments = reportAttachments.Select(MapAttachment).ToList();
            dto.PhotoCount = reportAttachments.Count(a => a.FileType.StartsWith("image", StringComparison.OrdinalIgnoreCase));
            dto.DocumentCount = reportAttachments.Count - dto.PhotoCount;
            dto.HasCriticalIssues = !string.IsNullOrEmpty(report.Issues) || !string.IsNullOrEmpty(report.SafetyIncidents);
            dto.LastModifiedAt = report.UpdatedAt;

            items.Add(dto);
        }

        // Resolve reporter names without an inner join (reporter may be orphaned).
        await FillNamesAsync(items);

        var result = new EnhancedPagedResult<EnhancedDailyReportDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.IsSortDescending ? "desc" : "asc"
        };

        return Result<EnhancedPagedResult<EnhancedDailyReportDto>>.SuccessResult(result, "Project daily reports retrieved successfully");
    }

    public async Task<Result<bool>> ValidateProjectAccessAsync(Guid projectId, Guid userId)
    {
        var exists = await _context.Projects.AnyAsync(p => p.ProjectId == projectId);
        return exists
            ? Result<bool>.SuccessResult(true, "Access validated")
            : Result<bool>.ErrorResult("Project not found");
    }

    public async Task<Result<EnhancedDailyReportDto>> CreateEnhancedDailyReportAsync(EnhancedCreateDailyReportRequest request, Guid userId)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectManager)
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);
        if (project == null)
            return Result<EnhancedDailyReportDto>.ErrorResult("Project not found");

        var report = new DailyReport
        {
            DailyReportId = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ReportDate = request.ReportDate,
            Status = DailyReportStatus.Draft,
            ReporterId = userId,
            CreatedAt = DateTime.UtcNow,
            Summary = request.WorkSummary,
            WorkAccomplished = request.WorkAccomplished ?? string.Empty,
            WorkPlanned = request.WorkPlannedNextDay ?? string.Empty,
            Issues = request.IssuesEncountered ?? string.Empty,
            GeneralNotes = request.AdditionalNotes ?? string.Empty,
            TotalWorkHours = (int)Math.Round(request.TotalWorkHours),
            PersonnelOnSite = request.PersonnelOnSite,
            SafetyIncidents = request.SafetyIncidents ?? string.Empty,
            QualityIssues = request.QualityIssues ?? string.Empty,
            TemperatureHigh = request.Temperature,
            Humidity = request.Humidity,
            WindSpeed = request.WindSpeed
        };

        if (!string.IsNullOrWhiteSpace(request.WeatherCondition) &&
            Enum.TryParse<WeatherCondition>(request.WeatherCondition, true, out var weather))
            report.WeatherCondition = weather;

        foreach (var wp in request.WorkProgressItems)
        {
            var item = _mapper.Map<WorkProgressItem>(wp);
            item.WorkProgressItemId = Guid.NewGuid();
            item.DailyReportId = report.DailyReportId;
            item.CreatedAt = DateTime.UtcNow;
            report.WorkProgressItems.Add(item);
        }

        foreach (var pl in request.PersonnelLogs)
        {
            report.PersonnelLogs.Add(new PersonnelLog
            {
                PersonnelLogId = Guid.NewGuid(),
                DailyReportId = report.DailyReportId,
                UserId = pl.UserId,
                HoursWorked = pl.HoursWorked,
                Position = pl.Role ?? string.Empty,
                Notes = pl.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var mu in request.MaterialUsages)
        {
            report.MaterialUsages.Add(new MaterialUsage
            {
                MaterialUsageId = Guid.NewGuid(),
                DailyReportId = report.DailyReportId,
                MaterialName = mu.MaterialId.ToString(),
                QuantityUsed = mu.Quantity,
                Unit = mu.Unit,
                Cost = mu.UnitCost,
                Notes = mu.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var eq in request.EquipmentLogs)
        {
            report.EquipmentLogs.Add(new EquipmentLog
            {
                EquipmentLogId = Guid.NewGuid(),
                DailyReportId = report.DailyReportId,
                EquipmentName = eq.EquipmentId.ToString(),
                HoursUsed = eq.HoursUsed,
                MaintenanceRequired = !string.IsNullOrWhiteSpace(eq.MaintenanceNotes),
                MaintenanceNotes = eq.MaintenanceNotes ?? string.Empty,
                Issues = eq.Issues ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.DailyReports.Add(report);
        await _context.SaveChangesAsync();

        var created = await BuildBaseQuery().FirstOrDefaultAsync(r => r.DailyReportId == report.DailyReportId) ?? report;
        var dto = _mapper.Map<EnhancedDailyReportDto>(created);
        dto.ProjectName = project.ProjectName;
        dto.ProjectCode = project.ProjectId.ToString();
        dto.ProjectLocation = project.Address;
        dto.ProjectManagerName = project.ProjectManager?.FullName ?? string.Empty;
        dto.ClientName = project.ClientInfo;
        dto.SafetyScore = request.SafetyScore;
        dto.QualityScore = request.QualityScore;
        dto.DailyProgressContribution = request.DailyProgressContribution;
        dto.ApprovalStatus = created.Status.ToString();

        return Result<EnhancedDailyReportDto>.SuccessResult(dto, "Enhanced daily report created successfully");
    }

    // ------------------------------------------------------------ Analytics --

    public async Task<Result<DailyReportAnalyticsDto>> GetDailyReportAnalyticsAsync(Guid projectId, DateTime startDate, DateTime endDate)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project == null)
            return Result<DailyReportAnalyticsDto>.ErrorResult("Project not found");

        var reports = await _context.DailyReports
            .Where(r => r.ProjectId == projectId && r.ReportDate >= startDate && r.ReportDate <= endDate)
            .ToListAsync();

        var analytics = new DailyReportAnalyticsDto
        {
            ProjectId = projectId,
            ProjectName = project.ProjectName,
            AnalysisPeriodStart = startDate,
            AnalysisPeriodEnd = endDate,
            TotalReports = reports.Count,
            TotalHoursLogged = reports.Sum(r => r.TotalWorkHours),
            AverageHoursPerDay = reports.Count > 0 ? reports.Average(r => r.TotalWorkHours) : 0,
            AverageTeamSize = reports.Count > 0 ? (int)Math.Round(reports.Average(r => r.PersonnelOnSite)) : 0,
            TotalCriticalIssues = reports.Count(r => !string.IsNullOrEmpty(r.Issues) || !string.IsNullOrEmpty(r.SafetyIncidents)),
            WeatherDelayDays = reports.Count(r => r.WeatherCondition == WeatherCondition.Rainy || r.WeatherCondition == WeatherCondition.Stormy)
        };

        analytics.WeatherConditionDays = reports
            .Where(r => r.WeatherCondition != null)
            .GroupBy(r => r.WeatherCondition!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        analytics.ProgressTrend = reports
            .OrderBy(r => r.ReportDate)
            .GroupBy(r => r.ReportDate.Date)
            .Select(g => new DailyTrendDto
            {
                Date = g.Key,
                Value = g.Sum(r => r.TotalWorkHours),
                Category = "WorkHours"
            })
            .ToList();

        // Safety/Quality scores are not persisted on DailyReport, so those trend
        // series and averages are left at their defaults (empty / 0).
        return Result<DailyReportAnalyticsDto>.SuccessResult(analytics, "Analytics generated successfully");
    }

    public async Task<Result<WeeklySummaryDto>> GetWeeklySummaryAsync(Guid projectId, DateTime weekStartDate)
        => await BuildWeeklySummaryAsync(projectId, weekStartDate);

    public async Task<Result<WeeklySummaryDto>> GetWeeklyProgressReportAsync(Guid projectId, DateTime weekStart)
        => await BuildWeeklySummaryAsync(projectId, weekStart);

    private async Task<Result<WeeklySummaryDto>> BuildWeeklySummaryAsync(Guid projectId, DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(7);
        var query = BuildBaseQuery().Where(r => r.ReportDate >= weekStart && r.ReportDate < weekEnd);
        if (projectId != Guid.Empty)
            query = query.Where(r => r.ProjectId == projectId);

        var reports = await query.ToListAsync();

        var summary = new WeeklySummaryDto
        {
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            TotalReports = reports.Count,
            TotalHoursWorked = reports.Sum(r => r.TotalWorkHours),
            CompletedTasks = reports.SelectMany(r => r.WorkProgressItems).Count(w => w.PercentageComplete >= 100),
            PendingTasks = reports.SelectMany(r => r.WorkProgressItems).Count(w => w.PercentageComplete < 100),
            AverageProgress = reports.SelectMany(r => r.WorkProgressItems).Any()
                ? (decimal)reports.SelectMany(r => r.WorkProgressItems).Average(w => w.PercentageComplete)
                : 0,
            TopIssues = reports
                .Where(r => !string.IsNullOrEmpty(r.Issues))
                .Select(r => r.Issues!)
                .Distinct()
                .Take(10)
                .ToList(),
            Reports = await ToDtosAsync(reports)
        };

        return Result<WeeklySummaryDto>.SuccessResult(summary, "Weekly summary generated successfully");
    }

    // ------------------------------------------------------------- Insights --

    public async Task<Result<DailyReportInsightsDto>> GetDailyReportInsightsAsync(Guid projectId, Guid? reportId = null)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);
        if (project == null)
            return Result<DailyReportInsightsDto>.ErrorResult("Project not found");

        var reports = await _context.DailyReports
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

        var insights = new DailyReportInsightsDto
        {
            ProjectId = projectId,
            ProjectName = project.ProjectName,
            GeneratedAt = DateTime.UtcNow
        };

        var criticalCount = reports.Count(r => !string.IsNullOrEmpty(r.Issues) || !string.IsNullOrEmpty(r.SafetyIncidents));
        var avgHours = reports.Count > 0 ? reports.Average(r => r.TotalWorkHours) : 0;
        var safetyIncidentCount = reports.Count(r => !string.IsNullOrEmpty(r.SafetyIncidents));

        insights.PerformanceInsights.Add($"{reports.Count} daily reports logged with an average of {avgHours:F1} work hours per report.");
        if (avgHours < 4 && reports.Count > 0)
            insights.ProductivityRecommendations.Add("Average logged hours are low; review whether work hours are being fully captured.");

        if (safetyIncidentCount > 0)
            insights.SafetyRecommendations.Add($"{safetyIncidentCount} report(s) recorded safety incidents; schedule a safety review.");

        // Heuristic risk level based on the share of reports flagging issues.
        var issueRatio = reports.Count > 0 ? (double)criticalCount / reports.Count : 0;
        insights.RiskLevel = issueRatio switch
        {
            >= 0.5 => "Critical",
            >= 0.25 => "High",
            >= 0.1 => "Medium",
            _ => "Low"
        };
        if (criticalCount > 0)
            insights.IdentifiedRisks.Add($"{criticalCount} report(s) flagged issues or safety incidents.");

        insights.IsOnTrack = issueRatio < 0.25;

        return Result<DailyReportInsightsDto>.SuccessResult(insights, "Insights generated successfully");
    }

    // ----------------------------------------------------------- Validation --

    public Task<Result<DailyReportValidationResultDto>> ValidateDailyReportAsync(EnhancedCreateDailyReportRequest request)
    {
        var result = new DailyReportValidationResultDto { IsValid = true };

        void Rule(string name, bool passed, string severity, string message, string? suggestion = null)
        {
            result.RuleResults.Add(new ValidationRuleResultDto
            {
                RuleName = name,
                Severity = severity,
                Passed = passed,
                Message = message,
                Suggestion = suggestion
            });
            if (!passed)
            {
                if (severity == "Error") { result.Errors.Add(message); result.IsValid = false; }
                else if (severity == "Warning") result.Warnings.Add(message);
            }
        }

        Rule("WorkSummaryRequired",
            !string.IsNullOrWhiteSpace(request.WorkSummary) && request.WorkSummary.Trim().Length >= 10,
            "Error", "Work summary must be at least 10 characters.", "Describe the day's work in more detail.");

        Rule("TotalWorkHoursRange",
            request.TotalWorkHours is > 0 and <= 24,
            "Error", "Total work hours must be between 0 and 24.");

        Rule("PersonnelOnSiteRange",
            request.PersonnelOnSite >= 1,
            "Error", "At least one person must be on site.");

        Rule("SafetyScoreRange",
            request.SafetyScore is >= 1 and <= 10,
            "Warning", "Safety score should be between 1 and 10.");

        Rule("QualityScoreRange",
            request.QualityScore is >= 1 and <= 10,
            "Warning", "Quality score should be between 1 and 10.");

        if (!string.IsNullOrEmpty(request.SafetyIncidents))
            result.Suggestions.Add("Safety incidents were reported; consider attaching supporting photos.");

        return System.Threading.Tasks.Task.FromResult(Result<DailyReportValidationResultDto>.SuccessResult(result, "Validation completed"));
    }

    // ------------------------------------------------------------ Templates --

    public Task<Result<List<DailyReportTemplateDto>>> GetDailyReportTemplatesAsync(Guid projectId)
    {
        // No template table backs this feature yet; return an empty set rather than error.
        var templates = new List<DailyReportTemplateDto>();
        return System.Threading.Tasks.Task.FromResult(Result<List<DailyReportTemplateDto>>.SuccessResult(templates, "No templates configured"));
    }

    // --------------------------------------------------------- Bulk actions --

    public async Task<Result<BulkOperationResultDto>> BulkApproveDailyReportsAsync(DailyReportBulkApprovalRequest request, Guid userId)
    {
        var result = new BulkOperationResultDto { TotalRequested = request.ReportIds.Count };

        foreach (var id in request.ReportIds)
        {
            var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
            if (report == null)
            {
                result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = false, ErrorMessage = "Report not found" });
                continue;
            }
            if (report.Status != DailyReportStatus.Submitted)
            {
                result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = false, ErrorMessage = $"Invalid status '{report.Status}'" });
                continue;
            }

            report.Status = DailyReportStatus.Approved;
            report.ApprovedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;
            result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = true });
        }

        await _context.SaveChangesAsync();

        result.SuccessCount = result.Results.Count(r => r.Success);
        result.FailureCount = result.Results.Count(r => !r.Success);
        result.Summary = $"Approved {result.SuccessCount} of {result.TotalRequested} report(s)";

        return Result<BulkOperationResultDto>.SuccessResult(result, result.Summary);
    }

    public async Task<Result<BulkOperationResultDto>> BulkRejectDailyReportsAsync(DailyReportBulkRejectionRequest request, Guid userId)
    {
        var result = new BulkOperationResultDto { TotalRequested = request.ReportIds.Count };

        foreach (var id in request.ReportIds)
        {
            var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == id);
            if (report == null)
            {
                result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = false, ErrorMessage = "Report not found" });
                continue;
            }
            if (report.Status != DailyReportStatus.Submitted)
            {
                result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = false, ErrorMessage = $"Invalid status '{report.Status}'" });
                continue;
            }

            report.Status = DailyReportStatus.Rejected;
            report.RejectionReason = request.RejectionReason;
            report.UpdatedAt = DateTime.UtcNow;
            result.Results.Add(new BulkOperationItemResultDto { ItemId = id, Success = true });
        }

        await _context.SaveChangesAsync();

        result.SuccessCount = result.Results.Count(r => r.Success);
        result.FailureCount = result.Results.Count(r => !r.Success);
        result.Summary = $"Rejected {result.SuccessCount} of {result.TotalRequested} report(s)";

        return Result<BulkOperationResultDto>.SuccessResult(result, result.Summary);
    }

    // --------------------------------------------------------------- Export --

    public async Task<Result<byte[]>> ExportDailyReportsAsync(DailyReportQueryParameters parameters)
    {
        var query = BuildBaseQuery();
        if (parameters.ProjectId.HasValue)
            query = query.Where(r => r.ProjectId == parameters.ProjectId.Value);
        if (parameters.ReportDateAfter.HasValue)
            query = query.Where(r => r.ReportDate >= parameters.ReportDateAfter.Value);
        if (parameters.ReportDateBefore.HasValue)
            query = query.Where(r => r.ReportDate <= parameters.ReportDateBefore.Value);

        var reports = await query.OrderBy(r => r.ReportDate).ToListAsync();
        var bytes = BuildCsv(reports);
        return Result<byte[]>.SuccessResult(bytes, "Export generated successfully");
    }

    public async Task<Result<byte[]>> ExportEnhancedDailyReportsAsync(DailyReportExportRequest request)
    {
        var query = BuildBaseQuery().Where(r => r.ProjectId == request.ProjectId);
        if (request.StartDate.HasValue)
            query = query.Where(r => r.ReportDate >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            query = query.Where(r => r.ReportDate <= request.EndDate.Value);

        var reports = await query.OrderBy(r => r.ReportDate).ToListAsync();

        byte[] bytes;
        if (string.Equals(request.Format, "json", StringComparison.OrdinalIgnoreCase))
        {
            var dtos = _mapper.Map<List<DailyReportDto>>(reports);
            bytes = JsonSerializer.SerializeToUtf8Bytes(dtos, new JsonSerializerOptions { WriteIndented = true });
        }
        else
        {
            // csv is the default; excel/pdf fall back to csv content.
            bytes = BuildCsv(reports);
        }

        return Result<byte[]>.SuccessResult(bytes, "Export generated successfully");
    }

    // --------------------------------------------------- Approvals / history --

    public async Task<Result<EnhancedPagedResult<DailyReportDto>>> GetPendingApprovalsAsync(Guid? projectId = null, int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = BuildBaseQuery().Where(r => r.Status == DailyReportStatus.Submitted);
        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);

        query = query.OrderBy(r => r.SubmittedAt);

        var totalCount = await query.CountAsync();
        var reports = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<DailyReportDto>
        {
            Items = await ToDtosAsync(reports),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Result<EnhancedPagedResult<DailyReportDto>>.SuccessResult(result, "Pending approvals retrieved successfully");
    }

    public async Task<Result<List<ApprovalHistoryDto>>> GetApprovalHistoryAsync(Guid reportId)
    {
        var report = await _context.DailyReports.FirstOrDefaultAsync(r => r.DailyReportId == reportId);
        if (report == null)
            return Result<List<ApprovalHistoryDto>>.ErrorResult("Daily report not found");

        // No dedicated history table exists; synthesize entries from the report's
        // workflow timestamps and current status.
        var history = new List<ApprovalHistoryDto>();

        if (report.SubmittedAt.HasValue)
        {
            history.Add(new ApprovalHistoryDto
            {
                Id = Guid.NewGuid(),
                Action = "Submitted",
                ActorId = report.SubmittedByUserId ?? report.ReporterId,
                Timestamp = report.SubmittedAt.Value
            });
        }

        if (report.Status == DailyReportStatus.Approved && report.ApprovedAt.HasValue)
        {
            history.Add(new ApprovalHistoryDto
            {
                Id = Guid.NewGuid(),
                Action = "Approved",
                Timestamp = report.ApprovedAt.Value
            });
        }
        else if (report.Status == DailyReportStatus.Rejected)
        {
            history.Add(new ApprovalHistoryDto
            {
                Id = Guid.NewGuid(),
                Action = "Rejected",
                Timestamp = report.UpdatedAt ?? report.SubmittedAt ?? report.CreatedAt,
                RejectionReason = report.RejectionReason
            });
        }

        return Result<List<ApprovalHistoryDto>>.SuccessResult(
            history.OrderBy(h => h.Timestamp).ToList(),
            "Approval history retrieved successfully");
    }

    // -------------------------------------------------------------- Helpers --

    private IQueryable<DailyReport> BuildBaseQuery()
    {
        // NOTE: We deliberately do NOT Include the required references Project and
        // Reporter. In EF Core 10 a required-reference Include is emitted as an INNER
        // JOIN, which silently drops any report whose ReporterId/ProjectId is orphaned
        // (a report must never vanish from a read). Only collection navigations — which
        // are left-join safe — and the nullable WorkProgressItem.Task are eager-loaded.
        // Project/Reporter display names are backfilled in-memory via FillNamesAsync.
        return _context.DailyReports
            .Include(r => r.WorkProgressItems).ThenInclude(w => w.Task)
            .Include(r => r.PersonnelLogs).ThenInclude(p => p.User)
            .Include(r => r.MaterialUsages)
            .Include(r => r.EquipmentLogs)
            .AsQueryable();
    }

    private async Task<Result<DailyReportDto>> ReloadAndMapAsync(Guid id, string message)
    {
        var report = await BuildBaseQuery().FirstOrDefaultAsync(r => r.DailyReportId == id);
        if (report == null)
            return Result<DailyReportDto>.ErrorResult("Daily report not found");
        return Result<DailyReportDto>.SuccessResult(await ToDtoAsync(report), message);
    }

    /// <summary>Maps a single report and backfills Project/Reporter display names.</summary>
    private async Task<DailyReportDto> ToDtoAsync(DailyReport report)
    {
        var dto = _mapper.Map<DailyReportDto>(report);
        await FillNamesAsync(new[] { dto });
        return dto;
    }

    /// <summary>Maps a list of reports and backfills Project/Reporter display names.</summary>
    private async Task<List<DailyReportDto>> ToDtosAsync(List<DailyReport> reports)
    {
        var dtos = _mapper.Map<List<DailyReportDto>>(reports);
        await FillNamesAsync(dtos);
        return dtos;
    }

    /// <summary>
    /// Populates ProjectName/ReporterName by loading the referenced rows separately,
    /// so a missing (orphaned) Project or Reporter yields an empty name rather than
    /// dropping the report from the result set. Only fills names left empty by mapping.
    /// </summary>
    private async System.Threading.Tasks.Task FillNamesAsync(IReadOnlyList<DailyReportDto> dtos)
    {
        if (dtos.Count == 0)
            return;

        var projectIds = dtos.Select(d => d.ProjectId).Distinct().ToList();
        var reporterIds = dtos.Select(d => d.ReporterId).Distinct().ToList();

        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.ProjectId))
            .Select(p => new { p.ProjectId, p.ProjectName })
            .ToDictionaryAsync(p => p.ProjectId, p => p.ProjectName);

        var users = await _context.Users
            .Where(u => reporterIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.FullName })
            .ToDictionaryAsync(u => u.UserId, u => u.FullName);

        foreach (var dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.ProjectName) && projects.TryGetValue(dto.ProjectId, out var projectName))
                dto.ProjectName = projectName;
            if (string.IsNullOrEmpty(dto.ReporterName) && users.TryGetValue(dto.ReporterId, out var reporterName))
                dto.ReporterName = reporterName;
        }
    }

    private static IQueryable<DailyReport> ApplySorting(IQueryable<DailyReport> query, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "totalworkhours" or "totalhours" => descending ? query.OrderByDescending(r => r.TotalWorkHours) : query.OrderBy(r => r.TotalWorkHours),
            "status" => descending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
            "createdat" => descending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "reportdate" => descending ? query.OrderByDescending(r => r.ReportDate) : query.OrderBy(r => r.ReportDate),
            _ => query.OrderByDescending(r => r.ReportDate)
        };
    }

    private static ReportAttachmentDto MapAttachment(DailyReportAttachment a)
    {
        var isPhoto = a.FileType.StartsWith("image", StringComparison.OrdinalIgnoreCase);
        return new ReportAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FileType = a.FileType,
            FileSize = a.FileSize,
            Category = isPhoto ? "Photo" : "Document",
            UploadedAt = a.CreatedAt,
            UploadedBy = a.CreatedById,
            DownloadUrl = a.FilePath
        };
    }

    private static byte[] BuildCsv(List<DailyReport> reports)
    {
        var sb = new StringBuilder();
        sb.AppendLine("DailyReportId,ProjectId,ReportDate,Status,ReporterId,TotalWorkHours,PersonnelOnSite,WeatherCondition,Summary,Issues,SafetyIncidents,CreatedAt");
        foreach (var r in reports)
        {
            sb.Append(r.DailyReportId).Append(',')
              .Append(r.ProjectId).Append(',')
              .Append(r.ReportDate.ToString("yyyy-MM-dd")).Append(',')
              .Append(r.Status).Append(',')
              .Append(r.ReporterId).Append(',')
              .Append(r.TotalWorkHours).Append(',')
              .Append(r.PersonnelOnSite).Append(',')
              .Append(r.WeatherCondition?.ToString() ?? string.Empty).Append(',')
              .Append(CsvEscape(r.Summary)).Append(',')
              .Append(CsvEscape(r.Issues)).Append(',')
              .Append(CsvEscape(r.SafetyIncidents)).Append(',')
              .Append(r.CreatedAt.ToString("O"))
              .AppendLine();
        }
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }
}
