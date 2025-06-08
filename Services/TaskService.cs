using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public TaskService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(Guid taskId)
    {
        try
        {
            var task = await _context.Tasks
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

            var taskDto = new TaskDto
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

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto
            };
        }
        catch (Exception ex)
        {
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
            var query = _context.Tasks
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

            return new ApiResponse<PagedResult<TaskDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<TaskDto>>
            {
                Success = false,
                Message = $"Error retrieving tasks: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10)
    {
        return await GetTasksAsync(pageNumber, pageSize, projectId, null);
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

            var task = new TaskItem
            {
                TaskId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                ProjectId = projectId,
                AssignedTechnicianId = request.AssignedTechnicianId,
                DueDate = request.DueDate,
                Status = Models.TaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Load the task with related data
            var createdTask = await _context.Tasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .FirstAsync(t => t.TaskId == task.TaskId);

            var taskDto = new TaskDto
            {
                TaskId = createdTask.TaskId,
                ProjectId = createdTask.ProjectId,
                ProjectName = createdTask.Project.ProjectName,
                Title = createdTask.Title,
                Description = createdTask.Description,
                Status = createdTask.Status.ToString(),
                DueDate = createdTask.DueDate,
                AssignedTechnician = createdTask.AssignedTechnician != null ? new UserDto
                {
                    UserId = createdTask.AssignedTechnician.UserId,
                    Username = createdTask.AssignedTechnician.Username,
                    Email = createdTask.AssignedTechnician.Email,
                    FullName = createdTask.AssignedTechnician.FullName,
                    RoleName = createdTask.AssignedTechnician.Role.RoleName,
                    IsActive = createdTask.AssignedTechnician.IsActive
                } : null,
                CompletionDate = createdTask.CompletionDate,
                CreatedAt = createdTask.CreatedAt
            };

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto,
                Message = "Task created successfully"
            };
        }
        catch (Exception ex)
        {
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
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<TaskDto>
                {
                    Success = false,
                    Message = "Task not found"
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
            var updatedTask = await _context.Tasks
                .Include(t => t.AssignedTechnician)
                .ThenInclude(u => u!.Role)
                .Include(t => t.Project)
                .FirstAsync(t => t.TaskId == task.TaskId);

            var taskDto = new TaskDto
            {
                TaskId = updatedTask.TaskId,
                ProjectId = updatedTask.ProjectId,
                ProjectName = updatedTask.Project.ProjectName,
                Title = updatedTask.Title,
                Description = updatedTask.Description,
                Status = updatedTask.Status.ToString(),
                DueDate = updatedTask.DueDate,
                AssignedTechnician = updatedTask.AssignedTechnician != null ? new UserDto
                {
                    UserId = updatedTask.AssignedTechnician.UserId,
                    Username = updatedTask.AssignedTechnician.Username,
                    Email = updatedTask.AssignedTechnician.Email,
                    FullName = updatedTask.AssignedTechnician.FullName,
                    RoleName = updatedTask.AssignedTechnician.Role.RoleName,
                    IsActive = updatedTask.AssignedTechnician.IsActive
                } : null,
                CompletionDate = updatedTask.CompletionDate,
                CreatedAt = updatedTask.CreatedAt
            };

            return new ApiResponse<TaskDto>
            {
                Success = true,
                Data = taskDto,
                Message = "Task updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TaskDto>
            {
                Success = false,
                Message = $"Error updating task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteTaskAsync(Guid taskId)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Task deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error deleting task: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateTaskStatusAsync(Guid taskId, dotnet_rest_api.Models.TaskStatus status)
    {
        try
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Task not found"
                };
            }

            task.Status = status;
            if (status == dotnet_rest_api.Models.TaskStatus.Completed)
            {
                task.CompletionDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Task status updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error updating task status: {ex.Message}"
            };
        }
    }
    
    // Advanced querying methods
    public async Task<ApiResponse<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters)
    {
        try
        {
            var baseQuery = _context.Tasks
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

            return new ApiResponse<EnhancedPagedResult<TaskDto>>
            {
                Success = true,
                Data = enhancedResult
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EnhancedPagedResult<TaskDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving tasks",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<TaskDto>>> GetTasksLegacyAsync(int pageNumber = 1, int pageSize = 10, Guid? projectId = null, Guid? assignedTechnicianId = null)
    {
        // This method maintains backward compatibility with the original API
        return await GetTasksAsync(pageNumber, pageSize, projectId, assignedTechnicianId);
    }
    
    private IQueryable<TaskItem> ApplyTaskFilters(IQueryable<TaskItem> query, TaskQueryParameters parameters)
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

    private TaskDto MapToDto(TaskItem task)
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
}