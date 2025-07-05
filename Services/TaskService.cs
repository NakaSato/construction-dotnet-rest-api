using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;
using AutoMapper;
using TaskStatus = dotnet_rest_api.Models.TaskStatus;

namespace dotnet_rest_api.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(ApplicationDbContext context, IMapper mapper, ILogger<TaskService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId)
    {
        try
        {
            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .AsQueryable();

            // Apply filters
            if (projectId.HasValue)
                query = query.Where(t => t.ProjectId == projectId.Value);

            if (assigneeId.HasValue)
                query = query.Where(t => t.AssignedTechnicianId == assigneeId.Value);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tasks = await query
                .OrderBy(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var result = new PagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<TaskDto>>.SuccessResult(result, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return ServiceResult<PagedResult<TaskDto>>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskDto>> GetTaskByIdAsync(Guid id)
    {
        try
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            var taskDto = _mapper.Map<TaskDto>(task);
            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<PagedResult<TaskDto>>> GetProjectTasksAsync(Guid projectId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .Where(t => t.ProjectId == projectId);

            var totalCount = await query.CountAsync();

            var tasks = await query
                .OrderBy(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var result = new PagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<TaskDto>>.SuccessResult(result, "Project tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
            return ServiceResult<PagedResult<TaskDto>>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request)
    {
        try
        {
            // Verify project exists
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return ServiceResult<TaskDto>.ErrorResult("Project not found");

            // Verify assigned technician exists if provided
            if (request.AssignedTechnicianId.HasValue)
            {
                var technician = await _context.Users.FindAsync(request.AssignedTechnicianId.Value);
                if (technician == null)
                    return ServiceResult<TaskDto>.ErrorResult("Assigned technician not found");
            }

            // Create new task
            var task = new ProjectTask
            {
                TaskId = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title,
                Description = request.Description,
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Medium,
                DueDate = request.DueDate,
                AssignedTechnicianId = request.AssignedTechnicianId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            // Retrieve with navigation properties
            var createdTask = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

            var taskDto = _mapper.Map<TaskDto>(createdTask);
            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for project {ProjectId}", projectId);
            return ServiceResult<TaskDto>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            // Verify assigned technician exists if provided
            if (request.AssignedTechnicianId.HasValue)
            {
                var technician = await _context.Users.FindAsync(request.AssignedTechnicianId.Value);
                if (technician == null)
                    return ServiceResult<TaskDto>.ErrorResult("Assigned technician not found");
            }

            // Parse status
            if (!Enum.TryParse<TaskStatus>(request.Status, true, out var taskStatus))
                return ServiceResult<TaskDto>.ErrorResult($"Invalid task status: {request.Status}");

            // Update task properties
            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = taskStatus;
            task.DueDate = request.DueDate;
            task.AssignedTechnicianId = request.AssignedTechnicianId;

            // Set completion date if status is completed
            if (taskStatus == TaskStatus.Completed && task.CompletionDate == null)
            {
                task.CompletionDate = DateTime.UtcNow;
                task.CompletionPercentage = 100;
            }
            else if (taskStatus != TaskStatus.Completed)
            {
                task.CompletionDate = null;
            }

            await _context.SaveChangesAsync();

            // Retrieve with navigation properties
            var updatedTask = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            var taskDto = _mapper.Map<TaskDto>(updatedTask);
            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<TaskDto>.ErrorResult("Task not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                task.Title = request.Title;

            if (!string.IsNullOrEmpty(request.Description))
                task.Description = request.Description;

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (!Enum.TryParse<TaskStatus>(request.Status, true, out var taskStatus))
                    return ServiceResult<TaskDto>.ErrorResult($"Invalid task status: {request.Status}");
                
                task.Status = taskStatus;
                
                // Set completion date if status is completed
                if (taskStatus == TaskStatus.Completed && task.CompletionDate == null)
                {
                    task.CompletionDate = DateTime.UtcNow;
                    task.CompletionPercentage = 100;
                }
                else if (taskStatus != TaskStatus.Completed)
                {
                    task.CompletionDate = null;
                }
            }

            if (request.DueDate.HasValue)
                task.DueDate = request.DueDate.Value;

            if (request.AssignedTechnicianId.HasValue)
            {
                var technician = await _context.Users.FindAsync(request.AssignedTechnicianId.Value);
                if (technician == null)
                    return ServiceResult<TaskDto>.ErrorResult("Assigned technician not found");
                
                task.AssignedTechnicianId = request.AssignedTechnicianId.Value;
            }

            await _context.SaveChangesAsync();

            // Retrieve with navigation properties
            var updatedTask = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            var taskDto = _mapper.Map<TaskDto>(updatedTask);
            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Task updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching task {TaskId}", id);
            return ServiceResult<TaskDto>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> UpdateTaskStatusAsync(Guid id, TaskStatus status)
    {
        try
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return ServiceResult<bool>.ErrorResult("Task not found");

            task.Status = status;

            // Set completion date if status is completed
            if (status == TaskStatus.Completed && task.CompletionDate == null)
            {
                task.CompletionDate = DateTime.UtcNow;
                task.CompletionPercentage = 100;
            }
            else if (status != TaskStatus.Completed)
            {
                task.CompletionDate = null;
            }

            await _context.SaveChangesAsync();
            return ServiceResult<bool>.SuccessResult(true, "Task status updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status for task {TaskId}", id);
            return ServiceResult<bool>.ErrorResult(ex.Message);
        }
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
            return ServiceResult<bool>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters)
    {
        try
        {
            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .AsQueryable();

            // Apply filters
            if (parameters.ProjectId.HasValue)
                query = query.Where(t => t.ProjectId == parameters.ProjectId.Value);

            if (parameters.AssigneeId.HasValue)
                query = query.Where(t => t.AssignedTechnicianId == parameters.AssigneeId.Value);

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                if (Enum.TryParse<TaskStatus>(parameters.Status, true, out var taskStatus))
                    query = query.Where(t => t.Status == taskStatus);
            }

            if (!string.IsNullOrEmpty(parameters.Title))
                query = query.Where(t => t.Title.Contains(parameters.Title));

            if (parameters.DueDateAfter.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= parameters.DueDateAfter.Value);

            if (parameters.DueDateBefore.HasValue)
                query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= parameters.DueDateBefore.Value);

            if (parameters.CreatedAfter.HasValue)
                query = query.Where(t => t.CreatedAt >= parameters.CreatedAfter.Value);

            if (parameters.CreatedBefore.HasValue)
                query = query.Where(t => t.CreatedAt <= parameters.CreatedBefore.Value);

            if (parameters.CompletedAfter.HasValue)
                query = query.Where(t => t.CompletionDate.HasValue && t.CompletionDate.Value >= parameters.CompletedAfter.Value);

            if (parameters.CompletedBefore.HasValue)
                query = query.Where(t => t.CompletionDate.HasValue && t.CompletionDate.Value <= parameters.CompletedBefore.Value);

            // Apply sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "title" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                    "status" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                    "duedate" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
                    "createdat" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                    "priority" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
                    _ => query.OrderBy(t => t.CreatedAt)
                };
            }
            else
            {
                query = query.OrderBy(t => t.CreatedAt);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var tasks = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // Map to DTOs
            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var result = new EnhancedPagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                SortBy = parameters.SortBy,
                SortOrder = parameters.SortOrder
            };

            return ServiceResult<EnhancedPagedResult<TaskDto>>.SuccessResult(result, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with advanced query");
            return ServiceResult<EnhancedPagedResult<TaskDto>>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<PagedResult<TaskDto>>> GetPhaseTasksAsync(Guid phaseId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .Where(t => t.PhaseId == phaseId);

            var totalCount = await query.CountAsync();

            var tasks = await query
                .OrderBy(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var taskDtos = _mapper.Map<List<TaskDto>>(tasks);

            var result = new PagedResult<TaskDto>
            {
                Items = taskDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<TaskDto>>.SuccessResult(result, "Phase tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for phase {PhaseId}", phaseId);
            return ServiceResult<PagedResult<TaskDto>>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskDto>> CreatePhaseTaskAsync(Guid phaseId, CreateTaskRequest request)
    {
        try
        {
            // Verify phase exists and get project ID through master plan
            var phase = await _context.ProjectPhases
                .Include(p => p.MasterPlan)
                .FirstOrDefaultAsync(p => p.PhaseId == phaseId);
            
            if (phase == null)
                return ServiceResult<TaskDto>.ErrorResult("Phase not found");

            // Verify assigned technician exists if provided
            if (request.AssignedTechnicianId.HasValue)
            {
                var technician = await _context.Users.FindAsync(request.AssignedTechnicianId.Value);
                if (technician == null)
                    return ServiceResult<TaskDto>.ErrorResult("Assigned technician not found");
            }

            // Create new task
            var task = new ProjectTask
            {
                TaskId = Guid.NewGuid(),
                ProjectId = phase.MasterPlan.ProjectId,
                PhaseId = phaseId,
                Title = request.Title,
                Description = request.Description,
                Status = TaskStatus.NotStarted,
                Priority = TaskPriority.Medium,
                DueDate = request.DueDate,
                AssignedTechnicianId = request.AssignedTechnicianId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            // Retrieve with navigation properties
            var createdTask = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTechnician)
                .FirstOrDefaultAsync(t => t.TaskId == task.TaskId);

            var taskDto = _mapper.Map<TaskDto>(createdTask);
            return ServiceResult<TaskDto>.SuccessResult(taskDto, "Phase task created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task for phase {PhaseId}", phaseId);
            return ServiceResult<TaskDto>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize)
    {
        try
        {
            // For now, return empty result as TaskProgressReport entity implementation would be needed
            await System.Threading.Tasks.Task.CompletedTask; // To make async method valid
            
            var result = new PagedResult<TaskProgressReportDto>
            {
                Items = new List<TaskProgressReportDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<TaskProgressReportDto>>.SuccessResult(result, "Task progress reports retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress reports for task {TaskId}", taskId);
            return ServiceResult<PagedResult<TaskProgressReportDto>>.ErrorResult(ex.Message);
        }
    }

    public async Task<ServiceResult<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request)
    {
        try
        {
            // TODO: Implement proper task progress report storage when TaskProgressReport entity is added
            await System.Threading.Tasks.Task.CompletedTask;
            
            var report = new TaskProgressReportDto
            {
                ProgressReportId = Guid.NewGuid(),
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow,
                ReportDate = DateTime.UtcNow,
                CompletionPercentage = 0,
                PlannedCompletionPercentage = 0,
                Status = "Not Started",
                HoursWorked = 0,
                CreatedById = Guid.Empty,
                CreatedByName = "System"
            };

            return ServiceResult<TaskProgressReportDto>.SuccessResult(report, "Task progress report created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating progress report for task {TaskId}", taskId);
            return ServiceResult<TaskProgressReportDto>.ErrorResult(ex.Message);
        }
    }
}
