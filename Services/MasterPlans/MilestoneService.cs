using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace dotnet_rest_api.Services.MasterPlans;

/// <summary>
/// Service for managing project milestones within master plans
/// Extracted from the original large MasterPlanService for better maintainability
/// </summary>
public interface IMilestoneService
{
    Task<Result<List<ProjectMilestoneDto>>> GetMilestonesByMasterPlanAsync(Guid masterPlanId);
    Task<Result<ProjectMilestoneDto>> GetMilestoneByIdAsync(Guid milestoneId);
    Task<Result<ProjectMilestoneDto>> AddMilestoneToMasterPlanAsync(Guid masterPlanId, CreateProjectMilestoneRequest request);
    Task<Result<ProjectMilestoneDto>> UpdateMilestoneAsync(Guid milestoneId, UpdateProjectMilestoneRequest request);
    Task<Result<bool>> CompleteMilestoneAsync(Guid milestoneId, Guid completedById, string? evidence);
    Task<Result<bool>> DeleteMilestoneAsync(Guid milestoneId);
    Task<Result<List<ProjectMilestoneDto>>> GetUpcomingMilestonesAsync(Guid masterPlanId, int days = 30);
    Task<Result<List<ProjectMilestoneDto>>> GetOverdueMilestonesAsync(Guid masterPlanId);
}

public class MilestoneService : IMilestoneService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MilestoneService> _logger;

    public MilestoneService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<MilestoneService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ProjectMilestoneDto>>> GetMilestonesByMasterPlanAsync(Guid masterPlanId)
    {
        try
        {
            var milestones = await _context.ProjectMilestones
                .Where(m => m.MasterPlanId == masterPlanId)
                .OrderBy(m => m.TargetDate)
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

    public async Task<Result<ProjectMilestoneDto>> GetMilestoneByIdAsync(Guid milestoneId)
    {
        try
        {
            var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
            if (milestone == null)
                return Result<ProjectMilestoneDto>.Failure($"Milestone with ID {milestoneId} not found");

            var dto = _mapper.Map<ProjectMilestoneDto>(milestone);
            return Result<ProjectMilestoneDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving milestone {MilestoneId}", milestoneId);
            return Result<ProjectMilestoneDto>.Failure($"Error retrieving milestone: {ex.Message}");
        }
    }

    public async Task<Result<ProjectMilestoneDto>> AddMilestoneToMasterPlanAsync(Guid masterPlanId, CreateProjectMilestoneRequest request)
    {
        try
        {
            // Validate master plan exists
            var masterPlanExists = await _context.MasterPlans.AnyAsync(mp => mp.MasterPlanId == masterPlanId);
            if (!masterPlanExists)
                return Result<ProjectMilestoneDto>.Failure($"Master plan with ID {masterPlanId} not found");

            var milestone = CreateMilestoneEntity(masterPlanId, request);

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

            UpdateMilestoneEntity(milestone, request);

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

    public async Task<Result<bool>> CompleteMilestoneAsync(Guid milestoneId, Guid completedById, string? evidence)
    {
        try
        {
            var milestone = await _context.ProjectMilestones.FindAsync(milestoneId);
            if (milestone == null)
                return Result<bool>.Failure($"Milestone with ID {milestoneId} not found");

            if (milestone.Status == MilestoneStatus.Completed)
                return Result<bool>.Failure("Milestone is already completed");

            milestone.Status = MilestoneStatus.Completed;
            milestone.ActualDate = DateTime.UtcNow;
            milestone.VerifiedById = completedById;
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

    public async Task<Result<List<ProjectMilestoneDto>>> GetUpcomingMilestonesAsync(Guid masterPlanId, int days = 30)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(days);
            
            var upcomingMilestones = await _context.ProjectMilestones
                .Where(m => m.MasterPlanId == masterPlanId &&
                           m.Status == MilestoneStatus.Pending &&
                           m.TargetDate <= cutoffDate &&
                           m.TargetDate >= DateTime.UtcNow)
                .OrderBy(m => m.TargetDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectMilestoneDto>>(upcomingMilestones);
            return Result<List<ProjectMilestoneDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upcoming milestones for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving upcoming milestones: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectMilestoneDto>>> GetOverdueMilestonesAsync(Guid masterPlanId)
    {
        try
        {
            var overdueMilestones = await _context.ProjectMilestones
                .Where(m => m.MasterPlanId == masterPlanId &&
                           m.Status != MilestoneStatus.Completed &&
                           m.TargetDate < DateTime.UtcNow)
                .OrderBy(m => m.TargetDate)
                .ToListAsync();

            var dtos = _mapper.Map<List<ProjectMilestoneDto>>(overdueMilestones);
            return Result<List<ProjectMilestoneDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue milestones for master plan {MasterPlanId}", masterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving overdue milestones: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private static ProjectMilestone CreateMilestoneEntity(Guid masterPlanId, CreateProjectMilestoneRequest request)
    {
        return new ProjectMilestone
        {
            MilestoneId = Guid.NewGuid(),
            MasterPlanId = masterPlanId,
            MilestoneName = request.MilestoneName,
            Description = request.Description,
            TargetDate = request.TargetDate,
            Priority = request.Priority,
            Status = MilestoneStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateMilestoneEntity(ProjectMilestone milestone, UpdateProjectMilestoneRequest request)
    {
        if (!string.IsNullOrEmpty(request.MilestoneName))
            milestone.MilestoneName = request.MilestoneName;

        if (!string.IsNullOrEmpty(request.Description))
            milestone.Description = request.Description;

        // Update target date and priority directly since they are not nullable in the request
        milestone.TargetDate = request.TargetDate;
        milestone.Priority = request.Priority;

        milestone.UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}
