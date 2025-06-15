using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using AutoMapper;
using System.Collections.Generic;

namespace dotnet_rest_api.Services;

/// <summary>
/// Implementation of the MasterPlan service with CRUD operations and advanced functionality
/// </summary>
public class MasterPlanService : IMasterPlanService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MasterPlanService> _logger;

    public MasterPlanService(ApplicationDbContext context, IMapper mapper, ILogger<MasterPlanService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Basic CRUD Operations

    public async Task<IEnumerable<MasterPlan>> GetAllMasterPlansAsync()
    {
        return await _context.MasterPlans
            .Include(mp => mp.Project)
            .ToListAsync();
    }

    public async Task<MasterPlan?> GetMasterPlanByIdAsync(Guid id)
    {
        return await _context.MasterPlans
            .Include(mp => mp.Project)
            .Include(mp => mp.Phases)
            .Include(mp => mp.Milestones)
            .FirstOrDefaultAsync(mp => mp.MasterPlanId == id);
    }

    public async Task<IEnumerable<MasterPlan>> GetMasterPlansByProjectIdAsync(Guid projectId)
    {
        return await _context.MasterPlans
            .Where(mp => mp.ProjectId == projectId)
            .Include(mp => mp.Phases)
            .Include(mp => mp.Milestones)
            .ToListAsync();
    }

    public async Task<MasterPlan> CreateMasterPlanAsync(MasterPlan masterPlan)
    {
        masterPlan.CreatedAt = DateTime.UtcNow;
        _context.MasterPlans.Add(masterPlan);
        await _context.SaveChangesAsync();
        return masterPlan;
    }

    public async Task<MasterPlan?> UpdateMasterPlanAsync(Guid id, MasterPlan masterPlan)
    {
        var existingPlan = await _context.MasterPlans.FindAsync(id);
        if (existingPlan == null)
            return null;

        // Update properties
        existingPlan.PlanName = masterPlan.PlanName;
        existingPlan.Description = masterPlan.Description;
        existingPlan.PlannedStartDate = masterPlan.PlannedStartDate;
        existingPlan.PlannedEndDate = masterPlan.PlannedEndDate;
        existingPlan.TotalPlannedDays = masterPlan.TotalPlannedDays;
        existingPlan.TotalEstimatedBudget = masterPlan.TotalEstimatedBudget;
        existingPlan.Status = masterPlan.Status;
        existingPlan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingPlan;
    }

    public async Task<bool> DeleteMasterPlanAsync(Guid id)
    {
        var masterPlan = await _context.MasterPlans.FindAsync(id);
        if (masterPlan == null)
            return false;

        _context.MasterPlans.Remove(masterPlan);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Advanced Master Plan Operations

    public async Task<Result<MasterPlanDto>> CreateMasterPlanAsync(CreateMasterPlanRequest request, Guid createdById)
    {
        try
        {
            var projectId = Guid.Parse(request.ProjectId.ToString());
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return Result<MasterPlanDto>.Failure($"Project with ID {projectId} not found");

            var masterPlan = new MasterPlan
            {
                MasterPlanId = Guid.NewGuid(),
                ProjectId = projectId,
                PlanName = request.Title,
                Description = request.Description,
                PlannedStartDate = request.StartDate,
                PlannedEndDate = request.EndDate,
                TotalPlannedDays = (request.EndDate - request.StartDate).Days,
                TotalEstimatedBudget = request.Budget,
                Version = 1,
                Status = MasterPlanStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById
            };

            _context.MasterPlans.Add(masterPlan);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<MasterPlanDto>(masterPlan);
            return Result<MasterPlanDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating master plan");
            return Result<MasterPlanDto>.Failure($"Error creating master plan: {ex.Message}");
        }
    }

    public async Task<Result<MasterPlanDto>> GetMasterPlanDtoByIdAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Project)
                .Include(mp => mp.CreatedBy)
                .Include(mp => mp.ApprovedBy)
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<MasterPlanDto>.Failure($"Master plan with ID {masterPlanId} not found");

            var dto = _mapper.Map<MasterPlanDto>(masterPlan);
            return Result<MasterPlanDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving master plan {MasterPlanId}", masterPlanId);
            return Result<MasterPlanDto>.Failure($"Error retrieving master plan: {ex.Message}");
        }
    }

    public async Task<Result<MasterPlanDto>> GetMasterPlanByProjectIdAsync(Guid projectId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Project)
                .Include(mp => mp.CreatedBy)
                .Include(mp => mp.ApprovedBy)
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.ProjectId == projectId);

            if (masterPlan == null)
                return Result<MasterPlanDto>.Failure($"No master plan found for project ID {projectId}");

            var dto = _mapper.Map<MasterPlanDto>(masterPlan);
            return Result<MasterPlanDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving master plan for project {ProjectId}", projectId);
            return Result<MasterPlanDto>.Failure($"Error retrieving master plan: {ex.Message}");
        }
    }

    public async Task<Result<MasterPlanDto>> UpdateMasterPlanAsync(Guid masterPlanId, UpdateMasterPlanRequest request)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<MasterPlanDto>.Failure($"Master plan with ID {masterPlanId} not found");

            // Update only the properties that should be updated
            masterPlan.PlanName = request.Title;
            masterPlan.Description = request.Description;
            masterPlan.PlannedStartDate = request.StartDate;
            masterPlan.PlannedEndDate = request.EndDate;
            masterPlan.TotalPlannedDays = (request.EndDate - request.StartDate).Days;
            masterPlan.Status = (MasterPlanStatus)Enum.Parse(typeof(MasterPlanStatus), request.Status);
            masterPlan.TotalEstimatedBudget = request.Budget;
            masterPlan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<MasterPlanDto>(masterPlan);
            return Result<MasterPlanDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating master plan {MasterPlanId}", masterPlanId);
            return Result<MasterPlanDto>.Failure($"Error updating master plan: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteMasterPlanDtoAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

            // Check if the plan can be deleted (e.g., not active, no dependencies)
            if (masterPlan.Status == MasterPlanStatus.Active)
                return Result<bool>.Failure("Cannot delete an active master plan");

            _context.MasterPlans.Remove(masterPlan);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting master plan {MasterPlanId}", masterPlanId);
            return Result<bool>.Failure($"Error deleting master plan: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ApproveMasterPlanAsync(Guid masterPlanId, Guid approvedById, string? notes)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

            // Check if plan can be approved
            if (masterPlan.Status != MasterPlanStatus.Draft && masterPlan.Status != MasterPlanStatus.UnderReview)
                return Result<bool>.Failure($"Master plan with status {masterPlan.Status} cannot be approved");

            masterPlan.Status = MasterPlanStatus.Approved;
            masterPlan.ApprovedById = approvedById;
            masterPlan.ApprovedAt = DateTime.UtcNow;
            masterPlan.ApprovalNotes = notes;
            masterPlan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving master plan {MasterPlanId}", masterPlanId);
            return Result<bool>.Failure($"Error approving master plan: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ActivateMasterPlanAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

            // Check if plan can be activated
            if (masterPlan.Status != MasterPlanStatus.Approved)
                return Result<bool>.Failure($"Only approved master plans can be activated");

            masterPlan.Status = MasterPlanStatus.Active;
            masterPlan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating master plan {MasterPlanId}", masterPlanId);
            return Result<bool>.Failure($"Error activating master plan: {ex.Message}");
        }
    }

    #endregion

    #region Phase Management

    public async Task<Result<ProjectPhaseDto>> AddPhaseToMasterPlanAsync(Guid masterPlanId, CreateProjectPhaseRequest request)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<ProjectPhaseDto>.Failure($"Master plan with ID {masterPlanId} not found");

            // Determine phase order (next available number)
            int phaseOrder = masterPlan.Phases.Count > 0 ? masterPlan.Phases.Max(p => p.PhaseOrder) + 1 : 1;

            var phase = new ProjectPhase
            {
                PhaseId = Guid.NewGuid(),
                MasterPlanId = masterPlanId,
                PhaseName = request.Name,
                Description = request.Description,
                PhaseOrder = phaseOrder,
                PlannedStartDate = request.StartDate,
                PlannedEndDate = request.EndDate,
                PlannedDurationDays = (request.EndDate - request.StartDate).Days,
                Status = PhaseStatus.NotStarted,
                EstimatedBudget = 0, // Default value, can be updated later
                CompletionPercentage = 0,
                WeightPercentage = 0, // Default value, can be updated later
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectPhases.Add(phase);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProjectPhaseDto>(phase);
            return Result<ProjectPhaseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding phase to master plan {MasterPlanId}", masterPlanId);
            return Result<ProjectPhaseDto>.Failure($"Error adding phase: {ex.Message}");
        }
    }

    public async Task<Result<ProjectPhaseDto>> UpdatePhaseAsync(Guid phaseId, UpdateProjectPhaseRequest request)
    {
        try
        {
            var phase = await _context.ProjectPhases.FindAsync(phaseId);
            if (phase == null)
                return Result<ProjectPhaseDto>.Failure($"Phase with ID {phaseId} not found");

            phase.PhaseName = request.Name;
            phase.Description = request.Description;
            phase.PlannedStartDate = request.StartDate;
            phase.PlannedEndDate = request.EndDate;
            phase.PlannedDurationDays = (request.EndDate - request.StartDate).Days;
            phase.Status = request.Status;
            phase.CompletionPercentage = request.CompletionPercentage;
            phase.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProjectPhaseDto>(phase);
            return Result<ProjectPhaseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phase {PhaseId}", phaseId);
            return Result<ProjectPhaseDto>.Failure($"Error updating phase: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeletePhaseAsync(Guid phaseId)
    {
        try
        {
            var phase = await _context.ProjectPhases.FindAsync(phaseId);
            if (phase == null)
                return Result<bool>.Failure($"Phase with ID {phaseId} not found");

            _context.ProjectPhases.Remove(phase);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting phase {PhaseId}", phaseId);
            return Result<bool>.Failure($"Error deleting phase: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectPhaseDto>>> GetPhasesByMasterPlanAsync(Guid masterPlanId)
    {
        try
        {
            var phases = await _context.ProjectPhases
                .Where(p => p.MasterPlanId == masterPlanId)
                .OrderBy(p => p.PhaseOrder)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectPhaseDto>>(phases);
            return Result<List<ProjectPhaseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving phases for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error retrieving phases: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdatePhaseProgressAsync(Guid phaseId, decimal completionPercentage, PhaseStatus status)
    {
        try
        {
            var phase = await _context.ProjectPhases.FindAsync(phaseId);
            if (phase == null)
                return Result<bool>.Failure($"Phase with ID {phaseId} not found");

            phase.CompletionPercentage = completionPercentage;
            phase.Status = status;
            phase.UpdatedAt = DateTime.UtcNow;

            // Update actual start/end dates based on status
            if (status == PhaseStatus.InProgress && phase.ActualStartDate == null)
            {
                phase.ActualStartDate = DateTime.UtcNow;
            }
            else if (status == PhaseStatus.Completed && phase.ActualEndDate == null)
            {
                phase.ActualEndDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phase progress {PhaseId}", phaseId);
            return Result<bool>.Failure($"Error updating phase progress: {ex.Message}");
        }
    }

    #endregion

    #region Milestone Management

    public async Task<Result<ProjectMilestoneDto>> AddMilestoneToMasterPlanAsync(Guid masterPlanId, CreateProjectMilestoneRequest request)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<ProjectMilestoneDto>.Failure($"Master plan with ID {masterPlanId} not found");

            var milestone = new ProjectMilestone
            {
                MilestoneId = Guid.NewGuid(),
                MasterPlanId = masterPlanId,
                MilestoneName = request.Name,
                Description = request.Description,
                PlannedDate = request.DueDate,
                Type = MilestoneType.Custom, // Default value
                Status = MilestoneStatus.Pending,
                WeightPercentage = 0, // Default value
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectMilestones.Add(milestone);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProjectMilestoneDto>(milestone);
            return Result<ProjectMilestoneDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding milestone to master plan {MasterPlanId}", masterPlanId);
            return Result<ProjectMilestoneDto>.Failure($"Error adding milestone: {ex.Message}");
        }
    }

    public async Task<Result<ProjectMilestoneDto>> UpdateMilestoneAsync(Guid milestoneId, UpdateProjectMilestoneRequest request)
    {
        try
        {
            var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
            if (milestone == null)
                return Result<ProjectMilestoneDto>.Failure($"Milestone with ID {milestoneId} not found");

            milestone.MilestoneName = request.Name;
            milestone.Description = request.Description;
            milestone.PlannedDate = request.DueDate;
            milestone.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<ProjectMilestoneDto>(milestone);
            return Result<ProjectMilestoneDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating milestone {MilestoneId}", milestoneId);
            return Result<ProjectMilestoneDto>.Failure($"Error updating milestone: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CompleteMilestoneAsync(Guid milestoneId, Guid verifiedById, string? evidence)
    {
        try
        {
            var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
            if (milestone == null)
                return Result<bool>.Failure($"Milestone with ID {milestoneId} not found");

            milestone.Status = MilestoneStatus.Completed;
            milestone.ActualDate = DateTime.UtcNow;
            milestone.VerifiedById = verifiedById;
            milestone.VerifiedAt = DateTime.UtcNow;
            milestone.CompletionEvidence = evidence;
            milestone.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing milestone {MilestoneId}", milestoneId);
            return Result<bool>.Failure($"Error completing milestone: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteMilestoneAsync(Guid milestoneId)
    {
        try
        {
            var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
            if (milestone == null)
                return Result<bool>.Failure($"Milestone with ID {milestoneId} not found");

            _context.ProjectMilestones.Remove(milestone);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting milestone {MilestoneId}", milestoneId);
            return Result<bool>.Failure($"Error deleting milestone: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectMilestoneDto>>> GetMilestonesByMasterPlanAsync(Guid masterPlanId)
    {
        try
        {
            var milestones = await _context.ProjectMilestones
                .Where(m => m.MasterPlanId == masterPlanId)
                .OrderBy(m => m.PlannedDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectMilestoneDto>>(milestones);
            return Result<List<ProjectMilestoneDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving milestones for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving milestones: {ex.Message}");
        }
    }

    #endregion

    #region Progress Tracking

    public async Task<Result<ProgressReportDto>> CreateProgressReportAsync(Guid masterPlanId, CreateProgressReportRequest request, Guid createdById)
    {
        try
        {
            // Verify master plan exists
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .Include(mp => mp.Project)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<ProgressReportDto>.Failure($"Master plan with ID {masterPlanId} not found");

            // Calculate overall progress based on phase completion
            decimal overallCompletionPercentage = 0;
            if (masterPlan.Phases.Any())
            {
                overallCompletionPercentage = masterPlan.Phases.Sum(p => p.CompletionPercentage * (p.WeightPercentage / 100)) / 
                                             masterPlan.Phases.Sum(p => p.WeightPercentage / 100);
            }

            // Create the progress report
            var progressReport = new ProgressReport
            {
                ProgressReportId = Guid.NewGuid(),
                MasterPlanId = masterPlanId,
                ProjectId = masterPlan.ProjectId,
                ReportDate = DateTime.UtcNow,
                OverallCompletionPercentage = overallCompletionPercentage,
                KeyAccomplishments = request.KeyAccomplishments,
                CurrentChallenges = request.CurrentChallenges,
                UpcomingActivities = request.UpcomingActivities,
                RiskSummary = request.RiskSummary,
                QualityNotes = request.QualityNotes,
                WeatherImpact = request.WeatherImpact,
                ResourceNotes = request.ResourceNotes,
                ExecutiveSummary = request.ExecutiveSummary,
                CreatedById = createdById,
                CreatedAt = DateTime.UtcNow,
                
                // Default values for calculated fields (to be updated by separate logic)
                SchedulePerformanceIndex = 1.0m,
                CostPerformanceIndex = 1.0m,
                ActualCostToDate = 0,
                EstimatedCostAtCompletion = 0,
                BudgetVariance = 0,
                ScheduleVarianceDays = 0,
                ProjectedCompletionDate = masterPlan.PlannedEndDate,
                HealthStatus = ProjectHealthStatus.Good,
                ActiveIssuesCount = 0,
                CompletedMilestonesCount = 0,
                TotalMilestonesCount = 0
            };

            _context.ProgressReports.Add(progressReport);
            
            // Add phase progress details
            foreach (var phaseUpdate in request.PhaseUpdates)
            {
                var phase = await _context.ProjectPhases.FindAsync(phaseUpdate.PhaseId);
                if (phase != null)
                {
                    var phaseProgress = new PhaseProgress
                    {
                        PhaseProgressId = Guid.NewGuid(),
                        ProgressReportId = progressReport.ProgressReportId,
                        PhaseId = phaseUpdate.PhaseId,
                        CompletionPercentage = phaseUpdate.CompletionPercentage,
                        PlannedCompletionPercentage = 0, // To be calculated
                        ProgressVariance = 0, // To be calculated
                        Status = phaseUpdate.Status,
                        Notes = phaseUpdate.Notes,
                        ActivitiesCompleted = phaseUpdate.ActivitiesCompleted,
                        Issues = phaseUpdate.Issues
                    };
                    
                    // Update the phase itself with the new progress
                    phase.CompletionPercentage = phaseUpdate.CompletionPercentage;
                    phase.Status = phaseUpdate.Status;
                    phase.UpdatedAt = DateTime.UtcNow;
                    
                    _context.PhaseProgresses.Add(phaseProgress);
                }
            }

            await _context.SaveChangesAsync();

            // Map to DTO with related data
            var reportDto = await GetProgressReportDtoAsync(progressReport.ProgressReportId);
            return Result<ProgressReportDto>.Success(reportDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating progress report for master plan {MasterPlanId}", masterPlanId);
            return Result<ProgressReportDto>.Failure($"Error creating progress report: {ex.Message}");
        }
    }

    public async Task<Result<List<ProgressReportDto>>> GetProgressReportsAsync(Guid masterPlanId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var reports = await _context.ProgressReports
                .Where(r => r.MasterPlanId == masterPlanId)
                .OrderByDescending(r => r.ReportDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var reportDtos = new List<ProgressReportDto>();
            foreach (var report in reports)
            {
                reportDtos.Add(await GetProgressReportDtoAsync(report.ProgressReportId));
            }

            return Result<List<ProgressReportDto>>.Success(reportDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress reports for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProgressReportDto>>.Failure($"Error retrieving progress reports: {ex.Message}");
        }
    }

    public async Task<Result<ProgressReportDto>> GetLatestProgressReportAsync(Guid masterPlanId)
    {
        try
        {
            var latestReport = await _context.ProgressReports
                .Where(r => r.MasterPlanId == masterPlanId)
                .OrderByDescending(r => r.ReportDate)
                .FirstOrDefaultAsync();

            if (latestReport == null)
                return Result<ProgressReportDto>.Failure($"No progress reports found for master plan {masterPlanId}");

            var reportDto = await GetProgressReportDtoAsync(latestReport.ProgressReportId);
            return Result<ProgressReportDto>.Success(reportDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest progress report for master plan {MasterPlanId}", masterPlanId);
            return Result<ProgressReportDto>.Failure($"Error retrieving latest progress report: {ex.Message}");
        }
    }

    private async Task<ProgressReportDto> GetProgressReportDtoAsync(Guid reportId)
    {
        var report = await _context.ProgressReports
            .Include(r => r.Project)
            .Include(r => r.CreatedBy)
            .Include(r => r.PhaseProgressDetails)
                .ThenInclude(pp => pp.Phase)
            .FirstOrDefaultAsync(r => r.ProgressReportId == reportId);

        if (report == null)
            throw new KeyNotFoundException($"Progress report with ID {reportId} not found");

        var dto = _mapper.Map<ProgressReportDto>(report);
        
        // Add related data
        dto.ProjectName = report.Project.ProjectName;
        dto.CreatedByName = report.CreatedBy.FullName;
        
        // Map phase progress details
        foreach (var phaseProgress in report.PhaseProgressDetails)
        {
            dto.PhaseProgressDetails.Add(new PhaseProgressDto
            {
                PhaseProgressId = phaseProgress.PhaseProgressId,
                PhaseId = phaseProgress.PhaseId,
                PhaseName = phaseProgress.Phase.PhaseName,
                CompletionPercentage = phaseProgress.CompletionPercentage,
                PlannedCompletionPercentage = phaseProgress.PlannedCompletionPercentage,
                ProgressVariance = phaseProgress.ProgressVariance,
                Status = phaseProgress.Status,
                Notes = phaseProgress.Notes,
                ActivitiesCompleted = phaseProgress.ActivitiesCompleted,
                Issues = phaseProgress.Issues
            });
        }

        return dto;
    }

    public async Task<Result<ProgressSummaryDto>> GetProgressSummaryAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<ProgressSummaryDto>.Failure($"Master plan with ID {masterPlanId} not found");

            // Calculate metrics
            var overallCompletionResult = await CalculateOverallProgressAsync(masterPlanId);
            var healthStatusResult = await CalculateProjectHealthAsync(masterPlanId);

            if (overallCompletionResult.IsFailure || healthStatusResult.IsFailure)
                return Result<ProgressSummaryDto>.Failure("Failed to calculate project metrics");

            var summary = new ProgressSummaryDto
            {
                OverallCompletion = overallCompletionResult.Data,
                HealthStatus = healthStatusResult.Data,
                CompletedPhases = masterPlan.Phases.Count(p => p.Status == PhaseStatus.Completed),
                TotalPhases = masterPlan.Phases.Count,
                CompletedMilestones = masterPlan.Milestones.Count(m => m.Status == MilestoneStatus.Completed),
                TotalMilestones = masterPlan.Milestones.Count,
                DaysRemaining = (masterPlan.PlannedEndDate - DateTime.UtcNow).Days,
                IsOnSchedule = (DateTime.UtcNow <= masterPlan.PlannedEndDate),
                IsOnBudget = true, // Assuming budget tracking is implemented elsewhere
                LastUpdated = masterPlan.UpdatedAt ?? masterPlan.CreatedAt
            };

            return Result<ProgressSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress summary for master plan {MasterPlanId}", masterPlanId);
            return Result<ProgressSummaryDto>.Failure($"Error retrieving progress summary: {ex.Message}");
        }
    }

    #endregion

    #region Analytics and Reporting

    public async Task<Result<decimal>> CalculateOverallProgressAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<decimal>.Failure($"Master plan with ID {masterPlanId} not found");

            decimal overallProgress = 0;
            
            // Calculate weighted progress based on phase completion
            if (masterPlan.Phases.Any())
            {
                decimal totalWeight = masterPlan.Phases.Sum(p => p.WeightPercentage);
                if (totalWeight > 0)
                {
                    overallProgress = masterPlan.Phases.Sum(p => 
                        p.CompletionPercentage * (p.WeightPercentage / totalWeight));
                }
                else
                {
                    // If no weights are assigned, use simple average
                    overallProgress = masterPlan.Phases.Average(p => p.CompletionPercentage);
                }
            }

            return Result<decimal>.Success(overallProgress);
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
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<ProjectHealthStatus>.Failure($"Master plan with ID {masterPlanId} not found");
            
            // Simple health calculation - can be enhanced with more sophisticated logic
            int delayedPhases = masterPlan.Phases.Count(p => p.Status == PhaseStatus.Delayed);
            int delayedMilestones = masterPlan.Milestones.Count(m => m.Status == MilestoneStatus.Delayed || m.Status == MilestoneStatus.AtRisk);
            
            ProjectHealthStatus health;
            
            if (delayedPhases == 0 && delayedMilestones == 0)
            {
                health = ProjectHealthStatus.Excellent;
            }
            else if (delayedPhases == 0 && delayedMilestones < 2)
            {
                health = ProjectHealthStatus.Good;
            }
            else if (delayedPhases < 2 || delayedMilestones < 3)
            {
                health = ProjectHealthStatus.Caution;
            }
            else if (delayedPhases < 3 || delayedMilestones < 5)
            {
                health = ProjectHealthStatus.AtRisk;
            }
            else
            {
                health = ProjectHealthStatus.Critical;
            }

            return Result<ProjectHealthStatus>.Success(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project health for master plan {MasterPlanId}", masterPlanId);
            return Result<ProjectHealthStatus>.Failure($"Error calculating project health: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectPhaseDto>>> GetCriticalPathAsync(Guid masterPlanId)
    {
        // Simplified critical path implementation - in real projects this would be more complex
        try
        {
            var phases = await _context.ProjectPhases
                .Where(p => p.MasterPlanId == masterPlanId)
                .OrderBy(p => p.PhaseOrder)
                .ToListAsync();

            // Just return phases in order for now - critical path calculation is more complex
            var criticalPath = phases
                .OrderByDescending(p => p.WeightPercentage)
                .ThenBy(p => p.PlannedStartDate)
                .Take(phases.Count / 2 + 1) // Just take the highest weighted phases
                .ToList();

            var dtos = _mapper.Map<List<ProjectPhaseDto>>(criticalPath);
            return Result<List<ProjectPhaseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying critical path for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error identifying critical path: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, object>>> GetProjectMetricsAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<Dictionary<string, object>>.Failure($"Master plan with ID {masterPlanId} not found");

            var metrics = new Dictionary<string, object>();
            
            // Calculate various metrics
            metrics["TotalPhases"] = masterPlan.Phases.Count;
            metrics["CompletedPhases"] = masterPlan.Phases.Count(p => p.Status == PhaseStatus.Completed);
            metrics["TotalMilestones"] = masterPlan.Milestones.Count;
            metrics["CompletedMilestones"] = masterPlan.Milestones.Count(m => m.Status == MilestoneStatus.Completed);
            
            var progressResult = await CalculateOverallProgressAsync(masterPlanId);
            if (progressResult.IsSuccess)
                metrics["OverallProgress"] = progressResult.Data;
            
            var healthResult = await CalculateProjectHealthAsync(masterPlanId);
            if (healthResult.IsSuccess)
                metrics["HealthStatus"] = healthResult.Data;
                
            // Calculate schedule metrics
            metrics["PlannedDuration"] = masterPlan.TotalPlannedDays;
            metrics["ElapsedDays"] = (DateTime.UtcNow - masterPlan.PlannedStartDate).Days;
            metrics["RemainingDays"] = Math.Max(0, (masterPlan.PlannedEndDate - DateTime.UtcNow).Days);
            
            // Is project on schedule?
            decimal expectedProgress = 0;
            var totalDuration = (masterPlan.PlannedEndDate - masterPlan.PlannedStartDate).TotalDays;
            if (totalDuration > 0)
            {
                var elapsed = (DateTime.UtcNow - masterPlan.PlannedStartDate).TotalDays;
                expectedProgress = (decimal)(elapsed / totalDuration * 100);
            }
            
            metrics["ExpectedProgress"] = Math.Min(100, Math.Max(0, expectedProgress));
            metrics["ProgressVariance"] = progressResult.IsSuccess 
                ? progressResult.Data - (decimal)metrics["ExpectedProgress"]
                : 0;
            
            return Result<Dictionary<string, object>>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project metrics for master plan {MasterPlanId}", masterPlanId);
            return Result<Dictionary<string, object>>.Failure($"Error calculating project metrics: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectMilestoneDto>>> GetUpcomingMilestonesAsync(Guid masterPlanId, int days = 30)
    {
        try
        {
            var endDate = DateTime.UtcNow.AddDays(days);
            var milestones = await _context.ProjectMilestones
                .Where(m => m.MasterPlanId == masterPlanId && 
                           m.Status != MilestoneStatus.Completed && 
                           m.Status != MilestoneStatus.Cancelled &&
                           m.PlannedDate <= endDate)
                .OrderBy(m => m.PlannedDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectMilestoneDto>>(milestones);
            return Result<List<ProjectMilestoneDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming milestones for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving upcoming milestones: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectPhaseDto>>> GetDelayedPhasesAsync(Guid masterPlanId)
    {
        try
        {
            var phases = await _context.ProjectPhases
                .Where(p => p.MasterPlanId == masterPlanId && 
                           (p.Status == PhaseStatus.Delayed || 
                            (p.PlannedStartDate < DateTime.UtcNow && p.Status == PhaseStatus.NotStarted) ||
                            (p.PlannedEndDate < DateTime.UtcNow && p.Status == PhaseStatus.InProgress)))
                .OrderBy(p => p.PhaseOrder)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectPhaseDto>>(phases);
            return Result<List<ProjectPhaseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed phases for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error retrieving delayed phases: {ex.Message}");
        }
    }

    #endregion

    #region Template Management

    public async Task<Result<MasterPlanDto>> CreateFromTemplateAsync(Guid projectId, string templateName, Guid createdById)
    {
        try
        {
            // Get project
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return Result<MasterPlanDto>.Failure($"Project with ID {projectId} not found");

            // Simple templates - in a real app, these would be stored in the database or as configuration
            Dictionary<string, List<(string Name, int DurationDays, decimal Weight)>> templates = new()
            {
                {
                    "Solar Installation", new List<(string, int, decimal)>
                    {
                        ("Site Assessment", 7, 5),
                        ("Permit Application", 14, 5),
                        ("System Design", 10, 10),
                        ("Material Procurement", 21, 10),
                        ("Site Preparation", 7, 10),
                        ("Panel Installation", 14, 25),
                        ("Electrical Work", 7, 15),
                        ("Final Inspection", 3, 5),
                        ("Grid Connection", 7, 10),
                        ("System Testing", 5, 5)
                    }
                },
                {
                    "Maintenance", new List<(string, int, decimal)>
                    {
                        ("Initial Assessment", 3, 20),
                        ("Panel Cleaning", 2, 25),
                        ("Electrical Checks", 3, 25),
                        ("Inverter Service", 2, 20),
                        ("Performance Review", 2, 10)
                    }
                }
            };

            if (!templates.ContainsKey(templateName))
                return Result<MasterPlanDto>.Failure($"Template '{templateName}' not found");

            var template = templates[templateName];

            // Create master plan
            DateTime startDate = DateTime.UtcNow.Date;
            int totalDays = template.Sum(p => p.DurationDays);
            DateTime endDate = startDate.AddDays(totalDays);

            var masterPlan = new MasterPlan
            {
                MasterPlanId = Guid.NewGuid(),
                ProjectId = projectId,
                PlanName = $"{templateName} Plan - {project.ProjectName}",
                Description = $"Created from {templateName} template",
                PlannedStartDate = startDate,
                PlannedEndDate = endDate,
                TotalPlannedDays = totalDays,
                TotalEstimatedBudget = 0, // Default value
                Version = 1,
                Status = MasterPlanStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById
            };

            _context.MasterPlans.Add(masterPlan);

            // Create phases from template
            DateTime phaseStart = startDate;
            int order = 1;
            foreach (var (name, duration, weight) in template)
            {
                DateTime phaseEnd = phaseStart.AddDays(duration);
                
                var phase = new ProjectPhase
                {
                    PhaseId = Guid.NewGuid(),
                    MasterPlanId = masterPlan.MasterPlanId,
                    PhaseName = name,
                    Description = $"Created from template",
                    PhaseOrder = order++,
                    PlannedStartDate = phaseStart,
                    PlannedEndDate = phaseEnd,
                    PlannedDurationDays = duration,
                    Status = PhaseStatus.NotStarted,
                    EstimatedBudget = 0, // Default value
                    CompletionPercentage = 0,
                    WeightPercentage = weight,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.ProjectPhases.Add(phase);
                phaseStart = phaseEnd.AddDays(1);
            }

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<MasterPlanDto>(masterPlan);
            return Result<MasterPlanDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating master plan from template {TemplateName} for project {ProjectId}", templateName, projectId);
            return Result<MasterPlanDto>.Failure($"Error creating master plan from template: {ex.Message}");
        }
    }

    public Task<Result<List<string>>> GetAvailableTemplatesAsync()
    {
        try
        {
            // In a real app, templates would be stored in the database or configuration
            List<string> templates = new() { "Solar Installation", "Maintenance" };
            return Task.FromResult(Result<List<string>>.Success(templates));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available templates");
            return Task.FromResult(Result<List<string>>.Failure($"Error retrieving available templates: {ex.Message}"));
        }
    }

    #endregion

    #region Validation and Business Rules

    public async Task<Result<bool>> ValidateMasterPlanAsync(Guid masterPlanId)
    {
        try
        {
            var errors = await GetValidationErrorsAsync(masterPlanId);
            if (errors.IsFailure)
                return Result<bool>.Failure(errors.Message);
            return Result<bool>.Success(errors.Data?.Count == 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating master plan {MasterPlanId}", masterPlanId);
            return Result<bool>.Failure($"Error validating master plan: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetValidationErrorsAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans
                .Include(mp => mp.Phases)
                .Include(mp => mp.Milestones)
                .FirstOrDefaultAsync(mp => mp.MasterPlanId == masterPlanId);

            if (masterPlan == null)
                return Result<List<string>>.Failure($"Master plan with ID {masterPlanId} not found");

            var errors = new List<string>();

            // Basic validation rules
            if (string.IsNullOrWhiteSpace(masterPlan.PlanName))
                errors.Add("Plan name cannot be empty");

            if (masterPlan.PlannedStartDate >= masterPlan.PlannedEndDate)
                errors.Add("End date must be after start date");

            if (!masterPlan.Phases.Any())
                errors.Add("Plan must have at least one phase");

            // Validate phase continuity
            var previousPhaseEnd = DateTime.MinValue;
            foreach (var phase in masterPlan.Phases.OrderBy(p => p.PhaseOrder))
            {
                if (phase.PlannedStartDate >= phase.PlannedEndDate)
                    errors.Add($"Phase '{phase.PhaseName}': End date must be after start date");

                if (phase.PhaseOrder > 1 && phase.PlannedStartDate < previousPhaseEnd)
                    errors.Add($"Phase '{phase.PhaseName}': Overlaps with the previous phase");

                previousPhaseEnd = phase.PlannedEndDate;
            }

            // Validate plan end date contains all phases
            var lastPhaseEnd = masterPlan.Phases.Any() ? 
                masterPlan.Phases.Max(p => p.PlannedEndDate) : 
                masterPlan.PlannedStartDate;
                
            if (lastPhaseEnd > masterPlan.PlannedEndDate)
                errors.Add("Master plan end date does not include all phases");

            return Result<List<string>>.Success(errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating master plan {MasterPlanId}", masterPlanId);
            return Result<List<string>>.Failure($"Error validating master plan: {ex.Message}");
        }
    }

    #endregion
}
