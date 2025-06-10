using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public ProjectService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(Guid projectId)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return Result<ProjectDto>.NotFound("Project", projectId.ToString());
            }

            var projectDto = MapToDto(project);
            return Result<ProjectDto>.Success(projectDto, "Project retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<ProjectDto>.ServerError($"An error occurred while retrieving the project: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<ProjectDto>>> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .AsQueryable();

            if (managerId.HasValue)
            {
                query = query.Where(p => p.ProjectManagerId == managerId.Value);
            }

            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = projects.Select(MapToDto).ToList();

            var result = new PagedResult<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Result<PagedResult<ProjectDto>>.Success(result, "Projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ProjectDto>>.ServerError($"An error occurred while retrieving projects: {ex.Message}");
        }
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            // Validate project manager exists
            var manager = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId && u.IsActive);

            if (manager == null)
            {
                return Result<ProjectDto>.Failure("Project manager not found or inactive", null, ResultErrorType.Validation);
            }

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = request.ProjectName,
                Address = request.Address,
                ClientInfo = request.ClientInfo,
                Status = ProjectStatus.Planning,
                StartDate = request.StartDate,
                EstimatedEndDate = request.EstimatedEndDate,
                ProjectManagerId = request.ProjectManagerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstAsync(p => p.ProjectId == project.ProjectId);

            var projectDto = MapToDto(project);
            return Result<ProjectDto>.Success(projectDto, "Project created successfully");
        }
        catch (Exception ex)
        {
            return Result<ProjectDto>.ServerError($"An error occurred while creating the project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return Result<ProjectDto>.NotFound("Project", projectId.ToString());
            }

            // Validate project manager exists
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId && u.IsActive);

            if (manager == null)
            {
                return Result<ProjectDto>.Failure("Project manager not found or inactive", null, ResultErrorType.Validation);
            }

            // Parse status
            if (!Enum.TryParse<ProjectStatus>(request.Status, out var status))
            {
                return Result<ProjectDto>.Failure("Invalid project status", null, ResultErrorType.Validation);
            }

            project.ProjectName = request.ProjectName;
            project.Address = request.Address;
            project.ClientInfo = request.ClientInfo;
            project.Status = status;
            project.StartDate = request.StartDate;
            project.EstimatedEndDate = request.EstimatedEndDate;
            project.ActualEndDate = request.ActualEndDate;
            project.ProjectManagerId = request.ProjectManagerId;

            await _context.SaveChangesAsync();

            // Reload to get updated manager info
            project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstAsync(p => p.ProjectId == projectId);

            var projectDto = MapToDto(project);
            return Result<ProjectDto>.Success(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ProjectDto>.ServerError($"An error occurred while updating the project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectDto>> PatchProjectAsync(Guid projectId, PatchProjectRequest request)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return Result<ProjectDto>.NotFound("Project", projectId.ToString());
            }

            // Only update provided fields
            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                project.ProjectName = request.ProjectName;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                project.Address = request.Address;
            }

            if (!string.IsNullOrEmpty(request.ClientInfo))
            {
                project.ClientInfo = request.ClientInfo;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (!Enum.TryParse<ProjectStatus>(request.Status, out var status))
                {
                    return Result<ProjectDto>.Failure("Invalid project status", null, ResultErrorType.Validation);
                }
                project.Status = status;
            }

            if (request.StartDate.HasValue)
            {
                project.StartDate = request.StartDate.Value;
            }

            if (request.EstimatedEndDate.HasValue)
            {
                project.EstimatedEndDate = request.EstimatedEndDate.Value;
            }

            if (request.ActualEndDate.HasValue)
            {
                project.ActualEndDate = request.ActualEndDate.Value;
            }

            if (request.ProjectManagerId.HasValue)
            {
                // Validate project manager exists
                var manager = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId.Value && u.IsActive);

                if (manager == null)
                {
                    return Result<ProjectDto>.Failure("Project manager not found or inactive", null, ResultErrorType.Validation);
                }
                project.ProjectManagerId = request.ProjectManagerId.Value;
            }

            await _context.SaveChangesAsync();

            // Reload to get updated manager info
            project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .FirstAsync(p => p.ProjectId == projectId);

            var projectDto = MapToDto(project);
            return Result<ProjectDto>.Success(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ProjectDto>.ServerError($"An error occurred while updating the project: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteProjectAsync(Guid projectId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return Result<bool>.NotFound("Project", projectId.ToString());
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true, "Project deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.ServerError($"An error occurred while deleting the project: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .Where(p => p.ProjectManagerId == userId || p.Tasks.Any(t => t.AssignedTechnicianId == userId));

            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = projects.Select(MapToDto).ToList();

            var result = new PagedResult<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Result<PagedResult<ProjectDto>>.Success(result, "User projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ProjectDto>>.ServerError($"An error occurred while retrieving user projects: {ex.Message}");
        }
    }

    public async Task<Result<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .AsQueryable();

            // Apply specific filters based on query parameters
            query = ApplyProjectFilters(query, parameters);
            
            // Execute the query first to materialize the data, avoiding the MapToDto memory leak issue
            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // Map to DTOs after materialization
            var projectDtos = projects.Select(MapToDto).ToList();
            
            // Create the enhanced paged result
            var result = new EnhancedPagedResult<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder,
                RequestedFields = string.IsNullOrEmpty(parameters.Fields) 
                    ? new List<string>() 
                    : parameters.Fields.Split(',').Select(f => f.Trim()).ToList(),
                Metadata = new QueryMetadata
                {
                    ExecutionTime = TimeSpan.FromMilliseconds(0), // Can be enhanced with timing
                    FiltersApplied = DetermineFiltersApplied(parameters),
                    QueryComplexity = DetermineQueryComplexity(parameters),
                    QueryExecutedAt = DateTime.UtcNow,
                    CacheStatus = "Miss"
                },
                Pagination = new PaginationInfo
                {
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize),
                    CurrentPage = parameters.PageNumber,
                    PageSize = parameters.PageSize
                }
            };
            
            return Result<EnhancedPagedResult<ProjectDto>>.Success(result, "Projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<EnhancedPagedResult<ProjectDto>>.ServerError($"An error occurred while retrieving projects: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null)
    {
        // This method maintains backward compatibility with the original API
        return await GetProjectsAsync(pageNumber, pageSize, managerId);
    }
    
    private IQueryable<Project> ApplyProjectFilters(IQueryable<Project> query, ProjectQueryParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.ProjectName))
        {
            query = query.Where(p => p.ProjectName.Contains(parameters.ProjectName));
        }
        
        if (!string.IsNullOrEmpty(parameters.Status))
        {
            if (Enum.TryParse<ProjectStatus>(parameters.Status, true, out var status))
            {
                query = query.Where(p => p.Status == status);
            }
        }
        
        if (!string.IsNullOrEmpty(parameters.ClientInfo))
        {
            query = query.Where(p => p.ClientInfo.Contains(parameters.ClientInfo));
        }
        
        if (!string.IsNullOrEmpty(parameters.Address))
        {
            query = query.Where(p => p.Address.Contains(parameters.Address));
        }
        
        if (parameters.ManagerId.HasValue)
        {
            query = query.Where(p => p.ProjectManagerId == parameters.ManagerId.Value);
        }
        
        if (parameters.StartDateAfter.HasValue)
        {
            query = query.Where(p => p.StartDate >= parameters.StartDateAfter.Value);
        }
        
        if (parameters.StartDateBefore.HasValue)
        {
            query = query.Where(p => p.StartDate <= parameters.StartDateBefore.Value);
        }
        
        if (parameters.EstimatedEndDateAfter.HasValue)
        {
            query = query.Where(p => p.EstimatedEndDate >= parameters.EstimatedEndDateAfter.Value);
        }
        
        if (parameters.EstimatedEndDateBefore.HasValue)
        {
            query = query.Where(p => p.EstimatedEndDate <= parameters.EstimatedEndDateBefore.Value);
        }
        
        return query;
    }

    private ProjectDto MapToDto(Project project)
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
            ProjectManager = new UserDto
            {
                UserId = project.ProjectManager.UserId,
                Username = project.ProjectManager.Username,
                Email = project.ProjectManager.Email,
                FullName = project.ProjectManager.FullName,
                RoleName = project.ProjectManager.Role.RoleName,
                IsActive = project.ProjectManager.IsActive
            },
            TaskCount = project.Tasks.Count,
            CompletedTaskCount = project.Tasks.Count(t => t.Status == Models.TaskStatus.Completed)
        };
    }

    private int DetermineFiltersApplied(ProjectQueryParameters parameters)
    {
        int count = 0;
        
        if (!string.IsNullOrEmpty(parameters.ProjectName)) count++;
        if (!string.IsNullOrEmpty(parameters.Status)) count++;
        if (!string.IsNullOrEmpty(parameters.ClientInfo)) count++;
        if (!string.IsNullOrEmpty(parameters.Address)) count++;
        if (parameters.ManagerId.HasValue) count++;
        if (parameters.StartDateAfter.HasValue) count++;
        if (parameters.StartDateBefore.HasValue) count++;
        if (parameters.EstimatedEndDateAfter.HasValue) count++;
        if (parameters.EstimatedEndDateBefore.HasValue) count++;
        
        // Add generic filters
        count += parameters.Filters.Count;
        
        return count;
    }
    
    private string DetermineQueryComplexity(ProjectQueryParameters parameters)
    {
        var complexity = DetermineFiltersApplied(parameters);
        
        // Add complexity for sorting
        if (!string.IsNullOrEmpty(parameters.SortBy)) complexity++;
        
        // Add complexity for field selection
        if (!string.IsNullOrEmpty(parameters.Fields)) complexity++;
        
        return complexity switch
        {
            <= 2 => "Simple",
            <= 5 => "Medium",
            _ => "Complex"
        };
    }
}
