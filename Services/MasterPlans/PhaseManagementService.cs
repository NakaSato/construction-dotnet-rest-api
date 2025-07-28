using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace dotnet_rest_api.Services.MasterPlans;

/// <summary>
/// Service for managing project phases within master plans
/// Extracted from the original large MasterPlanService for better maintainability
/// </summary>
public interface IPhaseManagementService
{
    Task<Result<List<ProjectPhaseDto>>> GetPhasesByMasterPlanAsync(Guid masterPlanId);
    Task<Result<ProjectPhaseDto>> GetPhaseByIdAsync(Guid phaseId);
    Task<Result<ProjectPhaseDto>> AddPhaseToMasterPlanAsync(Guid masterPlanId, CreateProjectPhaseRequest request);
    Task<Result<ProjectPhaseDto>> UpdatePhaseAsync(Guid phaseId, UpdateProjectPhaseRequest request);
    Task<Result<bool>> UpdatePhaseProgressAsync(Guid phaseId, decimal completionPercentage, PhaseStatus status);
    Task<Result<bool>> DeletePhaseAsync(Guid phaseId);
    Task<Result<List<ProjectPhaseDto>>> GetDelayedPhasesAsync(Guid masterPlanId);
}

public class PhaseManagementService : IPhaseManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PhaseManagementService> _logger;

    public PhaseManagementService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<PhaseManagementService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
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

    public async Task<Result<ProjectPhaseDto>> GetPhaseByIdAsync(Guid phaseId)
    {
        try
        {
            var phase = await _context.ProjectPhases.FindAsync(phaseId);
            if (phase == null)
                return Result<ProjectPhaseDto>.Failure($"Phase with ID {phaseId} not found");

            var dto = _mapper.Map<ProjectPhaseDto>(phase);
            return Result<ProjectPhaseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving phase {PhaseId}", phaseId);
            return Result<ProjectPhaseDto>.Failure($"Error retrieving phase: {ex.Message}");
        }
    }

    public async Task<Result<ProjectPhaseDto>> AddPhaseToMasterPlanAsync(Guid masterPlanId, CreateProjectPhaseRequest request)
    {
        try
        {
            // Validate master plan exists
            var masterPlanExists = await _context.MasterPlans.AnyAsync(mp => mp.MasterPlanId == masterPlanId);
            if (!masterPlanExists)
                return Result<ProjectPhaseDto>.Failure($"Master plan with ID {masterPlanId} not found");

            // Validate phase order
            var validationResult = await ValidatePhaseOrder(masterPlanId, request.PhaseOrder);
            if (!validationResult.IsSuccess)
                return Result<ProjectPhaseDto>.Failure(validationResult.Message!);

            var phase = CreatePhaseEntity(masterPlanId, request);

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

            UpdatePhaseEntity(phase, request);

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

    public async Task<Result<bool>> UpdatePhaseProgressAsync(Guid phaseId, decimal completionPercentage, PhaseStatus status)
    {
        try
        {
            var phase = await _context.ProjectPhases.FindAsync(phaseId);
            if (phase == null)
                return Result<bool>.Failure($"Phase with ID {phaseId} not found");

            // Validate completion percentage
            if (completionPercentage < 0 || completionPercentage > 100)
                return Result<bool>.Failure("Completion percentage must be between 0 and 100");

            phase.CompletionPercentage = completionPercentage;
            phase.Status = status;
            phase.UpdatedAt = DateTime.UtcNow;

            // Set actual dates based on status
            UpdatePhaseStatusDates(phase, status);

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phase progress for phase {PhaseId}", phaseId);
            return Result<bool>.Failure($"Error updating phase progress: {ex.Message}");
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

    public async Task<Result<List<ProjectPhaseDto>>> GetDelayedPhasesAsync(Guid masterPlanId)
    {
        try
        {
            var delayedPhases = await _context.ProjectPhases
                .Where(p => p.MasterPlanId == masterPlanId &&
                           p.Status != PhaseStatus.Completed &&
                           p.PlannedEndDate < DateTime.UtcNow)
                .OrderBy(p => p.PlannedEndDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectPhaseDto>>(delayedPhases);
            return Result<List<ProjectPhaseDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delayed phases for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error retrieving delayed phases: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private async Task<Result<bool>> ValidatePhaseOrder(Guid masterPlanId, int phaseOrder)
    {
        var existingPhase = await _context.ProjectPhases
            .FirstOrDefaultAsync(p => p.MasterPlanId == masterPlanId && p.PhaseOrder == phaseOrder);

        if (existingPhase != null)
            return Result<bool>.Failure($"Phase order {phaseOrder} already exists in this master plan");

        return Result<bool>.Success(true);
    }

    private static ProjectPhase CreatePhaseEntity(Guid masterPlanId, CreateProjectPhaseRequest request)
    {
        return new ProjectPhase
        {
            PhaseId = Guid.NewGuid(),
            MasterPlanId = masterPlanId,
            PhaseName = request.PhaseName,
            Description = request.Description,
            PlannedStartDate = request.PlannedStartDate,
            PlannedEndDate = request.PlannedEndDate,
            PhaseOrder = request.PhaseOrder,
            WeightPercentage = request.WeightPercentage,
            Status = PhaseStatus.NotStarted,
            CompletionPercentage = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdatePhaseEntity(ProjectPhase phase, UpdateProjectPhaseRequest request)
    {
        if (!string.IsNullOrEmpty(request.PhaseName))
            phase.PhaseName = request.PhaseName;

        if (!string.IsNullOrEmpty(request.Description))
            phase.Description = request.Description;

        // StartDate and EndDate are not nullable in the request
        phase.PlannedStartDate = request.PlannedStartDate;
        phase.PlannedEndDate = request.PlannedEndDate;

        // WeightPercentage is not nullable in the request
        phase.WeightPercentage = request.WeightPercentage;

        phase.UpdatedAt = DateTime.UtcNow;
    }

    private static void UpdatePhaseStatusDates(ProjectPhase phase, PhaseStatus status)
    {
        switch (status)
        {
            case PhaseStatus.InProgress when phase.ActualStartDate == null:
                phase.ActualStartDate = DateTime.UtcNow;
                break;
            case PhaseStatus.Completed:
                phase.ActualEndDate ??= DateTime.UtcNow;
                phase.CompletionPercentage = 100;
                break;
        }
    }

    #endregion
}
