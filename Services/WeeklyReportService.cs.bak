using Microsoft.EntityFrameworkCore;
using AutoMapper;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Data;
using dotnet_rest_api.Common;
using System.Text.Json;

namespace dotnet_rest_api.Services;

public class WeeklyReportService : IWeeklyReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WeeklyReportService> _logger;
    private readonly IQueryService _queryService;

    public WeeklyReportService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<WeeklyReportService> logger,
        IQueryService queryService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _queryService = queryService;
    }

    public async Task<ApiResponse<WeeklyReportDto>> GetWeeklyReportByIdAsync(Guid reportId)
    {
        try
        {
            var report = await _context.WeeklyReports
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyReportId == reportId);

            if (report == null)
            {
                return ApiResponse<WeeklyReportDto>.ErrorResponse(
                    "Weekly report not found");
            }

            var dto = _mapper.Map<WeeklyReportDto>(report);
            return ApiResponse<WeeklyReportDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weekly report {ReportId}", reportId);
            return ApiResponse<WeeklyReportDto>.ErrorResponse(
                "An error occurred while retrieving the weekly report");
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<WeeklyReportDto>>> GetWeeklyReportsAsync(
        WeeklyReportQueryParameters parameters)
    {
        try
        {
            var query = _context.WeeklyReports
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .Include(w => w.ApprovedByUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status) && 
                Enum.TryParse<WeeklyReportStatus>(parameters.Status, out var status))
            {
                query = query.Where(w => w.Status == status);
            }

            if (parameters.WeekStartAfter.HasValue)
            {
                query = query.Where(w => w.WeekStartDate >= parameters.WeekStartAfter.Value);
            }

            if (parameters.WeekStartBefore.HasValue)
            {
                query = query.Where(w => w.WeekStartDate <= parameters.WeekStartBefore.Value);
            }

            if (parameters.ProjectId.HasValue)
            {
                query = query.Where(w => w.ProjectId == parameters.ProjectId.Value);
            }

            if (parameters.SubmittedById.HasValue)
            {
                query = query.Where(w => w.SubmittedById == parameters.SubmittedById.Value);
            }

            if (parameters.MinActualHours.HasValue)
            {
                query = query.Where(w => w.TotalManHours >= parameters.MinActualHours.Value);
            }

            if (parameters.MaxActualHours.HasValue)
            {
                query = query.Where(w => w.TotalManHours <= parameters.MaxActualHours.Value);
            }

            if (parameters.MinCompletionPercentage.HasValue)
            {
                query = query.Where(w => w.CompletionPercentage >= parameters.MinCompletionPercentage.Value);
            }

            if (parameters.MaxCompletionPercentage.HasValue)
            {
                query = query.Where(w => w.CompletionPercentage <= parameters.MaxCompletionPercentage.Value);
            }

            // Apply generic filters
            query = _queryService.ApplyFilters(query, parameters.Filters);

            // Apply sorting
            query = _queryService.ApplySorting(query, parameters.SortBy, parameters.SortOrder);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var reports = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<WeeklyReportDto>>(reports);

            var result = new EnhancedPagedResult<WeeklyReportDto>
            {
                Items = dtos,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<EnhancedPagedResult<WeeklyReportDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weekly reports");
            return ApiResponse<EnhancedPagedResult<WeeklyReportDto>>.ErrorResponse(
                "An error occurred while retrieving weekly reports");
        }
    }

    public async Task<ApiResponse<WeeklyReportDto>> CreateWeeklyReportAsync(CreateWeeklyReportDto request)
    {
        try
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return ApiResponse<WeeklyReportDto>.ErrorResponse("Project not found");
            }

            // Validate submitted by user exists
            var user = await _context.Users.FindAsync(request.SubmittedById);
            if (user == null)
            {
                return ApiResponse<WeeklyReportDto>.ErrorResponse("Submitted by user not found");
            }

            var weeklyReport = new WeeklyReport
            {
                WeeklyReportId = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                WeekStartDate = request.WeekStartDate,
                SummaryOfProgress = request.SummaryOfProgress,
                TotalManHours = request.TotalManHours,
                PanelsInstalled = request.PanelsInstalled,
                SafetyIncidents = request.SafetyIncidents,
                DelaysReported = request.DelaysReported,
                MajorAccomplishments = JsonSerializer.Serialize(request.MajorAccomplishments ?? new List<string>()),
                MajorIssues = JsonSerializer.Serialize(request.MajorIssues ?? new List<WeeklyIssueDto>()),
                Lookahead = request.Lookahead,
                SubmittedById = request.SubmittedById,
                CompletionPercentage = request.CompletionPercentage,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeeklyReports.Add(weeklyReport);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdReport = await _context.WeeklyReports
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyReportId == weeklyReport.WeeklyReportId);

            var dto = _mapper.Map<WeeklyReportDto>(createdReport);
            return ApiResponse<WeeklyReportDto>.SuccessResponse(dto, "Weekly report created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weekly report");
            return ApiResponse<WeeklyReportDto>.ErrorResponse(
                "An error occurred while creating the weekly report");
        }
    }

    public async Task<ApiResponse<WeeklyReportDto>> UpdateWeeklyReportAsync(
        Guid reportId, UpdateWeeklyReportDto request)
    {
        try
        {
            var weeklyReport = await _context.WeeklyReports
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyReportId == reportId);

            if (weeklyReport == null)
            {
                return ApiResponse<WeeklyReportDto>.ErrorResponse(
                    "Weekly report not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.SummaryOfProgress))
                weeklyReport.SummaryOfProgress = request.SummaryOfProgress;

            if (request.TotalManHours.HasValue)
                weeklyReport.TotalManHours = request.TotalManHours.Value;

            if (request.PanelsInstalled.HasValue)
                weeklyReport.PanelsInstalled = request.PanelsInstalled.Value;

            if (request.SafetyIncidents.HasValue)
                weeklyReport.SafetyIncidents = request.SafetyIncidents.Value;

            if (request.DelaysReported.HasValue)
                weeklyReport.DelaysReported = request.DelaysReported.Value;

            if (request.MajorAccomplishments != null)
                weeklyReport.MajorAccomplishments = JsonSerializer.Serialize(request.MajorAccomplishments);

            if (request.MajorIssues != null)
                weeklyReport.MajorIssues = JsonSerializer.Serialize(request.MajorIssues);

            if (request.Lookahead != null)
                weeklyReport.Lookahead = request.Lookahead;

            if (request.CompletionPercentage.HasValue)
                weeklyReport.CompletionPercentage = request.CompletionPercentage.Value;

            weeklyReport.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<WeeklyReportDto>(weeklyReport);
            return ApiResponse<WeeklyReportDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly report {ReportId}", reportId);
            return ApiResponse<WeeklyReportDto>.ErrorResponse(
                "An error occurred while updating the weekly report");
        }
    }

    public async Task<ApiResponse<WeeklyReportDto>> UpdateWeeklyReportStatusAsync(
        Guid reportId, WeeklyReportStatus status)
    {
        try
        {
            var weeklyReport = await _context.WeeklyReports
                .Include(w => w.Project)
                .Include(w => w.SubmittedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyReportId == reportId);

            if (weeklyReport == null)
            {
                return ApiResponse<WeeklyReportDto>.ErrorResponse(
                    "Weekly report not found");
            }

            weeklyReport.Status = status;
            weeklyReport.UpdatedAt = DateTime.UtcNow;

            // Set approval timestamp when approved
            if (status == WeeklyReportStatus.Approved)
            {
                weeklyReport.ApprovedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<WeeklyReportDto>(weeklyReport);
            return ApiResponse<WeeklyReportDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly report status {ReportId}", reportId);
            return ApiResponse<WeeklyReportDto>.ErrorResponse(
                "An error occurred while updating the weekly report status");
        }
    }

    public async Task<ApiResponse<bool>> DeleteWeeklyReportAsync(Guid reportId)
    {
        try
        {
            var weeklyReport = await _context.WeeklyReports.FindAsync(reportId);
            if (weeklyReport == null)
            {
                return ApiResponse<bool>.ErrorResponse("Weekly report not found");
            }

            _context.WeeklyReports.Remove(weeklyReport);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weekly report {ReportId}", reportId);
            return ApiResponse<bool>.ErrorResponse(
                "An error occurred while deleting the weekly report");
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<WeeklyReportDto>>> GetProjectWeeklyReportsAsync(
        Guid projectId, WeeklyReportQueryParameters parameters)
    {
        // Set the project filter and delegate to the main method
        parameters.ProjectId = projectId;
        return await GetWeeklyReportsAsync(parameters);
    }
}
