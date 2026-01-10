using Microsoft.EntityFrameworkCore;
using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services.Projects;

/// <summary>
/// Implementation of IProjectService using EF Core
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ProjectService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Include(p => p.ProjectManager)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.ToLower();
            query = query.Where(p => 
                p.ProjectName.ToLower().Contains(searchTerm) || 
                p.Address.ToLower().Contains(searchTerm) ||
                p.ClientInfo.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            if (Enum.TryParse<ProjectStatus>(parameters.Status, true, out var status))
            {
                query = query.Where(p => p.Status == status);
            }
        }

        // Count total items
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        bool sortDescending = parameters.SortOrder?.ToLower() == "desc";
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            // Simple sorting implementation - can be extended
            query = parameters.SortBy.ToLower() switch
            {
                "projectname" => sortDescending ? query.OrderByDescending(p => p.ProjectName) : query.OrderBy(p => p.ProjectName),
                "startdate" => sortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
                "createdat" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        // Apply pagination
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        var projectDtos = _mapper.Map<List<ProjectDto>>(items);

        var result = new EnhancedPagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };

        return ServiceResult<EnhancedPagedResult<ProjectDto>>.SuccessResult(result, "Projects retrieved successfully");
    }

    public async Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectManager)
            .FirstOrDefaultAsync(p => p.ProjectId == id, cancellationToken);

        if (project == null)
        {
            return ServiceResult<ProjectDto>.ErrorResult("Project not found");
        }

        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await CreateProjectAsync(request, "system", cancellationToken);
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = _mapper.Map<Project>(request);
            
            // Set default values if not provided
            project.Status = ProjectStatus.Planning;
            project.CreatedAt = DateTime.UtcNow;

            // Flatten DTO properties to Model properties if AutoMapper doesn't handle them automatically
            // (Assumes AutoMapper configuration is set up, but let's manual map complex objects to flatness if needed)
            // Based on Project.cs, properties like Inverter125kw are flat on Project entity but nested in DTO
            if (request.EquipmentDetails != null)
            {
                project.Inverter125kw = request.EquipmentDetails.Inverter125kw;
                project.Inverter80kw = request.EquipmentDetails.Inverter80kw;
                project.Inverter60kw = request.EquipmentDetails.Inverter60kw;
                project.Inverter40kw = request.EquipmentDetails.Inverter40kw;
            }

            if (request.LocationCoordinates != null)
            {
                project.Latitude = request.LocationCoordinates.Latitude;
                project.Longitude = request.LocationCoordinates.Longitude;
            }

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch created project with relationships
            var createdProject = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId, cancellationToken);

            var dto = _mapper.Map<ProjectDto>(createdProject);
            return ServiceResult<ProjectDto>.SuccessResult(dto, "Project created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProjectDto>.ErrorResult($"Failed to create project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await UpdateProjectAsync(id, request, "system", cancellationToken);
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userName, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project == null)
        {
            return ServiceResult<ProjectDto>.ErrorResult("Project not found");
        }

        _mapper.Map(request, project);
        project.UpdatedAt = DateTime.UtcNow;
        
        // Handle enum conversion if needed, AutoMapper usually handles string -> enum if names match
        if (Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            project.Status = status;
        }

        await _context.SaveChangesAsync(cancellationToken);
        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto, "Project updated successfully");
    }

    public async Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string userName, CancellationToken cancellationToken = default)
    {
         var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project == null)
        {
            return ServiceResult<ProjectDto>.ErrorResult("Project not found");
        }

        if (request.ProjectName != null) project.ProjectName = request.ProjectName;
        if (request.Address != null) project.Address = request.Address;
        if (request.ClientInfo != null) project.ClientInfo = request.ClientInfo;
        
        if (request.Status != null && Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            project.Status = status;
        }

        project.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto, "Project patched successfully");
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DeleteProjectAsync(id, "system", cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, string userName, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project == null)
        {
            return ServiceResult<bool>.ErrorResult("Project not found");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.SuccessResult(true, "Project deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects
            .Include(p => p.ProjectManager)
            .Where(p => p.ProjectManagerId == userId || p.Tasks.Any(t => t.AssignedTechnicianId == userId));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<ProjectDto>>(items);

        var result = new PagedResult<ProjectDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result);
    }

    // Milestone Management Implementation
    
    public async Task<ServiceResult<PerformanceMilestoneDto>> AddMilestoneAsync(Guid projectId, CreateProjectMilestoneRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure project exists
            var project = await _context.Projects
                .Include(p => p.MasterPlan)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
                
            if (project == null)
                return ServiceResult<PerformanceMilestoneDto>.ErrorResult("Project not found");
                
            if (project.MasterPlan == null)
            {
                var masterPlan = new MasterPlan
                {
                    MasterPlanId = Guid.NewGuid(),
                    ProjectId = projectId,
                    PlanName = $"{project.ProjectName} Master Plan",
                    Status = MasterPlanStatus.Draft,
                    CreatedById = project.ProjectManagerId ?? Guid.Empty, // Handle nullable Guid
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.MasterPlans.Add(masterPlan);
                await _context.SaveChangesAsync(cancellationToken);
                
                // Reload project to get the relationship
                project = await _context.Projects
                    .Include(p => p.MasterPlan)
                    .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
            }
            
            var milestone = new ProjectMilestone
            {
                MilestoneId = Guid.NewGuid(),
                MasterPlanId = project!.MasterPlan!.MasterPlanId,
                MilestoneName = request.Name,
                Description = request.Description,
                TargetDate = request.DueDate,
                PlannedDate = request.DueDate,
                Status = MilestoneStatus.Pending,
                Importance = request.Priority,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.ProjectMilestones.Add(milestone);
            await _context.SaveChangesAsync(cancellationToken);
            
            var dto = new PerformanceMilestoneDto
            {
                MilestoneId = milestone.MilestoneId,
                Title = milestone.MilestoneName,
                TargetDate = milestone.TargetDate,
                ActualDate = milestone.ActualDate,
                Status = milestone.Status.ToString(),
                VarianceDays = 0 // New milestone
            };
            
            return ServiceResult<PerformanceMilestoneDto>.SuccessResult(dto, "Milestone created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PerformanceMilestoneDto>.ErrorResult($"Failed to create milestone: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<PerformanceMilestoneDto>>> GetProjectMilestonesAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.MasterPlan)
                .ThenInclude(mp => mp.Milestones)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId, cancellationToken);
                
            if (project == null)
                return ServiceResult<List<PerformanceMilestoneDto>>.ErrorResult("Project not found");
                
            if (project.MasterPlan == null || project.MasterPlan.Milestones == null)
                return ServiceResult<List<PerformanceMilestoneDto>>.SuccessResult(new List<PerformanceMilestoneDto>());
                
            var dtos = project.MasterPlan.Milestones.Select(m => new PerformanceMilestoneDto
            {
                MilestoneId = m.MilestoneId,
                Title = m.MilestoneName,
                TargetDate = m.TargetDate,
                ActualDate = m.ActualDate,
                Status = m.Status.ToString(),
                VarianceDays = m.ActualDate.HasValue 
                    ? (int)(m.ActualDate.Value - m.TargetDate).TotalDays 
                    : (m.TargetDate < DateTime.UtcNow && m.Status != MilestoneStatus.Completed ? (int)(DateTime.UtcNow - m.TargetDate).TotalDays : 0)
            }).OrderBy(m => m.TargetDate).ToList();
            
            return ServiceResult<List<PerformanceMilestoneDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<PerformanceMilestoneDto>>.ErrorResult($"Failed to retrieve milestones: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PerformanceMilestoneDto>> UpdateMilestoneAsync(Guid projectId, Guid milestoneId, UpdateProjectMilestoneRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestone = await _context.ProjectMilestones
                .Include(m => m.MasterPlan)
                .FirstOrDefaultAsync(m => m.MilestoneId == milestoneId && m.MasterPlan.ProjectId == projectId, cancellationToken);
                
            if (milestone == null)
                return ServiceResult<PerformanceMilestoneDto>.ErrorResult("Milestone not found");
                
            milestone.MilestoneName = request.Name;
            milestone.Description = request.Description;
            milestone.TargetDate = request.DueDate;
            milestone.PlannedDate = request.DueDate;
            milestone.Importance = request.Priority;
            milestone.UpdatedAt = DateTime.UtcNow;

            if (request.Status.HasValue)
            {
                milestone.Status = request.Status.Value;
            }

            if (request.ActualDate.HasValue)
            {
                milestone.ActualDate = request.ActualDate.Value;
            }
            else if (milestone.Status == MilestoneStatus.Completed && !milestone.ActualDate.HasValue)
            {
                // Auto-set actual date if completed and not provided
                milestone.ActualDate = DateTime.UtcNow; 
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            var dto = new PerformanceMilestoneDto
            {
                MilestoneId = milestone.MilestoneId,
                Title = milestone.MilestoneName,
                TargetDate = milestone.TargetDate,
                ActualDate = milestone.ActualDate,
                Status = milestone.Status.ToString(),
                VarianceDays = milestone.ActualDate.HasValue 
                    ? (int)(milestone.ActualDate.Value - milestone.TargetDate).TotalDays 
                    : 0
            };
            
            return ServiceResult<PerformanceMilestoneDto>.SuccessResult(dto, "Milestone updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PerformanceMilestoneDto>.ErrorResult($"Failed to update milestone: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteMilestoneAsync(Guid projectId, Guid milestoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestone = await _context.ProjectMilestones
                .Include(m => m.MasterPlan)
                .FirstOrDefaultAsync(m => m.MilestoneId == milestoneId && m.MasterPlan.ProjectId == projectId, cancellationToken);
                
            if (milestone == null)
                return ServiceResult<bool>.ErrorResult("Milestone not found");
                
            _context.ProjectMilestones.Remove(milestone);
            await _context.SaveChangesAsync(cancellationToken);
            
            return ServiceResult<bool>.SuccessResult(true, "Milestone deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Failed to delete milestone: {ex.Message}");
        }
    }
}
