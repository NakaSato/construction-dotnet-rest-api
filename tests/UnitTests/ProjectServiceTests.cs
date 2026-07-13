using AutoMapper;
using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

// Models namespace defines a `Task` entity that collides with System.Threading.Tasks.Task.
using Task = System.Threading.Tasks.Task;

namespace UnitTests;

/// <summary>
/// Covers <see cref="ProjectService"/> CRUD: create defaults + equipment/location
/// flattening, get/update/patch/delete not-found paths, and status parsing.
/// Each test uses an isolated in-memory database.
/// </summary>
public class ProjectServiceTests
{
    private static ProjectService NewService(out ApplicationDbContext ctx)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"project-{Guid.NewGuid()}")
            .Options;
        ctx = new ApplicationDbContext(options);

        var config = new MapperConfiguration(
            cfg => cfg.AddMaps(typeof(MappingProfile).Assembly),
            NullLoggerFactory.Instance);
        var mapper = config.CreateMapper();

        return new ProjectService(ctx, mapper);
    }

    private static CreateProjectRequest NewRequest(string name = "Solar Farm A") => new()
    {
        ProjectName = name,
        Address = "123 Sunlight Ave",
        ClientInfo = "ACME Energy",
        StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    };

    [Fact]
    public async Task Create_SetsPlanningStatus_AndPersists()
    {
        var svc = NewService(out var ctx);

        var result = await svc.CreateProjectAsync(NewRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Solar Farm A", result.Data!.ProjectName);
        var stored = await ctx.Projects.SingleAsync();
        Assert.Equal(ProjectStatus.Planning, stored.Status);
    }

    [Fact]
    public async Task Create_FlattensEquipmentAndLocation()
    {
        var svc = NewService(out var ctx);
        var req = NewRequest();
        req.EquipmentDetails = new EquipmentDetailsDto { Inverter125kw = 2, Inverter80kw = 3 };
        req.LocationCoordinates = new LocationCoordinatesDto { Latitude = 13.75m, Longitude = 100.5m };

        var result = await svc.CreateProjectAsync(req);

        Assert.True(result.IsSuccess);
        var stored = await ctx.Projects.SingleAsync();
        Assert.Equal(2, stored.Inverter125kw);
        Assert.Equal(3, stored.Inverter80kw);
        Assert.Equal(13.75m, stored.Latitude);
        Assert.Equal(100.5m, stored.Longitude);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.GetProjectByIdAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Project not found", result.Errors);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsProject()
    {
        var svc = NewService(out _);
        var created = (await svc.CreateProjectAsync(NewRequest())).Data!;

        var result = await svc.GetProjectByIdAsync(created.ProjectId);

        Assert.True(result.IsSuccess);
        Assert.Equal(created.ProjectId, result.Data!.ProjectId);
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.UpdateProjectAsync(Guid.NewGuid(), new UpdateProjectRequest
        {
            ProjectName = "X",
            Address = "somewhere",
            Status = "InProgress",
            StartDate = DateTime.UtcNow,
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Project not found", result.Errors);
    }

    [Fact]
    public async Task Update_Existing_ParsesStatusEnum()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateProjectAsync(NewRequest())).Data!;

        var result = await svc.UpdateProjectAsync(created.ProjectId, new UpdateProjectRequest
        {
            ProjectName = "Renamed",
            Address = "456 New Rd",
            Status = "InProgress",
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
        });

        Assert.True(result.IsSuccess);
        var stored = await ctx.Projects.FindAsync(created.ProjectId);
        Assert.Equal("Renamed", stored!.ProjectName);
        Assert.Equal(ProjectStatus.InProgress, stored.Status);
    }

    [Fact]
    public async Task Patch_OnlyUpdatesProvidedFields()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateProjectAsync(NewRequest("Original"))).Data!;

        var result = await svc.PatchProjectAsync(created.ProjectId, new PatchProjectRequest
        {
            Status = "OnHold", // ProjectName omitted -> unchanged
        }, "tester");

        Assert.True(result.IsSuccess);
        var stored = await ctx.Projects.FindAsync(created.ProjectId);
        Assert.Equal("Original", stored!.ProjectName); // untouched
        Assert.Equal(ProjectStatus.OnHold, stored.Status);
    }

    [Fact]
    public async Task Patch_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.PatchProjectAsync(Guid.NewGuid(),
            new PatchProjectRequest { ProjectName = "X" }, "tester");

        Assert.False(result.IsSuccess);
        Assert.Contains("Project not found", result.Errors);
    }

    [Fact]
    public async Task Delete_Existing_RemovesRow()
    {
        var svc = NewService(out var ctx);
        var created = (await svc.CreateProjectAsync(NewRequest())).Data!;

        var result = await svc.DeleteProjectAsync(created.ProjectId);

        Assert.True(result.IsSuccess);
        Assert.False(await ctx.Projects.AnyAsync());
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        var svc = NewService(out _);

        var result = await svc.DeleteProjectAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains("Project not found", result.Errors);
    }
}
