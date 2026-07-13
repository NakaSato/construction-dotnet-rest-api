using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

// Models namespace defines a `Task` entity that collides with System.Threading.Tasks.Task.
using Task = System.Threading.Tasks.Task;
using TaskStatus = dotnet_rest_api.Models.TaskStatus;

namespace UnitTests;

/// <summary>
/// Covers <see cref="TaskService"/> create/get/update/patch/delete, status
/// enum round-trip, and assign/unassign. Each test uses an isolated in-memory
/// database. TaskService maps DTOs manually (no AutoMapper) and looks tasks up
/// by <c>ProjectTasks.FindAsync</c>, so no navigation seeding is required.
/// </summary>
public class TaskServiceTests
{
    private static TaskService NewService(out ApplicationDbContext ctx)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"task-{Guid.NewGuid()}")
            .Options;
        ctx = new ApplicationDbContext(options);
        return new TaskService(ctx, NullLogger<TaskService>.Instance);
    }

    private static CreateTaskRequest NewRequest(string title = "Wire inverters") => new()
    {
        Title = title,
        Description = "Connect DC strings",
        DueDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public async Task Create_DefaultsToNotStarted_AndPersists()
    {
        var svc = NewService(out var ctx);

        var result = await svc.CreateTaskAsync(NewRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("NotStarted", result.Data!.Status);
        var stored = await ctx.ProjectTasks.SingleAsync();
        Assert.Equal(TaskStatus.NotStarted, stored.Status);
    }

    [Fact]
    public async Task CreateWithProjectId_SetsProjectId()
    {
        var svc = NewService(out _);
        var projectId = Guid.NewGuid();

        var result = await svc.CreateTaskAsync(projectId, NewRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(projectId, result.Data!.ProjectId);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.GetTaskByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Task not found", result.Errors);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsTask()
    {
        var svc = NewService(out _);
        var created = (await svc.CreateTaskAsync(NewRequest())).Data!;

        var result = await svc.GetTaskByIdAsync(created.TaskId);

        Assert.True(result.IsSuccess);
        Assert.Equal(created.TaskId, result.Data!.TaskId);
    }

    [Fact]
    public async Task Update_ParsesStatusEnum()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateTaskAsync(NewRequest())).Data!;

        var result = await svc.UpdateTaskAsync(created.TaskId, new UpdateTaskRequest
        {
            Title = "Renamed",
            Description = "updated",
            Status = "InProgress",
            DueDate = null,
        });

        Assert.True(result.IsSuccess);
        var stored = await ctx.ProjectTasks.FindAsync(created.TaskId);
        Assert.Equal("Renamed", stored!.Title);
        Assert.Equal(TaskStatus.InProgress, stored.Status);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.UpdateTaskAsync(Guid.NewGuid(), new UpdateTaskRequest
        {
            Title = "X",
            Description = "",
            Status = "InProgress",
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Task not found", result.Errors);
    }

    [Fact]
    public async Task Patch_UpdatesStatus_KeepsOtherFieldsFromRequest()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateTaskAsync(NewRequest("Original"))).Data!;

        // Patch redirects to Update; Title provided so it is applied.
        var result = await svc.PatchTaskAsync(created.TaskId, new PatchTaskRequest
        {
            Title = "Patched",
            Status = "Completed",
        });

        Assert.True(result.IsSuccess);
        var stored = await ctx.ProjectTasks.FindAsync(created.TaskId);
        Assert.Equal("Patched", stored!.Title);
        Assert.Equal(TaskStatus.Completed, stored.Status);
    }

    [Fact]
    public async Task UpdateStatus_ByInt_CastsToEnum()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateTaskAsync(NewRequest())).Data!;

        var result = await svc.UpdateTaskStatusAsync(created.TaskId, 2); // Completed

        Assert.True(result.IsSuccess);
        Assert.Equal("Completed", result.Data!.Status);
        var stored = await ctx.ProjectTasks.FindAsync(created.TaskId);
        Assert.Equal(TaskStatus.Completed, stored!.Status);
    }

    [Fact]
    public async Task Assign_ThenUnassign_TogglesTechnician()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateTaskAsync(NewRequest())).Data!;
        var tech = Guid.NewGuid();

        Assert.True((await svc.AssignTaskAsync(created.TaskId, tech)).IsSuccess);
        Assert.Equal(tech, (await ctx.ProjectTasks.FindAsync(created.TaskId))!.AssignedTechnicianId);

        Assert.True((await svc.UnassignTaskAsync(created.TaskId)).IsSuccess);
        Assert.Null((await ctx.ProjectTasks.FindAsync(created.TaskId))!.AssignedTechnicianId);
    }

    [Fact]
    public async Task Assign_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.AssignTaskAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Task not found", result.Errors);
    }

    [Fact]
    public async Task Delete_Existing_RemovesRow()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateTaskAsync(NewRequest())).Data!;

        var result = await svc.DeleteTaskAsync(created.TaskId);

        Assert.True(result.IsSuccess);
        Assert.False(await ctx.ProjectTasks.AnyAsync());
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.DeleteTaskAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Task not found", result.Errors);
    }

    [Fact]
    public async Task GetTasksByProjectId_FiltersByProject()
    {
        var svc = NewService(out _);
        var projectId = Guid.NewGuid();
        await svc.CreateTaskAsync(projectId, NewRequest("A"));
        await svc.CreateTaskAsync(projectId, NewRequest("B"));
        await svc.CreateTaskAsync(Guid.NewGuid(), NewRequest("Other"));

        var result = await svc.GetTasksByProjectIdAsync(projectId);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data!.Count);
    }
}
