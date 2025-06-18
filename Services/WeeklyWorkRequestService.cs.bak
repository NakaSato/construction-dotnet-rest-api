using Microsoft.EntityFrameworkCore;
using AutoMapper;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Data;
using dotnet_rest_api.Common;
using System.Text.Json;

namespace dotnet_rest_api.Services;

public class WeeklyWorkRequestService : IWeeklyWorkRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WeeklyWorkRequestService> _logger;
    private readonly IQueryService _queryService;

    public WeeklyWorkRequestService(
        ApplicationDbContext context,
        IMapper mapper,
        ILogger<WeeklyWorkRequestService> logger,
        IQueryService queryService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _queryService = queryService;
    }

    public async Task<ApiResponse<WeeklyWorkRequestDto>> GetWeeklyWorkRequestByIdAsync(Guid requestId)
    {
        try
        {
            var request = await _context.WeeklyWorkRequests
                .Include(w => w.Project)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyRequestId == requestId);

            if (request == null)
            {
                return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                    "Weekly work request not found");
            }

            var dto = _mapper.Map<WeeklyWorkRequestDto>(request);
            return ApiResponse<WeeklyWorkRequestDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weekly work request {RequestId}", requestId);
            return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                "An error occurred while retrieving the weekly work request");
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetWeeklyWorkRequestsAsync(
        WeeklyWorkRequestQueryParameters parameters)
    {
        try
        {
            var query = _context.WeeklyWorkRequests
                .Include(w => w.Project)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ApprovedByUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status) && 
                Enum.TryParse<WeeklyRequestStatus>(parameters.Status, out var status))
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

            if (parameters.RequestedById.HasValue)
            {
                query = query.Where(w => w.RequestedById == parameters.RequestedById.Value);
            }

            if (parameters.MinEstimatedHours.HasValue)
            {
                query = query.Where(w => w.EstimatedHours >= parameters.MinEstimatedHours.Value);
            }

            if (parameters.MaxEstimatedHours.HasValue)
            {
                query = query.Where(w => w.EstimatedHours <= parameters.MaxEstimatedHours.Value);
            }

            // Apply generic filters
            query = _queryService.ApplyFilters(query, parameters.Filters);

            // Apply sorting
            query = _queryService.ApplySorting(query, parameters.SortBy, parameters.SortOrder);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var requests = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<WeeklyWorkRequestDto>>(requests);

            var result = new EnhancedPagedResult<WeeklyWorkRequestDto>
            {
                Items = dtos,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };

            return ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weekly work requests");
            return ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>.ErrorResponse(
                "An error occurred while retrieving weekly work requests");
        }
    }

    public async Task<ApiResponse<WeeklyWorkRequestDto>> CreateWeeklyWorkRequestAsync(CreateWeeklyWorkRequestDto request)
    {
        try
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse("Project not found");
            }

            // Validate requested by user exists
            var user = await _context.Users.FindAsync(request.RequestedById);
            if (user == null)
            {
                return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse("Requested by user not found");
            }

            var weeklyRequest = new WeeklyWorkRequest
            {
                WeeklyRequestId = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                WeekStartDate = request.WeekStartDate,
                OverallGoals = request.OverallGoals,
                KeyTasks = JsonSerializer.Serialize(request.KeyTasks ?? new List<string>()),
                PersonnelForecast = request.PersonnelForecast,
                MajorEquipment = request.MajorEquipment,
                CriticalMaterials = request.CriticalMaterials,
                RequestedById = request.RequestedById,
                EstimatedHours = request.EstimatedHours,
                Priority = request.Priority,
                Type = request.Type,
                CreatedAt = DateTime.UtcNow
            };

            _context.WeeklyWorkRequests.Add(weeklyRequest);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdRequest = await _context.WeeklyWorkRequests
                .Include(w => w.Project)
                .Include(w => w.RequestedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyRequestId == weeklyRequest.WeeklyRequestId);

            var dto = _mapper.Map<WeeklyWorkRequestDto>(createdRequest);
            return ApiResponse<WeeklyWorkRequestDto>.SuccessResponse(dto, "Weekly work request created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating weekly work request");
            return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                "An error occurred while creating the weekly work request");
        }
    }

    public async Task<ApiResponse<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestAsync(
        Guid requestId, UpdateWeeklyWorkRequestDto request)
    {
        try
        {
            var weeklyRequest = await _context.WeeklyWorkRequests
                .Include(w => w.Project)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyRequestId == requestId);

            if (weeklyRequest == null)
            {
                return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                    "Weekly work request not found");
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.OverallGoals))
                weeklyRequest.OverallGoals = request.OverallGoals;

            if (request.KeyTasks != null)
                weeklyRequest.KeyTasks = JsonSerializer.Serialize(request.KeyTasks);

            if (request.PersonnelForecast != null)
                weeklyRequest.PersonnelForecast = request.PersonnelForecast;

            if (request.MajorEquipment != null)
                weeklyRequest.MajorEquipment = request.MajorEquipment;

            if (request.CriticalMaterials != null)
                weeklyRequest.CriticalMaterials = request.CriticalMaterials;

            if (request.EstimatedHours.HasValue)
                weeklyRequest.EstimatedHours = request.EstimatedHours.Value;

            if (!string.IsNullOrEmpty(request.Priority))
                weeklyRequest.Priority = request.Priority;

            if (!string.IsNullOrEmpty(request.Type))
                weeklyRequest.Type = request.Type;

            weeklyRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<WeeklyWorkRequestDto>(weeklyRequest);
            return ApiResponse<WeeklyWorkRequestDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly work request {RequestId}", requestId);
            return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                "An error occurred while updating the weekly work request");
        }
    }

    public async Task<ApiResponse<WeeklyWorkRequestDto>> UpdateWeeklyWorkRequestStatusAsync(
        Guid requestId, WeeklyRequestStatus status)
    {
        try
        {
            var weeklyRequest = await _context.WeeklyWorkRequests
                .Include(w => w.Project)
                .Include(w => w.RequestedByUser)
                .Include(w => w.ApprovedByUser)
                .FirstOrDefaultAsync(w => w.WeeklyRequestId == requestId);

            if (weeklyRequest == null)
            {
                return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                    "Weekly work request not found");
            }

            weeklyRequest.Status = status;
            weeklyRequest.UpdatedAt = DateTime.UtcNow;

            // Set approval timestamp when approved
            if (status == WeeklyRequestStatus.Approved)
            {
                weeklyRequest.ApprovedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<WeeklyWorkRequestDto>(weeklyRequest);
            return ApiResponse<WeeklyWorkRequestDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weekly work request status {RequestId}", requestId);
            return ApiResponse<WeeklyWorkRequestDto>.ErrorResponse(
                "An error occurred while updating the weekly work request status");
        }
    }

    public async Task<ApiResponse<bool>> DeleteWeeklyWorkRequestAsync(Guid requestId)
    {
        try
        {
            var weeklyRequest = await _context.WeeklyWorkRequests.FindAsync(requestId);
            if (weeklyRequest == null)
            {
                return ApiResponse<bool>.ErrorResponse("Weekly work request not found");
            }

            _context.WeeklyWorkRequests.Remove(weeklyRequest);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting weekly work request {RequestId}", requestId);
            return ApiResponse<bool>.ErrorResponse(
                "An error occurred while deleting the weekly work request");
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>> GetProjectWeeklyWorkRequestsAsync(
        Guid projectId, WeeklyWorkRequestQueryParameters parameters)
    {
        // Set the project filter and delegate to the main method
        parameters.ProjectId = projectId;
        return await GetWeeklyWorkRequestsAsync(parameters);
    }
}
