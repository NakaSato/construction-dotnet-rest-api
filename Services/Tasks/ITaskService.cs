using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using TaskModel = dotnet_rest_api.Models.Task;
using TaskStatus = dotnet_rest_api.Models.TaskStatus;

namespace dotnet_rest_api.Services.Tasks;

public interface ITaskService
{
    // Overloaded GetTasksAsync methods
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId);
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskDto>>> GetTasksAsync(int pageNumber, int pageSize, Guid? projectId, Guid? assigneeId);
    System.Threading.Tasks.Task<ServiceResult<EnhancedPagedResult<TaskDto>>> GetTasksAsync(TaskQueryParameters parameters);
    
    // Basic CRUD operations
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> GetTaskByIdAsync(Guid id);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> CreateTaskAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> CreateTaskAsync(Guid projectId, CreateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> PatchTaskAsync(Guid id, PatchTaskRequest request);
    System.Threading.Tasks.Task<ServiceResult<bool>> DeleteTaskAsync(Guid id);
    
    // Status and progress operations
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskStatusAsync(Guid id, int status);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskStatusAsync(Guid id, TaskStatus status);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskProgressAsync(Guid id, decimal progress);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskPriorityAsync(Guid id, TaskPriority priority);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UpdateTaskDueDateAsync(Guid id, DateTime dueDate);
    
    // Assignment operations
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> AssignTaskAsync(Guid id, Guid technicianId);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> UnassignTaskAsync(Guid id);
    
    // Query operations
    System.Threading.Tasks.Task<ServiceResult<List<TaskDto>>> GetTasksByProjectIdAsync(Guid projectId);
    System.Threading.Tasks.Task<ServiceResult<List<TaskDto>>> GetTasksByAssigneeAsync(Guid userId);
    
    // Progress reporting
    System.Threading.Tasks.Task<ServiceResult<PagedResult<TaskProgressReportDto>>> GetTaskProgressReportsAsync(Guid taskId, int pageNumber, int pageSize);
    System.Threading.Tasks.Task<ServiceResult<TaskProgressReportDto>> CreateTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
    System.Threading.Tasks.Task<ServiceResult<TaskDto>> AddTaskProgressReportAsync(Guid taskId, CreateTaskProgressReportRequest request);
}
