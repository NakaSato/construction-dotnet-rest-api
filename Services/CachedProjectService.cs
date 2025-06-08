using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

/// <summary>
/// Enhanced Project Service with comprehensive caching capabilities
/// </summary>
public class CachedProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedProjectService> _logger;

    public CachedProjectService(
        ApplicationDbContext context, 
        IQueryService queryService,
        ICacheService cacheService,
        ILogger<CachedProjectService> logger)
    {
        _context = context;
        _queryService = queryService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid projectId)
    {
        var cacheKey = $"project:{projectId}:details";
        
        try
        {
            // Try to get from cache first
            var cachedProject = await _cacheService.GetAsync<ApiResponse<ProjectDto>>(cacheKey);
            if (cachedProject != null)
            {
                _logger.LogDebug("Retrieved project {ProjectId} from cache", projectId);
                return cachedProject;
            }

            // If not in cache, get from database
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

            var response = new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToDto(project)
            };

            // Cache the result for 20 minutes
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(20));
            _logger.LogDebug("Cached project {ProjectId} details", projectId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", projectId);
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
        var cacheKey = $"projects:list:page{pageNumber}:size{pageSize}:manager{managerId}";
        
        try
        {
            // Check cache first
            var cachedResult = await _cacheService.GetAsync<ApiResponse<PagedResult<ProjectDto>>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Retrieved projects list from cache");
                return cachedResult;
            }

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

            var response = new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = true,
                Data = result
            };

            // Cache for 10 minutes (project lists change more frequently)
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            _logger.LogDebug("Cached projects list");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
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
            // Validate project manager exists (with caching)
            var managerCacheKey = $"user:{request.ProjectManagerId}:profile";
            var manager = await _cacheService.GetAsync<User>(managerCacheKey);
            
            if (manager == null)
            {
                manager = await _context.Users
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
                
                // Cache the manager profile
                await _cacheService.SetAsync(managerCacheKey, manager, TimeSpan.FromMinutes(30));
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

            var response = new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToDto(project)
            };

            // Invalidate related caches
            await InvalidateProjectCaches();
            
            _logger.LogInformation("Created project {ProjectId}", project.ProjectId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
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

            // Validate project manager exists (with caching)
            var managerCacheKey = $"user:{request.ProjectManagerId}:profile";
            var manager = await _cacheService.GetAsync<User>(managerCacheKey);
            
            if (manager == null)
            {
                manager = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId && u.IsActive);

                if (manager == null)
                {
                    return new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Project manager not found or inactive"
                    };
                }
                
                await _cacheService.SetAsync(managerCacheKey, manager, TimeSpan.FromMinutes(30));
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

            // Update project
            project.ProjectName = request.ProjectName;
            project.Address = request.Address;
            project.ClientInfo = request.ClientInfo;
            project.Status = status;
            project.StartDate = request.StartDate;
            project.EstimatedEndDate = request.EstimatedEndDate;
            project.ActualEndDate = request.ActualEndDate;
            project.ProjectManagerId = request.ProjectManagerId;

            await _context.SaveChangesAsync();

            var response = new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToDto(project)
            };

            // Invalidate specific project cache and related caches
            await _cacheService.InvalidateProjectDataAsync(projectId);
            await InvalidateProjectCaches();
            
            _logger.LogInformation("Updated project {ProjectId}", projectId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", projectId);
            return new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while updating the project",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<ProjectDto>> PatchProjectAsync(Guid projectId, PatchProjectRequest request)
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

            bool hasChanges = false;

            // Update only provided fields
            if (request.ProjectName != null)
            {
                project.ProjectName = request.ProjectName;
                hasChanges = true;
            }

            if (request.Address != null)
            {
                project.Address = request.Address;
                hasChanges = true;
            }

            if (request.ClientInfo != null)
            {
                project.ClientInfo = request.ClientInfo;
                hasChanges = true;
            }

            if (request.Status != null)
            {
                if (!Enum.TryParse<ProjectStatus>(request.Status, out var status))
                {
                    return new ApiResponse<ProjectDto>
                    {
                        Success = false,
                        Message = "Invalid project status"
                    };
                }
                project.Status = status;
                hasChanges = true;
            }

            if (request.StartDate.HasValue)
            {
                project.StartDate = request.StartDate.Value;
                hasChanges = true;
            }

            if (request.EstimatedEndDate.HasValue)
            {
                project.EstimatedEndDate = request.EstimatedEndDate.Value;
                hasChanges = true;
            }

            if (request.ActualEndDate.HasValue)
            {
                project.ActualEndDate = request.ActualEndDate.Value;
                hasChanges = true;
            }

            if (request.ProjectManagerId.HasValue)
            {
                // Validate project manager exists (with caching)
                var managerCacheKey = $"user:{request.ProjectManagerId.Value}:profile";
                var manager = await _cacheService.GetAsync<User>(managerCacheKey);
                
                if (manager == null)
                {
                    manager = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == request.ProjectManagerId.Value && u.IsActive);

                    if (manager == null)
                    {
                        return new ApiResponse<ProjectDto>
                        {
                            Success = false,
                            Message = "Project manager not found or inactive"
                        };
                    }
                    
                    await _cacheService.SetAsync(managerCacheKey, manager, TimeSpan.FromMinutes(30));
                }

                project.ProjectManagerId = request.ProjectManagerId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();
                
                // Invalidate specific project cache and related caches
                await _cacheService.InvalidateProjectDataAsync(projectId);
                await InvalidateProjectCaches();
                
                _logger.LogInformation("Patched project {ProjectId}", projectId);
            }

            var response = new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = MapToDto(project)
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching project {ProjectId}", projectId);
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

            // Invalidate all project-related caches
            await _cacheService.InvalidateProjectDataAsync(projectId);
            await InvalidateProjectCaches();
            
            _logger.LogInformation("Deleted project {ProjectId}", projectId);
            
            return new ApiResponse<bool>
            {
                Success = true,
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
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
        var cacheKey = $"user:{userId}:projects:page{pageNumber}:size{pageSize}";
        
        try
        {
            // Check cache first
            var cachedResult = await _cacheService.GetAsync<ApiResponse<PagedResult<ProjectDto>>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Retrieved user projects from cache for user {UserId}", userId);
                return cachedResult;
            }

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

            var response = new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = true,
                Data = result
            };

            // Cache user-specific projects for 15 minutes
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));
            _logger.LogDebug("Cached user projects for user {UserId}", userId);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects for user {UserId}", userId);
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
        var cacheKey = $"projects:enhanced:{parameters.GetHashCode()}";
        
        try
        {
            // Check cache first for complex queries
            var cachedResult = await _cacheService.GetAsync<ApiResponse<EnhancedPagedResult<ProjectDto>>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Retrieved enhanced projects from cache");
                return cachedResult;
            }

            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Include(p => p.Tasks)
                .AsQueryable();

            // Apply specific filters based on query parameters
            query = ApplyProjectFilters(query, parameters);
            
            // Use the generic query service for advanced querying
            var result = await _queryService.ExecuteQueryAsync(query.Select(p => MapToDto(p)), parameters);
            
            var response = new ApiResponse<EnhancedPagedResult<ProjectDto>>
            {
                Success = true,
                Message = "Projects retrieved successfully",
                Data = result
            };

            // Cache enhanced queries for 8 minutes (shorter due to complexity)
            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(8));
            _logger.LogDebug("Cached enhanced projects query");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enhanced projects");
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
        // This method maintains backward compatibility with the original API and delegates to the cached version
        return await GetProjectsAsync(pageNumber, pageSize, managerId);
    }

    #region Private Helper Methods

    private async Task InvalidateProjectCaches()
    {
        // Invalidate general project list caches
        var cacheKeysToInvalidate = new[]
        {
            "projects:list",
            "projects:stats",
            "projects:enhanced"
        };

        var tasks = cacheKeysToInvalidate.Select(key => _cacheService.RemoveAsync(key));
        await Task.WhenAll(tasks);
        
        _logger.LogDebug("Invalidated general project caches");
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
        
        return query;
    }

    private static ProjectDto MapToDto(Project project)
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

    #endregion
}
