using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace dotnet_rest_api.Services;

/// <summary>
/// Improved ProjectService with reduced complexity and better separation of concerns
/// Breaks down the original 601-line service into focused methods
/// </summary>
public class ImprovedProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;
    private readonly IMapper _mapper;
    private readonly ILogger<ImprovedProjectService> _logger;

    public ImprovedProjectService(
        ApplicationDbContext context, 
        IQueryService queryService,
        IMapper mapper,
        ILogger<ImprovedProjectService> logger)
    {
        _context = context;
        _queryService = queryService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
    {
        try
        {
            _logger.LogInformation("Retrieving projects with parameters: {@Parameters}", parameters);

            var query = BuildProjectQuery(parameters);
            var totalCount = await query.CountAsync();

            var projects = await ExecuteProjectQuery(query, parameters);
            var projectDtos = MapToProjectDtos(projects);

            var result = CreatePagedResult(projectDtos, totalCount, parameters);
            return ServiceResult<EnhancedPagedResult<ProjectDto>>.SuccessResult(result, "Projects retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return ServiceResult<EnhancedPagedResult<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving project {ProjectId}", id);

            var project = await GetProjectWithIncludesAsync(id);
            if (project == null)
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");

            var projectDto = MapToProjectDto(project);
            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return ServiceResult<ProjectDto>.ErrorResult($"Error retrieving project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Creating project {ProjectName}", request.ProjectName);

            // Validate project manager exists
            var validationResult = await ValidateProjectManagerAsync(request.ProjectManagerId);
            if (!validationResult.IsSuccess)
                return ServiceResult<ProjectDto>.ErrorResult(validationResult.Message);

            var project = CreateProjectEntity(request);
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var createdProject = await GetProjectWithIncludesAsync(project.ProjectId);
            var projectDto = MapToProjectDto(createdProject!);

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return ServiceResult<ProjectDto>.ErrorResult($"Error creating project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        try
        {
            _logger.LogInformation("Updating project {ProjectId}", id);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");

            // Validate project manager
            var validationResult = await ValidateProjectManagerAsync(request.ProjectManagerId);
            if (!validationResult.IsSuccess)
                return ServiceResult<ProjectDto>.ErrorResult(validationResult.Message);

            UpdateProjectEntity(project, request);
            await _context.SaveChangesAsync();

            var updatedProject = await GetProjectWithIncludesAsync(id);
            var projectDto = MapToProjectDto(updatedProject!);

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return ServiceResult<ProjectDto>.ErrorResult($"Error updating project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting project {ProjectId}", id);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return ServiceResult<bool>.ErrorResult("Project not found");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Project deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return ServiceResult<bool>.ErrorResult($"Error deleting project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber, int pageSize, Guid? managerId)
    {
        try
        {
            var parameters = new ProjectQueryParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ManagerId = managerId
            };

            var result = await GetProjectsAsync(parameters);
            
            if (result.IsSuccess && result.Data != null)
            {
                var legacyResult = new PagedResult<ProjectDto>
                {
                    Items = result.Data.Items,
                    PageNumber = result.Data.PageNumber,
                    PageSize = result.Data.PageSize,
                    TotalCount = result.Data.TotalCount
                };
                return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(legacyResult);
            }

            return ServiceResult<PagedResult<ProjectDto>>.ErrorResult(result.Message ?? "Failed to retrieve projects");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects legacy");
            return ServiceResult<PagedResult<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return ServiceResult<ProjectDto>.ErrorResult($"Project with ID {id} not found");

            // Apply patch operations using the correct property names
            if (!string.IsNullOrEmpty(request.ProjectName))
                project.ProjectName = request.ProjectName;

            if (!string.IsNullOrEmpty(request.Address))
                project.Address = request.Address;

            if (!string.IsNullOrEmpty(request.ClientInfo))
                project.ClientInfo = request.ClientInfo;

            if (!string.IsNullOrEmpty(request.Status))
                project.Status = Enum.Parse<ProjectStatus>(request.Status);

            if (request.StartDate.HasValue)
                project.StartDate = request.StartDate.Value;

            if (request.EstimatedEndDate.HasValue)
                project.EstimatedEndDate = request.EstimatedEndDate;

            if (request.ActualEndDate.HasValue)
                project.ActualEndDate = request.ActualEndDate;

            if (request.ProjectManagerId.HasValue)
                project.ProjectManagerId = request.ProjectManagerId.Value;

            // Update the UpdatedAt property if it exists
            if (project.UpdatedAt != null)
                project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var projectDto = _mapper.Map<ProjectDto>(project);
            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching project {ProjectId}", id);
            return ServiceResult<ProjectDto>.ErrorResult($"Error updating project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.Projects
                .Where(p => p.ManagerId == userId || p.CreatedById == userId)
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = _mapper.Map<List<ProjectDto>>(projects);

            var result = new PagedResult<ProjectDto>
            {
                Items = projectDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user projects for user {UserId}", userId);
            return ServiceResult<PagedResult<ProjectDto>>.ErrorResult($"Error retrieving user projects: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private IQueryable<Project> BuildProjectQuery(ProjectQueryParameters parameters)
    {
        var query = _context.Projects
            .Include(p => p.ProjectManager)
            .ThenInclude(pm => pm.Role)
            .AsQueryable();

        query = ApplyFilters(query, parameters);
        query = ApplySorting(query, parameters);

        return query;
    }

    private static IQueryable<Project> ApplyFilters(IQueryable<Project> query, ProjectQueryParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.ProjectName))
            query = query.Where(p => p.ProjectName.Contains(parameters.ProjectName));

        if (!string.IsNullOrEmpty(parameters.Status) && 
            Enum.TryParse<ProjectStatus>(parameters.Status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (parameters.ManagerId.HasValue)
            query = query.Where(p => p.ProjectManagerId == parameters.ManagerId.Value);

        if (!string.IsNullOrEmpty(parameters.Address))
            query = query.Where(p => p.Address.Contains(parameters.Address));

        if (!string.IsNullOrEmpty(parameters.ClientInfo))
            query = query.Where(p => p.ClientInfo.Contains(parameters.ClientInfo));

        if (parameters.StartDateAfter.HasValue)
            query = query.Where(p => p.StartDate >= parameters.StartDateAfter.Value);

        if (parameters.StartDateBefore.HasValue)
            query = query.Where(p => p.StartDate <= parameters.StartDateBefore.Value);

        return query;
    }

    private static IQueryable<Project> ApplySorting(IQueryable<Project> query, ProjectQueryParameters parameters)
    {
        return (parameters.SortBy?.ToLower()) switch
        {
            "name" => parameters.SortOrder?.ToLower() == "desc" 
                ? query.OrderByDescending(p => p.ProjectName)
                : query.OrderBy(p => p.ProjectName),
            "startdate" => parameters.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.StartDate)
                : query.OrderBy(p => p.StartDate),
            "status" => parameters.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),
            _ => query.OrderBy(p => p.ProjectName) // Default sorting
        };
    }

    private static async Task<List<Project>> ExecuteProjectQuery(IQueryable<Project> query, ProjectQueryParameters parameters)
    {
        return await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
    }

    private List<ProjectDto> MapToProjectDtos(List<Project> projects)
    {
        return projects.Select(MapToProjectDto).ToList();
    }

    private ProjectDto MapToProjectDto(Project project)
    {
        return new ProjectDto
        {
            ProjectId = project.ProjectId,
            ProjectName = project.ProjectName,
            Address = project.Address,
            ClientInfo = project.ClientInfo,
            Status = project.Status.ToString(),
            StartDate = project.StartDate,
            EstimatedEndDate = project.EstimatedEndDate,
            ActualEndDate = project.ActualEndDate,
            ProjectManager = MapToUserDto(project.ProjectManager),
            Team = project.Team,
            ConnectionType = project.ConnectionType,
            ConnectionNotes = project.ConnectionNotes,
            TotalCapacityKw = project.TotalCapacityKw,
            PvModuleCount = project.PvModuleCount,
            FtsValue = project.FtsValue,
            RevenueValue = project.RevenueValue,
            PqmValue = project.PqmValue,
            CreatedAt = project.CreatedAt
        };
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role.RoleName,
            IsActive = user.IsActive
        };
    }

    private static EnhancedPagedResult<ProjectDto> CreatePagedResult(
        List<ProjectDto> projects, 
        int totalCount, 
        ProjectQueryParameters parameters)
    {
        var result = new EnhancedPagedResult<ProjectDto>
        {
            Items = projects,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
        
        return result;
    }

    private async Task<Project?> GetProjectWithIncludesAsync(Guid projectId)
    {
        return await _context.Projects
            .Include(p => p.ProjectManager)
            .ThenInclude(pm => pm.Role)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);
    }

    private async Task<ServiceResult<bool>> ValidateProjectManagerAsync(Guid projectManagerId)
    {
        var projectManager = await _context.Users.FindAsync(projectManagerId);
        return projectManager == null 
            ? ServiceResult<bool>.ErrorResult("Project manager not found")
            : ServiceResult<bool>.SuccessResult(true);
    }

    private static Project CreateProjectEntity(CreateProjectRequest request)
    {
        return new Project
        {
            ProjectId = Guid.NewGuid(),
            ProjectName = request.ProjectName,
            Address = request.Address,
            ClientInfo = request.ClientInfo,
            Status = ProjectStatus.Planning,
            StartDate = request.StartDate,
            EstimatedEndDate = request.EstimatedEndDate,
            ProjectManagerId = request.ProjectManagerId,
            Team = request.Team,
            ConnectionType = request.ConnectionType,
            ConnectionNotes = request.ConnectionNotes,
            TotalCapacityKw = request.TotalCapacityKw,
            PvModuleCount = request.PvModuleCount,
            FtsValue = request.FtsValue,
            RevenueValue = request.RevenueValue,
            PqmValue = request.PqmValue,
            // Map equipment details
            Inverter125kw = request.EquipmentDetails?.Inverter125kw ?? 0,
            Inverter80kw = request.EquipmentDetails?.Inverter80kw ?? 0,
            Inverter60kw = request.EquipmentDetails?.Inverter60kw ?? 0,
            Inverter40kw = request.EquipmentDetails?.Inverter40kw ?? 0,
            // Map location coordinates
            Latitude = request.LocationCoordinates?.Latitude,
            Longitude = request.LocationCoordinates?.Longitude,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateProjectEntity(Project project, UpdateProjectRequest request)
    {
        if (!string.IsNullOrEmpty(request.ProjectName))
            project.ProjectName = request.ProjectName;

        if (!string.IsNullOrEmpty(request.Address))
            project.Address = request.Address;

        if (!string.IsNullOrEmpty(request.ClientInfo))
            project.ClientInfo = request.ClientInfo;

        if (!string.IsNullOrEmpty(request.Status))
            project.Status = Enum.Parse<ProjectStatus>(request.Status);

        // StartDate is not nullable in UpdateProjectRequest, so set it directly
        project.StartDate = request.StartDate;

        if (request.EstimatedEndDate.HasValue)
            project.EstimatedEndDate = request.EstimatedEndDate;

        if (request.ActualEndDate.HasValue)
            project.ActualEndDate = request.ActualEndDate;

        // ProjectManagerId is not nullable in UpdateProjectRequest, so set it directly
        project.ProjectManagerId = request.ProjectManagerId;

        // Update the UpdatedAt property if it exists
        if (project.UpdatedAt != null)
            project.UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}
