using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

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

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid projectId)
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
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            var projectDto = MapToDto(project);

            return new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = projectDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetProjectsAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null)
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

            return new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving projects",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            // Validate project manager exists
            var manager = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId && u.IsActive);

            if (manager == null)
            {
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project manager not found or inactive"
                };
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

            return new ApiResponse<ProjectDto>
            {
                Success = true,
                Message = "Project created successfully",
                Data = projectDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while creating the project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request)
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
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            // Validate project manager exists
            var manager = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId && u.IsActive);

            if (manager == null)
            {
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project manager not found or inactive"
                };
            }

            // Parse status
            if (!Enum.TryParse<ProjectStatus>(request.Status, out var status))
            {
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Invalid project status"
                };
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

            return new ApiResponse<ProjectDto>
            {
                Success = true,
                Message = "Project updated successfully",
                Data = projectDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while updating the project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteProjectAsync(Guid projectId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Project deleted successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
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

            return new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving user projects",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
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
            
            // Use the generic query service for advanced querying
            var result = await _queryService.ExecuteQueryAsync(query.Select(p => MapToDto(p)), parameters);
            
            return new ApiResponse<EnhancedPagedResult<ProjectDto>>
            {
                Success = true,
                Message = "Projects retrieved successfully",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EnhancedPagedResult<ProjectDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving projects",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? managerId = null)
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
}
