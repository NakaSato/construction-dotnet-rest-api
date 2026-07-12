using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using TaskModel = dotnet_rest_api.Models.Task;
using TaskStatus = dotnet_rest_api.Models.TaskStatus;

namespace dotnet_rest_api.Services.Tasks;

public interface ITaskService
{
    // Overloaded GetTasksAsync methods
    System.Threading.Tasks.Task<Result<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId);
    System.Threading.Tasks.Task<Result<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId);
    System.Threading.Tasks.Task<Result<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters);
    
    // Basic CRUD operations
    System.Threading.Tasks.Task<Result<TaskDto>> GetTaskByIdAsync(Guid id);
    System.Threading.Tasks.Task<Result<TaskDto>> CreateTaskAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<Result<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request);
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<Result<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request);
    System.Threading.Tasks.Task<Result<bool>> DeleteTaskAsync(Guid id);
    
    // Status and progress operations
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, int status);
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus status);
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskProgressAsync(Guid id, decimal progress);
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskPriorityAsync(Guid id, TaskPriority priority);
    System.Threading.Tasks.Task<Result<TaskDto>> UpdateTaskDueDateAsync(Guid id, DateTime dueDate);
    
    // Assignment operations
    System.Threading.Tasks.Task<Result<TaskDto>> AssignTaskAsync(Guid id, Guid technicianId);
    System.Threading.Tasks.Task<Result<TaskDto>> UnassignTaskAsync(Guid id);
    
    // Query operations
    System.Threading.Tasks.Task<Result<List<TaskDto>>> GetTasksByProjectIdAsync(Guid projectId);
    System.Threading.Tasks.Task<Result<List<TaskDto>>> GetTasksByAssigneeAsync(Guid userId);
    
    // Progress reporting
    System.Threading.Tasks.Task<Result<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<Result<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
    System.Threading.Tasks.Task<Result<TaskDto>> AddTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
    
    // Phase-related operations
    System.Threading.Tasks.Task<Result<PagedResult<TaskDto>>> GetPhaseTasksAsync(Guid phaseId, int pageNumber, int pageSize);
}

