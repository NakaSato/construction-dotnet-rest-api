using Microsoft.EntityFrameworkCore;
using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services.WBS;

/// <summary>
/// Implementation of WBS (Work Breakdown Structure) service operations
/// </summary>
public class WbsService : IWbsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WbsService> _logger;
    private readonly WbsDataSeeder _dataSeeder;

    public WbsService(
        ApplicationDbContext context, 
        IMapper mapper, 
        ILogger<WbsService> logger,
        WbsDataSeeder dataSeeder)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dataSeeder = dataSeeder;
    }

    public async Task<IEnumerable<WbsTaskDto>> GetAllTasksAsync(Guid projectId, string? installationArea = null, WbsTaskStatus? status = null)
    {
        var query = _context.WbsTasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedUser)
            .Include(t => t.Dependencies)
            .Include(t => t.Evidence)
            .AsQueryable();

        if (!string.IsNullOrEmpty(installationArea))
        {
            query = query.Where(t => t.InstallationArea == installationArea);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var tasks = await query.OrderBy(t => t.WbsId).ToListAsync();
        return _mapper.Map<IEnumerable<WbsTaskDto>>(tasks);
    }

    public async Task<WbsTaskDto?> GetTaskByIdAsync(string wbsId)
    {
        var task = await _context.WbsTasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Dependencies)
                .ThenInclude(d => d.PrerequisiteTask)
            .Include(t => t.DependentTasks)
                .ThenInclude(d => d.DependentTask)
            .Include(t => t.Evidence)
            .Include(t => t.ChildTasks)
            .FirstOrDefaultAsync(t => t.WbsId == wbsId);

        return task != null ? _mapper.Map<WbsTaskDto>(task) : null;
    }

    public async Task<IEnumerable<WbsTaskDto>> GetChildTasksAsync(string parentWbsId)
    {
        var childTasks = await _context.WbsTasks
            .Where(t => t.ParentWbsId == parentWbsId)
            .Include(t => t.AssignedUser)
            .Include(t => t.Evidence)
            .OrderBy(t => t.WbsId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<WbsTaskDto>>(childTasks);
    }

    public async Task<IEnumerable<WbsTaskHierarchyDto>> GetTaskHierarchyAsync(Guid projectId)
    {
        var allTasks = await _context.WbsTasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.AssignedUser)
            .Include(t => t.Evidence)
            .OrderBy(t => t.WbsId)
            .ToListAsync();

        // Build hierarchy starting with root tasks (no parent)
        var rootTasks = allTasks.Where(t => string.IsNullOrEmpty(t.ParentWbsId)).ToList();
        var hierarchy = new List<WbsTaskHierarchyDto>();

        foreach (var rootTask in rootTasks)
        {
            var hierarchyTask = _mapper.Map<WbsTaskHierarchyDto>(rootTask);
            BuildTaskHierarchy(hierarchyTask, allTasks);
            hierarchy.Add(hierarchyTask);
        }

        return hierarchy;
    }

    private void BuildTaskHierarchy(WbsTaskHierarchyDto parentTask, List<WbsTask> allTasks)
    {
        var childTasks = allTasks.Where(t => t.ParentWbsId == parentTask.WbsId).ToList();
        
        foreach (var childTask in childTasks)
        {
            var hierarchyChild = _mapper.Map<WbsTaskHierarchyDto>(childTask);
            BuildTaskHierarchy(hierarchyChild, allTasks);
            parentTask.Children.Add(hierarchyChild);
        }
    }

    public async Task<WbsTaskDto> CreateTaskAsync(CreateWbsTaskDto createDto)
    {
        // Validate parent task exists if specified
        if (!string.IsNullOrEmpty(createDto.ParentWbsId))
        {
            var parentExists = await _context.WbsTasks.AnyAsync(t => t.WbsId == createDto.ParentWbsId);
            if (!parentExists)
            {
                throw new ArgumentException($"Parent task with WBS ID '{createDto.ParentWbsId}' not found.");
            }
        }

        // Validate project exists
        var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == createDto.ProjectId);
        if (!projectExists)
        {
            throw new ArgumentException($"Project with ID '{createDto.ProjectId}' not found.");
        }

        var task = _mapper.Map<WbsTask>(createDto);
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.WbsTasks.Add(task);
        await _context.SaveChangesAsync();

        // Create dependencies if specified
        if (createDto.Dependencies?.Any() == true)
        {
            foreach (var dependencyWbsId in createDto.Dependencies)
            {
                var dependency = new WbsTaskDependency
                {
                    DependentTaskId = task.WbsId,
                    PrerequisiteTaskId = dependencyWbsId
                };
                _context.WbsTaskDependencies.Add(dependency);
            }
            await _context.SaveChangesAsync();
        }

        return await GetTaskByIdAsync(task.WbsId) ?? throw new InvalidOperationException("Failed to retrieve created task.");
    }

    public async Task<WbsTaskDto> UpdateTaskAsync(string wbsId, UpdateWbsTaskDto updateDto)
    {
        var task = await _context.WbsTasks.FindAsync(wbsId);
        if (task == null)
        {
            throw new ArgumentException($"Task with WBS ID '{wbsId}' not found.");
        }

        _mapper.Map(updateDto, task);
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(wbsId) ?? throw new InvalidOperationException("Failed to retrieve updated task.");
    }

    public async Task<bool> DeleteTaskAsync(string wbsId)
    {
        var task = await _context.WbsTasks
            .Include(t => t.ChildTasks)
            .FirstOrDefaultAsync(t => t.WbsId == wbsId);

        if (task == null)
        {
            return false;
        }

        // Prevent deletion if there are child tasks
        if (task.ChildTasks.Any())
        {
            throw new InvalidOperationException("Cannot delete task with child tasks. Delete child tasks first.");
        }

        // Remove dependencies
        var dependencies = await _context.WbsTaskDependencies
            .Where(d => d.DependentTaskId == wbsId || d.PrerequisiteTaskId == wbsId)
            .ToListAsync();
        _context.WbsTaskDependencies.RemoveRange(dependencies);

        // Remove evidence
        var evidence = await _context.WbsTaskEvidence
            .Where(e => e.WbsTaskId == wbsId)
            .ToListAsync();
        _context.WbsTaskEvidence.RemoveRange(evidence);

        _context.WbsTasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<WbsTaskDto> UpdateTaskStatusAsync(string wbsId, WbsTaskStatus status)
    {
        var task = await _context.WbsTasks.FindAsync(wbsId);
        if (task == null)
        {
            throw new ArgumentException($"Task with WBS ID '{wbsId}' not found.");
        }

        var oldStatus = task.Status;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        // Update actual start/end dates based on status changes
        if (status == WbsTaskStatus.InProgress && oldStatus == WbsTaskStatus.NotStarted)
        {
            task.ActualStartDate = DateTime.UtcNow;
        }
        else if (status == WbsTaskStatus.Completed && task.ActualEndDate == null)
        {
            task.ActualEndDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await GetTaskByIdAsync(wbsId) ?? throw new InvalidOperationException("Failed to retrieve updated task.");
    }

    public async Task<WbsTaskEvidenceDto> AddEvidenceAsync(string wbsId, CreateWbsTaskEvidenceDto evidenceDto)
    {
        var taskExists = await _context.WbsTasks.AnyAsync(t => t.WbsId == wbsId);
        if (!taskExists)
        {
            throw new ArgumentException($"Task with WBS ID '{wbsId}' not found.");
        }

        var evidence = _mapper.Map<WbsTaskEvidence>(evidenceDto);
        evidence.WbsTaskId = wbsId;
        evidence.CreatedAt = DateTime.UtcNow;

        _context.WbsTaskEvidence.Add(evidence);
        await _context.SaveChangesAsync();

        return _mapper.Map<WbsTaskEvidenceDto>(evidence);
    }

    public async Task<IEnumerable<WbsTaskEvidenceDto>> GetTaskEvidenceAsync(string wbsId)
    {
        var evidence = await _context.WbsTaskEvidence
            .Where(e => e.WbsTaskId == wbsId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<WbsTaskEvidenceDto>>(evidence);
    }

    public async System.Threading.Tasks.Task<WbsProjectProgressDto> CalculateProjectProgressAsync(Guid projectId)
    {
        var tasks = await _context.WbsTasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        if (!tasks.Any())
        {
            return new WbsProjectProgressDto
            {
                ProjectId = projectId,
                ProgressPercentage = 0,
                TotalTasks = 0,
                CompletedTasks = 0,
                StatusSummary = new Dictionary<string, int>
                {
                    { "NotStarted", 0 },
                    { "InProgress", 0 },
                    { "Completed", 0 },
                    { "OnHold", 0 }
                }
            };
        }

        var totalWeight = tasks.Sum(t => t.WeightPercent);
        var completedWeight = tasks.Where(t => t.Status == WbsTaskStatus.Completed).Sum(t => t.WeightPercent);
        var overallProgress = totalWeight > 0 ? (completedWeight / totalWeight) * 100 : 0;

        return new WbsProjectProgressDto
        {
            ProjectId = projectId,
            ProgressPercentage = Math.Round(overallProgress, 2),
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == WbsTaskStatus.Completed),
            StatusSummary = new Dictionary<string, int>
            {
                { "NotStarted", tasks.Count(t => t.Status == WbsTaskStatus.NotStarted) },
                { "InProgress", tasks.Count(t => t.Status == WbsTaskStatus.InProgress) },
                { "Completed", tasks.Count(t => t.Status == WbsTaskStatus.Completed) },
                { "OnHold", tasks.Count(t => t.Status == WbsTaskStatus.OnHold) }
            }
        };
    }

    public async Task<bool> CanStartTaskAsync(string wbsId)
    {
        var dependencies = await _context.WbsTaskDependencies
            .Include(d => d.PrerequisiteTask)
            .Where(d => d.DependentTaskId == wbsId)
            .ToListAsync();

        return dependencies.All(d => d.PrerequisiteTask.Status == WbsTaskStatus.Completed);
    }

    public async Task<IEnumerable<WbsTaskDto>> GetReadyToStartTasksAsync(Guid projectId)
    {
        var allTasks = await _context.WbsTasks
            .Where(t => t.ProjectId == projectId && t.Status == WbsTaskStatus.NotStarted)
            .Include(t => t.Dependencies)
                .ThenInclude(d => d.PrerequisiteTask)
            .ToListAsync();

        var readyTasks = new List<WbsTask>();

        foreach (var task in allTasks)
        {
            var canStart = task.Dependencies.All(d => d.PrerequisiteTask.Status == WbsTaskStatus.Completed);
            if (canStart)
            {
                readyTasks.Add(task);
            }
        }

        return _mapper.Map<IEnumerable<WbsTaskDto>>(readyTasks);
    }

    public async Task<IEnumerable<WbsTaskDto>> GetCriticalPathTasksAsync(Guid projectId)
    {
        // This is a simplified critical path calculation
        // In a full implementation, you would use algorithms like CPM (Critical Path Method)
        var allTasks = await _context.WbsTasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Dependencies)
                .ThenInclude(d => d.PrerequisiteTask)
            .Include(t => t.DependentTasks)
                .ThenInclude(d => d.DependentTask)
            .ToListAsync();

        // For now, return tasks that have dependencies and are on the longest path
        var criticalTasks = allTasks
            .Where(t => t.Dependencies.Any() || t.DependentTasks.Any())
            .OrderBy(t => t.PlannedStartDate)
            .ToList();

        return _mapper.Map<IEnumerable<WbsTaskDto>>(criticalTasks);
    }

    public async System.Threading.Tasks.Task SeedSampleDataAsync(Guid projectId)
    {
        await _dataSeeder.SeedWbsDataAsync(projectId);
    }
}
