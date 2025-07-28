using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services.WBS;

/// <summary>
/// Interface for WBS (Work Breakdown Structure) service operations
/// </summary>
public interface IWbsService
{
    /// <summary>
    /// Gets all WBS tasks for a project with optional filtering
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskDto>> GetAllTasksAsync(Guid projectId, string? installationArea = null, WbsTaskStatus? status = null);

    /// <summary>
    /// Gets a specific WBS task by its WBS ID
    /// </summary>
    System.Threading.Tasks.Task<WbsTaskDto?> GetTaskByIdAsync(string wbsId);

    /// <summary>
    /// Gets all child tasks for a specific parent WBS task
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskDto>> GetChildTasksAsync(string parentWbsId);

    /// <summary>
    /// Gets the hierarchical tree structure of all WBS tasks for a project
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskHierarchyDto>> GetTaskHierarchyAsync(Guid projectId);

    /// <summary>
    /// Creates a new WBS task
    /// </summary>
    System.Threading.Tasks.Task<WbsTaskDto> CreateTaskAsync(CreateWbsTaskDto createDto);

    /// <summary>
    /// Updates an existing WBS task
    /// </summary>
    System.Threading.Tasks.Task<WbsTaskDto> UpdateTaskAsync(string wbsId, UpdateWbsTaskDto updateDto);

    /// <summary>
    /// Deletes a WBS task (only if it has no child tasks)
    /// </summary>
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(string wbsId);

    /// <summary>
    /// Updates the status of a WBS task
    /// </summary>
    System.Threading.Tasks.Task<WbsTaskDto> UpdateTaskStatusAsync(string wbsId, WbsTaskStatus status);

    /// <summary>
    /// Adds evidence (photos, documents) to a WBS task
    /// </summary>
    System.Threading.Tasks.Task<WbsTaskEvidenceDto> AddEvidenceAsync(string wbsId, CreateWbsTaskEvidenceDto evidenceDto);

    /// <summary>
    /// Gets all evidence for a specific WBS task
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskEvidenceDto>> GetTaskEvidenceAsync(string wbsId);

    /// <summary>
    /// Calculates the overall progress percentage for a project based on WBS task completion
    /// </summary>
    System.Threading.Tasks.Task<WbsProjectProgressDto> CalculateProjectProgressAsync(Guid projectId);

    /// <summary>
    /// Validates that a WBS task can be started (all dependencies are completed)
    /// </summary>
    System.Threading.Tasks.Task<bool> CanStartTaskAsync(string wbsId);

    /// <summary>
    /// Gets tasks that are ready to start (dependencies completed but not yet started)
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskDto>> GetReadyToStartTasksAsync(Guid projectId);

    /// <summary>
    /// Gets critical path tasks for the project
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<WbsTaskDto>> GetCriticalPathTasksAsync(Guid projectId);

    /// <summary>
    /// Seeds sample WBS data for a project (development/testing purposes)
    /// </summary>
    System.Threading.Tasks.Task SeedSampleDataAsync(Guid projectId);
}
