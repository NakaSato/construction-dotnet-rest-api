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

    public async Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
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
        var totalCount = await query.CountAsync();

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
            .ToListAsync();

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

    public async Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectManager)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
        {
            return ServiceResult<ProjectDto>.ErrorResult("Project not found");
        }

        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        // Use a default user if none provided (for seed scripts) usually handled by controller but here we can support overload
        return await CreateProjectAsync(request, "system");
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string userName)
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
            await _context.SaveChangesAsync();

            // Fetch created project with relationships
            var createdProject = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);

            var dto = _mapper.Map<ProjectDto>(createdProject);
            return ServiceResult<ProjectDto>.SuccessResult(dto, "Project created successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProjectDto>.ErrorResult($"Failed to create project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        return await UpdateProjectAsync(id, request, "system");
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string userName)
    {
        var project = await _context.Projects.FindAsync(id);
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

        await _context.SaveChangesAsync();
        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto, "Project updated successfully");
    }

    public async Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string userName)
    {
         var project = await _context.Projects.FindAsync(id);
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
        await _context.SaveChangesAsync();

        var dto = _mapper.Map<ProjectDto>(project);
        return ServiceResult<ProjectDto>.SuccessResult(dto, "Project patched successfully");
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id)
    {
        return await DeleteProjectAsync(id, "system");
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, string userName)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return ServiceResult<bool>.ErrorResult("Project not found");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.SuccessResult(true, "Project deleted successfully");
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _context.Projects
            .Include(p => p.ProjectManager)
            .Where(p => p.ProjectManagerId == userId || p.Tasks.Any(t => t.AssignedTechnicianId == userId));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
}
