using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for master plan analytics, progress calculations, and health monitoring
/// Extracted from the original large MasterPlanService for better maintainability
/// </summary>
public interface IMasterPlanAnalyticsService
{
    Task<Result<decimal>> CalculateOverallProgressAsync(Guid masterPlanId);
    Task<Result<ProjectHealthStatus>> CalculateProjectHealthAsync(Guid masterPlanId);
    Task<Result<ProgressSummaryDto>> GetProgressSummaryAsync(Guid masterPlanId);
    Task<Result<Dictionary<string, object>>> GetProjectMetricsAsync(Guid masterPlanId);
    Task<Result<List<ProjectPhaseDto>>> GetCriticalPathAsync(Guid masterPlanId);
}

public class MasterPlanAnalyticsService : IMasterPlanAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MasterPlanAnalyticsService> _logger;

    public MasterPlanAnalyticsService(
        ApplicationDbContext context,
        ILogger<MasterPlanAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<decimal>> CalculateOverallProgressAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await GetMasterPlanWithPhasesAsync(masterPlanId);
            if (masterPlan == null)
                return Result<decimal>.Failure($"Master plan with ID {masterPlanId} not found");

            var progress = CalculateWeightedProgress(masterPlan.Phases);
            return Result<decimal>.Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating overall progress for master plan {MasterPlanId}", masterPlanId);
            return Result<decimal>.Failure($"Error calculating overall progress: {ex.Message}");
        }
    }

    public async Task<Result<ProjectHealthStatus>> CalculateProjectHealthAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await GetMasterPlanWithPhasesAndMilestonesAsync(masterPlanId);
            if (masterPlan == null)
                return Result<ProjectHealthStatus>.Failure($"Master plan with ID {masterPlanId} not found");

            var health = DetermineProjectHealth(masterPlan);
            return Result<ProjectHealthStatus>.Success(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project health for master plan {MasterPlanId}", masterPlanId);
            return Result<ProjectHealthStatus>.Failure($"Error calculating project health: {ex.Message}");
        }
    }

    public async Task<Result<ProgressSummaryDto>> GetProgressSummaryAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await GetMasterPlanWithPhasesAndMilestonesAsync(masterPlanId);
            if (masterPlan == null)
                return Result<ProgressSummaryDto>.Failure($"Master plan with ID {masterPlanId} not found");

            var summary = CreateProgressSummary(masterPlan);
            return Result<ProgressSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress summary for master plan {MasterPlanId}", masterPlanId);
            return Result<ProgressSummaryDto>.Failure($"Error getting progress summary: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, object>>> GetProjectMetricsAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await GetMasterPlanWithPhasesAndMilestonesAsync(masterPlanId);
            if (masterPlan == null)
                return Result<Dictionary<string, object>>.Failure($"Master plan with ID {masterPlanId} not found");

            var metrics = await BuildProjectMetricsAsync(masterPlan);
            return Result<Dictionary<string, object>>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project metrics for master plan {MasterPlanId}", masterPlanId);
            return Result<Dictionary<string, object>>.Failure($"Error calculating project metrics: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectPhaseDto>>> GetCriticalPathAsync(Guid masterPlanId)
    {
        try
        {
            var phases = await _context.ProjectPhases
                .Where(p => p.MasterPlanId == masterPlanId)
                .OrderBy(p => p.PhaseOrder)
                .ToListAsync();

            if (!phases.Any())
                return Result<List<ProjectPhaseDto>>.Success(new List<ProjectPhaseDto>());

            var criticalPath = IdentifyCriticalPath(phases);
            var dtos = MapToPhaseDtos(criticalPath);

            return Result<List<ProjectPhaseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying critical path for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error identifying critical path: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private async Task<MasterPlan?> GetMasterPlanWithPhasesAsync(Guid masterPlanId)
    {
        return await _context.MasterPlans
            .Include(mp => mp.Phases)
            .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);
    }

    private async Task<MasterPlan?> GetMasterPlanWithPhasesAndMilestonesAsync(Guid masterPlanId)
    {
        return await _context.MasterPlans
            .Include(mp => mp.Phases)
            .Include(mp => mp.Milestones)
            .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);
    }

    private static decimal CalculateWeightedProgress(ICollection<ProjectPhase> phases)
    {
        if (!phases.Any())
            return 0;

        var totalWeight = phases.Sum(p => p.WeightPercentage);
        if (totalWeight > 0)
        {
            return phases.Sum(p => p.CompletionPercentage * (p.WeightPercentage / totalWeight));
        }

        // If no weights are assigned, use simple average  
        return phases.Average(p => p.CompletionPercentage);
    }

    private static ProjectHealthStatus DetermineProjectHealth(MasterPlan masterPlan)
    {
        var delayedPhases = CountDelayedPhases(masterPlan.Phases);
        var delayedMilestones = CountDelayedMilestones(masterPlan.Milestones);

        return (delayedPhases, delayedMilestones) switch
        {
            (0, 0) => ProjectHealthStatus.Excellent,
            (0, < 2) => ProjectHealthStatus.Good,
            (< 2, < 3) => ProjectHealthStatus.Caution,
            (< 3, < 5) => ProjectHealthStatus.AtRisk,
            _ => ProjectHealthStatus.Critical
        };
    }

    private static int CountDelayedPhases(ICollection<ProjectPhase> phases)
    {
        return phases.Count(p => 
            p.Status != PhaseStatus.Completed && 
            p.PlannedEndDate < DateTime.UtcNow);
    }

    private static int CountDelayedMilestones(ICollection<ProjectMilestone> milestones)
    {
        return milestones.Count(m => 
            m.Status != MilestoneStatus.Completed && 
            m.TargetDate < DateTime.UtcNow);
    }

    private static ProgressSummaryDto CreateProgressSummary(MasterPlan masterPlan)
    {
        var totalPhases = masterPlan.Phases.Count;
        var completedPhases = masterPlan.Phases.Count(p => p.Status == PhaseStatus.Completed);
        var totalMilestones = masterPlan.Milestones.Count;
        var completedMilestones = masterPlan.Milestones.Count(m => m.Status == MilestoneStatus.Completed);

        return new ProgressSummaryDto
        {
            OverallProgress = CalculateWeightedProgress(masterPlan.Phases),
            TotalPhases = totalPhases,
            CompletedPhases = completedPhases,
            TotalMilestones = totalMilestones,
            CompletedMilestones = completedMilestones,
            ProjectHealth = DetermineProjectHealth(masterPlan),
            LastUpdated = DateTime.UtcNow
        };
    }

    private async Task<Dictionary<string, object>> BuildProjectMetricsAsync(MasterPlan masterPlan)
    {
        var metrics = new Dictionary<string, object>
        {
            ["TotalPhases"] = masterPlan.Phases.Count,
            ["CompletedPhases"] = masterPlan.Phases.Count(p => p.Status == PhaseStatus.Completed),
            ["TotalMilestones"] = masterPlan.Milestones.Count,
            ["CompletedMilestones"] = masterPlan.Milestones.Count(m => m.Status == MilestoneStatus.Completed),
            ["PlannedDuration"] = masterPlan.TotalPlannedDays,
            ["ElapsedDays"] = (DateTime.UtcNow - masterPlan.PlannedStartDate).Days,
            ["RemainingDays"] = Math.Max(0, (masterPlan.PlannedEndDate - DateTime.UtcNow).Days)
        };

        // Add progress metrics
        var progressResult = await CalculateOverallProgressAsync(masterPlan.MasterPlanId);
        if (progressResult.IsSuccess)
        {
            metrics["OverallProgress"] = progressResult.Data;
            
            // Calculate expected progress based on timeline
            var expectedProgress = CalculateExpectedProgress(masterPlan);
            metrics["ExpectedProgress"] = expectedProgress;
            metrics["ProgressVariance"] = progressResult.Data - expectedProgress;
        }

        // Add health status
        var healthResult = await CalculateProjectHealthAsync(masterPlan.MasterPlanId);
        if (healthResult.IsSuccess)
            metrics["HealthStatus"] = healthResult.Data;

        return metrics;
    }

    private static decimal CalculateExpectedProgress(MasterPlan masterPlan)
    {
        var totalDuration = (masterPlan.PlannedEndDate - masterPlan.PlannedStartDate).TotalDays;
        if (totalDuration <= 0)
            return 0;

        var elapsed = (DateTime.UtcNow - masterPlan.PlannedStartDate).TotalDays;
        var expectedProgress = (decimal)(elapsed / totalDuration * 100);
        
        return Math.Min(100, Math.Max(0, expectedProgress));
    }

    private static List<ProjectPhase> IdentifyCriticalPath(List<ProjectPhase> phases)
    {
        // Simplified critical path - in real projects this would use CPM algorithm
        return phases
            .OrderByDescending(p => p.WeightPercentage)
            .ThenBy(p => p.PlannedStartDate)
            .Take(Math.Max(1, phases.Count / 2))
            .ToList();
    }

    private static List<ProjectPhaseDto> MapToPhaseDtos(List<ProjectPhase> phases)
    {
        return phases.Select(p => new ProjectPhaseDto
        {
            PhaseId = p.PhaseId,
            PhaseName = p.PhaseName,
            Description = p.Description,
            PlannedStartDate = p.PlannedStartDate,
            PlannedEndDate = p.PlannedEndDate,
            ActualStartDate = p.ActualStartDate,
            ActualEndDate = p.ActualEndDate,
            Status = p.Status,
            CompletionPercentage = p.CompletionPercentage,
            WeightPercentage = p.WeightPercentage,
            PhaseOrder = p.PhaseOrder
        }).ToList();
    }

    #endregion
}
