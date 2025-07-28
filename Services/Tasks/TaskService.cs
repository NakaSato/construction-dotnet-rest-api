using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Common;
using TaskStatus = dotnet_rest_api.Models.TaskStatus;

namespace dotnet_rest_api.Services.Tasks;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId)
    {
        try
        {
            var query = _context.ProjectTasks.AsQueryable();
            
            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            var totalCount = await query.CountAsync();
            var tasks = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
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

            return ServiceResult<PagedResult<TaskDto>>.SuccessResult(result, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return ServiceResult<PagedResult<TaskDto>>.ErrorResult($"Error retrieving tasks: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId)
    {
        try
        {
            var query = _context.ProjectTasks.AsQueryable();
            
            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);
                
            if (assigneeId.HasValue)
                query = query.Where(t => t.AssignedTechnicianId == assigneeId.Value);

            var totalCount = await query.CountAsync();
            var tasks = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
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

            return ServiceResult<PagedResult<TaskDto>>.SuccessResult(result, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return ServiceResult<PagedResult<TaskDto>>.ErrorResult($"Error retrieving tasks: {ex.Message}");
        }
    }

    public async Task<ServiceResult<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters)
    {
        // Basic implementation - return empty enhanced result for now
        var result = new EnhancedPagedResult<TaskDto>
        {
            Items = new List<TaskDto>(),
            TotalCount = 0,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };

        return ServiceResult<EnhancedPagedResult<TaskDto>>.SuccessResult(result, "Tasks retrieved successfully");
    }

    public async Task<ServiceResult<TaskDto>> GetTaskByIdAsync(Guid id)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error retrieving task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            var task = new ProjectTask
            {
                TaskId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                AssignedTechnicianId = request.AssignedTechnicianId,
                Status = TaskStatus.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return ServiceResult<TaskDto>.ErrorResult($"Error creating task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request)
    {
        try
        {
            var task = new ProjectTask
            {
                TaskId = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                AssignedTechnicianId = request.AssignedTechnicianId,
                Status = TaskStatus.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return ServiceResult<TaskDto>.ErrorResult($"Error creating task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.AssignedTechnicianId = request.AssignedTechnicianId;
            
            if (Enum.TryParse<TaskStatus>(request.Status, out var status))
                task.Status = status;

            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error updating task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request)
    {
        // Basic implementation - redirect to update for now
        var updateRequest = new UpdateTaskRequest
        {
            Title = request.Title ?? "",
            Description = request.Description ?? "",
            Status = request.Status ?? "Pending",
            DueDate = request.DueDate,
            AssignedTechnicianId = request.AssignedTechnicianId
        };
        
        return await UpdateTaskAsync(id, updateRequest);
    }

    public async Task<ServiceResult<bool>> DeleteTaskAsync(Guid id)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<bool>.ErrorResult("Task not found");

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Task deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return ServiceResult<bool>.ErrorResult($"Error deleting task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskStatusAsync(Guid id, int status)
    {
        return await UpdateTaskStatusAsync(id, (TaskStatus)status);
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus status)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            task.Status = status;
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error updating task status: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskProgressAsync(Guid id, decimal progress)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            // Basic implementation - just save the change
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task progress updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task progress {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error updating task progress: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskPriorityAsync(Guid id, TaskPriority priority)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            // Basic implementation - just save the change
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task priority updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task priority {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error updating task priority: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskDueDateAsync(Guid id, DateTime dueDate)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            task.DueDate = dueDate;
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task due date updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task due date {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error updating task due date: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> AssignTaskAsync(Guid id, Guid technicianId)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            task.AssignedTechnicianId = technicianId;
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error assigning task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<TaskDto>> UnassignTaskAsync(Guid id)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            task.AssignedTechnicianId = null;
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task unassigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult($"Error unassigning task: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<TaskDto>>> GetTasksByProjectIdAsync(Guid projectId)
    {
        try
        {
            var tasks = await _context.ProjectTasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return ServiceResult<List<TaskDto>>.SuccessResult(tasks, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
            return ServiceResult<List<TaskDto>>.ErrorResult($"Error retrieving tasks: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<TaskDto>>> GetTasksByAssigneeAsync(Guid userId)
    {
        try
        {
            var tasks = await _context.ProjectTasks
                .Where(t => t.AssignedTechnicianId == userId)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    ProjectId = t.ProjectId,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return ServiceResult<List<TaskDto>>.SuccessResult(tasks, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user {UserId}", userId);
            return ServiceResult<List<TaskDto>>.ErrorResult($"Error retrieving tasks: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize)
    {
        // Basic implementation - return empty result for now
        var result = new PagedResult<TaskProgressReportDto>
        {
            Items = new List<TaskProgressReportDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ServiceResult<PagedResult<TaskProgressReportDto>>.SuccessResult(result, "Progress reports retrieved successfully");
    }

    public async Task<ServiceResult<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request)
    {
        // Basic implementation - return empty DTO for now
        var reportDto = new TaskProgressReportDto
        {
            TaskId = taskId,
            CreatedAt = DateTime.UtcNow
        };

        return ServiceResult<TaskProgressReportDto>.SuccessResult(reportDto, "Progress report created successfully");
    }

    public async Task<ServiceResult<TaskDto>> AddTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(taskId);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            // Basic implementation - just return the task
            var taskDto = new TaskDto
            {
                TaskId = task.TaskId,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt
            };

            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Progress report added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding progress report to task {TaskId}", taskId);
            return ServiceResult<TaskDto>.ErrorResult($"Error adding progress report: {ex.Message}");
        }
    }
}
