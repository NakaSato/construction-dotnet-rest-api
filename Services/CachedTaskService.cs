using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

/// <summary>
/// Enhanced Task Service with comprehensive caching capabilities
/// </summary>
public class CachedTaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedTaskService> _logger;

    // Cache duration constants
    private static readonly TimeSpan TaskDetailsCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TaskListCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan TaskQueryCacheDuration = TimeSpan.FromMinutes(8);
    private static readonly TimeSpan ProjectTasksCacheDuration = TimeSpan.FromMinutes(12);

    public CachedTaskService(
        ApplicationDbContext context, 
        IQueryService queryService,
        ICacheService cacheService,
        ILogger<CachedTaskService> logger)
    {
        _context = context;
        _queryService = queryService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(Guid taskId)
    {
        try
        {
            var cacheKey = $"task:id:{taskId}";
            
            // Try to get from cache first
            var cachedTask = await _cacheService.GetAsync<TaskDto>(cacheKey);
            if (cachedTask != null)
            {
                _logger.LogDebug("Cache hit for task ID: {TaskId}", taskId);
                return new ApiResponse<TaskDto>
                {
                    Success = true,
                    Data = cachedTask
                };
            }

            _logger.LogDebug("Cache miss for task ID: {TaskId}", taskId);

            var task = await _context.ProjectTasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                return new ApiResponse<TaskDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            var taskDto = MapToDto(task);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, taskDto, TaskDetailsCacheDuration);

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task by ID: {TaskId}", taskId);
            return new ApiResponse<TaskDto>
            {
                Success = false,
                Message = $"Error retrieving task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber = 1, int pageSize = 10, Guid? projectId = null, Guid? assignedTo = null)
    {
        try
        {
            var cacheKey = $"tasks:list:page:{pageNumber}:size:{pageSize}:project:{projectId?.ToString() ?? "all"}:assignee:{assignedTo?.ToString() ?? "all"}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<PagedResult<TaskDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for tasks list: {CacheKey}", cacheKey);
                return new ApiResponse<PagedResult<TaskDto>>
                {
                    Success = true,
                    Data = cachedResult
                };
            }

            _logger.LogDebug("Cache miss for tasks list: {CacheKey}", cacheKey);

            var query = _context.ProjectTasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .AsQueryable();

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            if (assignedTo.HasValue)
            {
                query = query.Where(t => t.AssignedTechnicianId == assignedTo.Value);
            }

            var totalCount = await query.CountAsync();
            
            var tasks = await query
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    AssignedTechnician = t.AssignedTechnician != null ? new UserDto
                    {
                        UserId = t.AssignedTechnician.UserId,
                        Username = t.AssignedTechnician.Username,
                        Email = t.AssignedTechnician.Email,
                        FullName = t.AssignedTechnician.FullName,
                        RoleName = t.AssignedTechnician.Role.RoleName,
                        IsActive = t.AssignedTechnician.IsActive
                    } : null,
                    CompletionDate = t.CompletionDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResult<TaskDto>
            {
                Items = tasks,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, result, TaskListCacheDuration);

            return new ApiResponse<PagedResult<TaskDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks list");
            return new ApiResponse<PagedResult<TaskDto>>
            {
                Success = false,
                Message = $"Error retrieving tasks: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters)
    {
        try
        {
            // Create cache key based on query parameters
            var cacheKey = $"tasks:query:{parameters.GetHashCode()}:{parameters.PageNumber}:{parameters.PageSize}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<EnhancedPagedResult<TaskDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for tasks query: {CacheKey}", cacheKey);
                return new ApiResponse<EnhancedPagedResult<TaskDto>>
                {
                    Success = true,
                    Data = cachedResult
                };
            }

            _logger.LogDebug("Cache miss for tasks query: {CacheKey}", cacheKey);

            var baseQuery = _context.ProjectTasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .AsQueryable();

            // Apply entity-specific filters first
            var filteredQuery = ApplyTaskFilters(baseQuery, parameters);

            // Use the generic query service for advanced filtering, sorting, and pagination
            var result = await _queryService.ExecuteQueryAsync(filteredQuery, parameters);

            // Convert entities to DTOs
            var dtoItems = result.Items.Select(MapToDto).ToList();
            
            // Apply field selection if requested
            var finalItems = string.IsNullOrEmpty(parameters.Fields) 
                ? dtoItems.Cast<object>().ToList()
                : _queryService.ApplyFieldSelection(dtoItems, parameters.Fields);

            var enhancedResult = new EnhancedPagedResult<TaskDto>
            {
                Items = string.IsNullOrEmpty(parameters.Fields) 
                    ? dtoItems 
                    : finalItems.Cast<TaskDto>().ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder,
                RequestedFields = string.IsNullOrEmpty(parameters.Fields) 
                    ? new List<string>() 
                    : parameters.Fields.Split(',').Select(f => f.Trim()).ToList(),
                Metadata = result.Metadata
            };

            // Cache the result with shorter duration for complex queries
            await _cacheService.SetAsync(cacheKey, enhancedResult, TaskQueryCacheDuration);

            return new ApiResponse<EnhancedPagedResult<TaskDto>>
            {
                Success = true,
                Data = enhancedResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with enhanced query");
            return new ApiResponse<EnhancedPagedResult<TaskDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving tasks",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetTasksLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? projectId = null, Guid? assignedTo = null)
    {
        // Use the cached version of the standard method
        return await GetTasksAsync(pageNumber, pageSize, projectId, assignedTo);
    }

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"tasks:project:{projectId}:page:{pageNumber}:size:{pageSize}";
            
            // Try to get from cache first
            var cachedResult = await _cacheService.GetAsync<PagedResult<TaskDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Cache hit for project tasks: {CacheKey}", cacheKey);
                return new ApiResponse<PagedResult<TaskDto>>
                {
                    Success = true,
                    Data = cachedResult
                };
            }

            _logger.LogDebug("Cache miss for project tasks: {CacheKey}", cacheKey);

            // Use the standard method with project filter and cache the result
            var result = await GetTasksAsync(pageNumber, pageSize, projectId, null);
            
            if (result.Success && result.Data != null)
            {
                // Cache project-specific tasks separately with longer duration
                await _cacheService.SetAsync(cacheKey, result.Data, ProjectTasksCacheDuration);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project tasks: {ProjectId}", projectId);
            return new ApiResponse<PagedResult<TaskDto>>
            {
                Success = false,
                Message = $"Error retrieving project tasks: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request)
    {
        try
        {
            // Verify project exists
            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                return new ApiResponse<TaskDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            // Verify assigned technician exists if provided
            if (request.AssignedTechnicianId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == request.AssignedTechnicianId.Value);
                if (!userExists)
                {
                    return new ApiResponse<TaskDto>
                    {
                        Success = false,
                        Message = "Assigned technician not found"
                    };
                }
            }

            var task = new ProjectTask
            {
                TaskId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                ProjectId = projectId,
                AssignedTechnicianId = request.AssignedTechnicianId,
                DueDate = request.DueDate,
                Status = Models.TaskStatus.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            // Load the task with related data
            var createdTask = await _context.ProjectTasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .FirstAsync(t => t.TaskId == task.TaskId);

            var taskDto = MapToDto(createdTask);

            // Cache the new task
            var taskCacheKey = $"task:id:{task.TaskId}";
            await _cacheService.SetAsync(taskCacheKey, taskDto, TaskDetailsCacheDuration);

            // Invalidate related caches
            await InvalidateTaskListCaches(projectId, request.AssignedTechnicianId);

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto,
                Message = "Task created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for project: {ProjectId}", projectId);
            return new ApiResponse<TaskDto>
            {
                Success = false,
                Message = $"Error creating task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<TaskDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            // Store original values for cache invalidation
            var originalProjectId = task.ProjectId;
            var originalAssigneeId = task.AssignedTechnicianId;

            // Verify assigned technician exists if provided
            if (request.AssignedTechnicianId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == request.AssignedTechnicianId.Value);
                if (!userExists)
                {
                    return new ApiResponse<TaskDto>
                    {
                        Success = false,
                        Message = "Assigned technician not found"
                    };
                }
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.AssignedTechnicianId = request.AssignedTechnicianId;
            task.DueDate = request.DueDate;

            if (Enum.TryParse<Models.TaskStatus>(request.Status, true, out var taskStatus))
            {
                task.Status = taskStatus;
                if (taskStatus == Models.TaskStatus.Completed)
                {
                    task.CompletionDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Load the task with related data
            var updatedTask = await _context.ProjectTasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .FirstAsync(t => t.TaskId == task.TaskId);

            var taskDto = MapToDto(updatedTask);

            // Update cache with new data
            var taskCacheKey = $"task:id:{taskId}";
            await _cacheService.SetAsync(taskCacheKey, taskDto, TaskDetailsCacheDuration);

            // Invalidate related caches (both old and new assignee/project if changed)
            await InvalidateTaskListCaches(originalProjectId, originalAssigneeId);
            if (originalProjectId != task.ProjectId)
            {
                await InvalidateTaskListCaches(task.ProjectId, null);
            }
            if (originalAssigneeId != task.AssignedTechnicianId)
            {
                await InvalidateTaskListCaches(null, task.AssignedTechnicianId);
            }

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto,
                Message = "Task updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task: {TaskId}", taskId);
            return new ApiResponse<TaskDto>
            {
                Success = false,
                Message = $"Error updating task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<TaskDto>> PatchTaskAsync(Guid taskId, PatchTaskRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<TaskDto>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            // Store original values for cache invalidation
            var originalProjectId = task.ProjectId;
            var originalAssigneeId = task.AssignedTechnicianId;
            bool hasChanges = false;

            // Only update provided fields
            if (!string.IsNullOrEmpty(request.Title))
            {
                task.Title = request.Title;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                task.Description = request.Description;
                hasChanges = true;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (!Enum.TryParse<Models.TaskStatus>(request.Status, true, out var taskStatus))
                {
                    return new ApiResponse<TaskDto>
                    {
                        Success = false,
                        Message = "Invalid task status"
                    };
                }
                
                task.Status = taskStatus;
                if (taskStatus == Models.TaskStatus.Completed)
                {
                    task.CompletionDate = DateTime.UtcNow;
                }
                hasChanges = true;
            }

            if (request.DueDate.HasValue)
            {
                task.DueDate = request.DueDate.Value;
                hasChanges = true;
            }

            if (request.AssignedTechnicianId.HasValue)
            {
                // Verify assigned technician exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == request.AssignedTechnicianId.Value);
                if (!userExists)
                {
                    return new ApiResponse<TaskDto>
                    {
                        Success = false,
                        Message = "Assigned technician not found"
                    };
                }
                task.AssignedTechnicianId = request.AssignedTechnicianId.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _context.SaveChangesAsync();

                // Load the task with related data
                var updatedTask = await _context.ProjectTasks
                    .Include(t => t.AssignedTechnician)
                    .ThenInclude(u => u!.Role)
                    .Include(t => t.Project)
                    .FirstAsync(t => t.TaskId == task.TaskId);

                var taskDto = MapToDto(updatedTask);

                // Update cache with new data
                var taskCacheKey = $"task:id:{taskId}";
                await _cacheService.SetAsync(taskCacheKey, taskDto, TaskDetailsCacheDuration);

                // Invalidate related caches (both old and new assignee/project if changed)
                await InvalidateTaskListCaches(originalProjectId, originalAssigneeId);
                if (originalProjectId != task.ProjectId)
                {
                    await InvalidateTaskListCaches(task.ProjectId, null);
                }
                if (originalAssigneeId != task.AssignedTechnicianId)
                {
                    await InvalidateTaskListCaches(null, task.AssignedTechnicianId);
                }

                _logger.LogInformation("Patched task {TaskId}", taskId);

                return new ApiResponse<TaskDto>
                {
                    Success = true,
                    Data = taskDto,
                    Message = "Task updated successfully"
                };
            }
            else
            {
                // No changes made, return current task
                var currentTask = await _context.ProjectTasks
                    .Include(t => t.AssignedTechnician)
                    .ThenInclude(u => u!.Role)
                    .Include(t => t.Project)
                    .FirstAsync(t => t.TaskId == task.TaskId);

                return new ApiResponse<TaskDto>
                {
                    Success = true,
                    Data = MapToDto(currentTask),
                    Message = "No changes were made to the task"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching task: {TaskId}", taskId);
            return new ApiResponse<TaskDto>
            {
                Success = false,
                Message = $"Error updating task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateTaskStatusAsync(Guid taskId, Models.TaskStatus status)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            task.Status = status;
            if (status == Models.TaskStatus.Completed)
            {
                task.CompletionDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Invalidate task cache
            var taskCacheKey = $"task:id:{taskId}";
            await _cacheService.RemoveAsync(taskCacheKey);

            // Invalidate related list caches
            await InvalidateTaskListCaches(task.ProjectId, task.AssignedTechnicianId);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Task status updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status: {TaskId}", taskId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error updating task status: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteTaskAsync(Guid taskId)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            var projectId = task.ProjectId;
            var assigneeId = task.AssignedTechnicianId;

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            // Remove from cache
            var taskCacheKey = $"task:id:{taskId}";
            await _cacheService.RemoveAsync(taskCacheKey);

            // Invalidate related list caches
            await InvalidateTaskListCaches(projectId, assigneeId);

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Task deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task: {TaskId}", taskId);
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting task: {ex.Message}"
            };
        }
    }

    #region Private Helper Methods

    private async Task InvalidateTaskListCaches(Guid? projectId = null, Guid? assigneeId = null)
    {
        // Invalidate general task list caches
        await _cacheService.InvalidateByPatternAsync("tasks:list:*");
        await _cacheService.InvalidateByPatternAsync("tasks:query:*");

        // Invalidate specific project task caches if project is specified
        if (projectId.HasValue)
        {
            await _cacheService.InvalidateByPatternAsync($"tasks:project:{projectId}:*");
        }

        // Note: We could also invalidate assignee-specific caches here if we had them
        // await _cacheService.InvalidateByPatternAsync($"tasks:assignee:{assigneeId}:*");
    }

    private IQueryable<ProjectTask> ApplyTaskFilters(IQueryable<ProjectTask> query, TaskQueryParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.Title))
        {
            query = query.Where(t => t.Title.Contains(parameters.Title));
        }
        
        if (!string.IsNullOrEmpty(parameters.Status))
        {
            if (Enum.TryParse<Models.TaskStatus>(parameters.Status, true, out var status))
            {
                query = query.Where(t => t.Status == status);
            }
        }
        
        if (parameters.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == parameters.ProjectId.Value);
        }
        
        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssignedTechnicianId == parameters.AssigneeId.Value);
        }
        
        if (parameters.DueDateAfter.HasValue)
        {
            query = query.Where(t => t.DueDate >= parameters.DueDateAfter.Value);
        }
        
        if (parameters.DueDateBefore.HasValue)
        {
            query = query.Where(t => t.DueDate <= parameters.DueDateBefore.Value);
        }
        
        if (parameters.CreatedAfter.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);
        }
        
        if (parameters.CreatedBefore.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);
        }
        
        if (parameters.CompletedAfter.HasValue)
        {
            query = query.Where(t => t.CompletionDate >= parameters.CompletedAfter.Value);
        }
        
        if (parameters.CompletedBefore.HasValue)
        {
            query = query.Where(t => t.CompletionDate <= parameters.CompletedBefore.Value);
        }
        
        return query;
    }

    private static TaskDto MapToDto(ProjectTask task)
    {
        return new TaskDto
        {
            TaskId = task.TaskId,
            ProjectId = task.ProjectId,
            ProjectName = task.Project.ProjectName,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            AssignedTechnician = task.AssignedTechnician != null ? new UserDto
            {
                UserId = task.AssignedTechnician.UserId,
                Username = task.AssignedTechnician.Username,
                Email = task.AssignedTechnician.Email,
                FullName = task.AssignedTechnician.FullName,
                RoleName = task.AssignedTechnician.Role.RoleName,
                IsActive = task.AssignedTechnician.IsActive
            } : null,
            CompletionDate = task.CompletionDate,
            CreatedAt = task.CreatedAt
        };
    }

    #endregion
}
