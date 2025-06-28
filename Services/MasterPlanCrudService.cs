using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for basic CRUD operations on master plans
/// Extracted from the original large MasterPlanService for better maintainability
/// </summary>
public interface IMasterPlanCrudService
{
    Task<Result<List<MasterPlanDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 50);
    Task<Result<MasterPlanDto>> GetByIdAsync(Guid masterPlanId);
    Task<Result<MasterPlanDto>> GetByProjectIdAsync(Guid projectId);
    Task<Result<MasterPlanDto>> CreateAsync(CreateMasterPlanRequest request, Guid createdById);
    Task<Result<MasterPlanDto>> UpdateAsync(Guid masterPlanId, UpdateMasterPlanRequest request);
    Task<Result<bool>> DeleteAsync(Guid masterPlanId);
    Task<Result<bool>> ApproveAsync(Guid masterPlanId, Guid approvedById, string? notes);
    Task<Result<bool>> ActivateAsync(Guid masterPlanId);
}

public class MasterPlanCrudService : IMasterPlanCrudService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MasterPlanCrudService> _logger;

    public MasterPlanCrudService(
        ApplicationDbContext context, 
        IMapper mapper, 
        ILogger<MasterPlanCrudService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<MasterPlanDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            var skip = (pageNumber - 1) * pageSize;

            var masterPlans = await _context.MasterPlans
                .Include(mp => mp.Project)
                .Include(mp => mp.CreatedBy)
                .Include(mp => mp.ApprovedBy)
                .OrderByDescending(mp => mp.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<MasterPlanDto>>(masterPlans);
            return Result<List<MasterPlanDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving master plans with pagination (page: {PageNumber}, size: {PageSize})", pageNumber, pageSize);
            return Result<List<MasterPlanDto>>.Failure($"Error retrieving master plans: {ex.Message}");
        }
    }

    public async Task<Result<MasterPlanDto>> GetByIdAsync(Guid masterPlanId)
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

    public async Task<Result<MasterPlanDto>> GetByProjectIdAsync(Guid projectId)
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

    public async Task<Result<MasterPlanDto>> CreateAsync(CreateMasterPlanRequest request, Guid createdById)
    {
        try
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                return Result<MasterPlanDto>.Failure($"Project with ID {request.ProjectId} not found");

            // Check if project already has a master plan
            var existingPlan = await _context.MasterPlans
                .FirstOrDefaultAsync(mp => mp.ProjectId == request.ProjectId);
            if (existingPlan != null)
                return Result<MasterPlanDto>.Failure("Project already has a master plan. Use update instead.");

            var masterPlan = CreateMasterPlanEntity(request, createdById);

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

    public async Task<Result<MasterPlanDto>> UpdateAsync(Guid masterPlanId, UpdateMasterPlanRequest request)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<MasterPlanDto>.Failure($"Master plan with ID {masterPlanId} not found");

            UpdateMasterPlanEntity(masterPlan, request);

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

    public async Task<Result<bool>> DeleteAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

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

    public async Task<Result<bool>> ApproveAsync(Guid masterPlanId, Guid approvedById, string? notes)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

            if (masterPlan.Status != MasterPlanStatus.Draft)
                return Result<bool>.Failure("Only draft master plans can be approved");

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

    public async Task<Result<bool>> ActivateAsync(Guid masterPlanId)
    {
        try
        {
            var masterPlan = await _context.MasterPlans.FindAsync(masterPlanId);
            if (masterPlan == null)
                return Result<bool>.Failure($"Master plan with ID {masterPlanId} not found");

            if (masterPlan.Status != MasterPlanStatus.Approved)
                return Result<bool>.Failure("Only approved master plans can be activated");

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

    #region Private Helper Methods

    private static MasterPlan CreateMasterPlanEntity(CreateMasterPlanRequest request, Guid createdById)
    {
        return new MasterPlan
        {
            MasterPlanId = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            PlanName = request.Title,
            Description = request.Description,
            PlannedStartDate = request.StartDate,
            PlannedEndDate = request.EndDate,
            TotalPlannedDays = (request.EndDate - request.StartDate).Days,
            TotalEstimatedBudget = request.Budget ?? 0,
            Version = 1,
            Status = MasterPlanStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedById = createdById
        };
    }

    private static void UpdateMasterPlanEntity(MasterPlan masterPlan, UpdateMasterPlanRequest request)
    {
        if (!string.IsNullOrEmpty(request.Title))
            masterPlan.PlanName = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            masterPlan.Description = request.Description;

        if (request.StartDate.HasValue)
            masterPlan.PlannedStartDate = request.StartDate.Value;

        if (request.EndDate.HasValue)
            masterPlan.PlannedEndDate = request.EndDate.Value;

        if (request.Budget.HasValue)
            masterPlan.TotalEstimatedBudget = request.Budget.Value;

        // Recalculate total days if dates changed
        if (request.StartDate.HasValue || request.EndDate.HasValue)
            masterPlan.TotalPlannedDays = (masterPlan.PlannedEndDate - masterPlan.PlannedStartDate).Days;

        masterPlan.UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}
