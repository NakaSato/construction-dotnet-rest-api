using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace dotnet_rest_api.Services.MasterPlans;

/// <summary>
/// Service for managing progress reports and reporting within master plans
/// Extracted from the original large MasterPlanService for better maintainability
/// </summary>
public interface IMasterPlanReportingService
{
    Task<Result<ProgressReportDto>> CreateProgressReportAsync(Guid masterPlanId, CreateProgressReportRequest request, Guid createdById);
    Task<Result<List<ProgressReportDto>>> GetProgressReportsAsync(Guid masterPlanId, int pageNumber, int pageSize);
    Task<Result<ProgressReportDto>> GetProgressReportByIdAsync(Guid reportId);
    Task<Result<bool>> UpdateProgressReportAsync(Guid reportId, UpdateProgressReportRequest request);
    Task<Result<bool>> DeleteProgressReportAsync(Guid reportId);
}

public class MasterPlanReportingService : IMasterPlanReportingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MasterPlanReportingService> _logger;

    public MasterPlanReportingService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<MasterPlanReportingService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProgressReportDto>> CreateProgressReportAsync(Guid masterPlanId, CreateProgressReportRequest request, Guid createdById)
    {
        try
        {
            // Validate master plan exists
            var masterPlanExists = await _context.MasterPlans.AnyAsync(mp => mp.MasterPlanId == masterPlanId);
            if (!masterPlanExists)
                return Result<ProgressReportDto>.Failure($"Master plan with ID {masterPlanId} not found");

            var report = CreateProgressReportEntity(masterPlanId, request, createdById);

            _context.ProgressReports.Add(report);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProgressReportDto>(report);
            return Result<ProgressReportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating progress report for master plan {MasterPlanId}", masterPlanId);
            return Result<ProgressReportDto>.Failure($"Error creating progress report: {ex.Message}");
        }
    }

    public async Task<Result<List<ProgressReportDto>>> GetProgressReportsAsync(Guid masterPlanId, int pageNumber, int pageSize)
    {
        try
        {
            // Validate master plan exists
            var masterPlanExists = await _context.MasterPlans.AnyAsync(mp => mp.MasterPlanId == masterPlanId);
            if (!masterPlanExists)
                return Result<List<ProgressReportDto>>.Failure($"Master plan with ID {masterPlanId} not found");

            var reports = await _context.ProgressReports
                .Where(pr => pr.MasterPlanId == masterPlanId)
                .OrderByDescending(pr => pr.ReportDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(pr => pr.CreatedBy)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProgressReportDto>>(reports);
            return Result<List<ProgressReportDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress reports for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProgressReportDto>>.Failure($"Error retrieving progress reports: {ex.Message}");
        }
    }

    public async Task<Result<ProgressReportDto>> GetProgressReportByIdAsync(Guid reportId)
    {
        try
        {
            var report = await _context.ProgressReports
                .Include(pr => pr.CreatedBy)
                .Include(pr => pr.MasterPlan)
                .FirstOrDefaultAsync(pr => pr.ProgressReportId == reportId);

            if (report == null)
                return Result<ProgressReportDto>.Failure($"Progress report with ID {reportId} not found");

            var dto = _mapper.Map<ProgressReportDto>(report);
            return Result<ProgressReportDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress report {ReportId}", reportId);
            return Result<ProgressReportDto>.Failure($"Error retrieving progress report: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateProgressReportAsync(Guid reportId, UpdateProgressReportRequest request)
    {
        try
        {
            var report = await _context.ProgressReports.FindAsync(reportId);
            if (report == null)
                return Result<bool>.Failure($"Progress report with ID {reportId} not found");

            UpdateProgressReportEntity(report, request);

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress report {ReportId}", reportId);
            return Result<bool>.Failure($"Error updating progress report: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteProgressReportAsync(Guid reportId)
    {
        try
        {
            var report = await _context.ProgressReports.FindAsync(reportId);
            if (report == null)
                return Result<bool>.Failure($"Progress report with ID {reportId} not found");

            _context.ProgressReports.Remove(report);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting progress report {ReportId}", reportId);
            return Result<bool>.Failure($"Error deleting progress report: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private static ProgressReport CreateProgressReportEntity(Guid masterPlanId, CreateProgressReportRequest request, Guid createdById)
    {
        return new ProgressReport
        {
            ProgressReportId = Guid.NewGuid(),
            MasterPlanId = masterPlanId,
            ReportTitle = request.ReportTitle,
            ReportContent = request.ReportContent,
            ReportDate = request.ReportDate ?? DateTime.UtcNow,
            CompletionPercentage = request.CompletionPercentage,
            KeyAccomplishments = request.KeyAccomplishments,
            ChallengesFaced = request.ChallengesFaced,
            NextSteps = request.UpcomingActivities,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateProgressReportEntity(ProgressReport report, UpdateProgressReportRequest request)
    {
        if (!string.IsNullOrEmpty(request.ReportTitle))
            report.ReportTitle = request.ReportTitle;

        if (!string.IsNullOrEmpty(request.ReportContent))
            report.ReportContent = request.ReportContent;

        if (request.ReportDate.HasValue)
            report.ReportDate = request.ReportDate.Value;

        if (request.CompletionPercentage.HasValue)
            report.CompletionPercentage = request.CompletionPercentage.Value;

        if (!string.IsNullOrEmpty(request.KeyAccomplishments))
            report.KeyAccomplishments = request.KeyAccomplishments;

        if (!string.IsNullOrEmpty(request.ChallengesFaced))
            report.ChallengesFaced = request.ChallengesFaced;

        if (!string.IsNullOrEmpty(request.NextSteps))
            report.NextSteps = request.NextSteps;

        report.UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}
