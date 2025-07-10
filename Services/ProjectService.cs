using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;
    private readonly INotificationService _notificationService;

    public ProjectService(ApplicationDbContext context, IQueryService queryService, INotificationService notificationService)
    {
        _context = context;
        _queryService = queryService;
        _notificationService = notificationService;
    }

    public async Task<ServiceResult<EnhancedPagedResult<ProjectDto>>> GetProjectsAsync(ProjectQueryParameters parameters)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .AsQueryable();

            // Apply filtering based on available properties
            if (!string.IsNullOrEmpty(parameters.ProjectName))
            {
                query = query.Where(p => p.ProjectName.Contains(parameters.ProjectName));
            }

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                if (Enum.TryParse<ProjectStatus>(parameters.Status, true, out var statusEnum))
                {
                    query = query.Where(p => p.Status == statusEnum);
                }
            }

            if (parameters.ManagerId.HasValue)
            {
                query = query.Where(p => p.ProjectManagerId == parameters.ManagerId.Value);
            }

            if (!string.IsNullOrEmpty(parameters.Address))
            {
                query = query.Where(p => p.Address.Contains(parameters.Address));
            }

            if (!string.IsNullOrEmpty(parameters.ClientInfo))
            {
                query = query.Where(p => p.ClientInfo.Contains(parameters.ClientInfo));
            }

            // Apply date filtering
            if (parameters.StartDateAfter.HasValue)
            {
                query = query.Where(p => p.StartDate >= parameters.StartDateAfter.Value);
            }

            if (parameters.StartDateBefore.HasValue)
            {
                query = query.Where(p => p.StartDate <= parameters.StartDateBefore.Value);
            }

            var totalCount = await query.CountAsync();

            // Calculate project statistics for enhanced response
            var allProjectsQuery = _context.Projects.AsQueryable();
            var projectStats = new ProjectStatistics
            {
                TotalProjects = await allProjectsQuery.CountAsync(),
                ActiveProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.InProgress),
                CompletedProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Completed),
                PlanningProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Planning),
                OnHoldProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.OnHold),
                CancelledProjects = await allProjectsQuery.CountAsync(p => p.Status == ProjectStatus.Cancelled),
                TotalCapacityKw = await allProjectsQuery.SumAsync(p => p.TotalCapacityKw ?? 0),
                TotalPvModules = await allProjectsQuery.SumAsync(p => p.PvModuleCount ?? 0),
                TotalFtsValue = await allProjectsQuery.SumAsync(p => p.FtsValue ?? 0),
                TotalRevenueValue = await allProjectsQuery.SumAsync(p => p.RevenueValue ?? 0),
                TotalPqmValue = await allProjectsQuery.SumAsync(p => p.PqmValue ?? 0),
                ProjectManagerCount = await allProjectsQuery.Select(p => p.ProjectManagerId).Distinct().CountAsync(),
                GeographicCoverage = "Multiple provinces and regions",
                LastUpdated = DateTime.UtcNow
            };

            // Apply pagination
            var projects = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Address = p.Address,
                    ClientInfo = p.ClientInfo,
                    Status = p.Status.ToString(),
                    StartDate = p.StartDate,
                    EstimatedEndDate = p.EstimatedEndDate,
                    ActualEndDate = p.ActualEndDate,
                    ProjectManager = p.ProjectManager != null ? new UserDto
                    {
                        UserId = p.ProjectManager.UserId,
                        Username = p.ProjectManager.Username,
                        Email = p.ProjectManager.Email,
                        FullName = p.ProjectManager.FullName,
                        RoleName = p.ProjectManager.Role != null ? p.ProjectManager.Role.RoleName : "Unknown",
                        IsActive = p.ProjectManager.IsActive
                    } : null,
                    Team = p.Team,
                    ConnectionType = p.ConnectionType,
                    ConnectionNotes = p.ConnectionNotes,
                    TotalCapacityKw = p.TotalCapacityKw,
                    PvModuleCount = p.PvModuleCount,
                    FtsValue = p.FtsValue,
                    RevenueValue = p.RevenueValue,
                    PqmValue = p.PqmValue,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var result = new EnhancedPagedResult<ProjectDto>
            {
                Items = projects,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                Statistics = projectStats
            };

            return ServiceResult<EnhancedPagedResult<ProjectDto>>.SuccessResult(result, "Projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<EnhancedPagedResult<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetProjectsLegacyAsync(int pageNumber, int pageSize, Guid? managerId)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .AsQueryable();

            if (managerId.HasValue)
            {
                query = query.Where(p => p.ProjectManagerId == managerId.Value);
            }

            var totalCount = await query.CountAsync();

            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Address = p.Address,
                    ClientInfo = p.ClientInfo,
                    Status = p.Status.ToString(),
                    StartDate = p.StartDate,
                    EstimatedEndDate = p.EstimatedEndDate,
                    ActualEndDate = p.ActualEndDate,
                    ProjectManager = new UserDto
                    {
                        UserId = p.ProjectManager.UserId,
                        Username = p.ProjectManager.Username,
                        Email = p.ProjectManager.Email,
                        FullName = p.ProjectManager.FullName,
                        RoleName = p.ProjectManager.Role.RoleName,
                        IsActive = p.ProjectManager.IsActive
                    },
                    Team = p.Team,
                    ConnectionType = p.ConnectionType,
                    ConnectionNotes = p.ConnectionNotes,
                    TotalCapacityKw = p.TotalCapacityKw,
                    PvModuleCount = p.PvModuleCount,
                    FtsValue = p.FtsValue,
                    RevenueValue = p.RevenueValue,
                    PqmValue = p.PqmValue,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResult<ProjectDto>
            {
                Items = projects,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result, "Projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");
            }

            var projectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Address = project.Address,
                ClientInfo = project.ClientInfo,
                Status = project.Status.ToString(),
                StartDate = project.StartDate,
                EstimatedEndDate = project.EstimatedEndDate,
                ActualEndDate = project.ActualEndDate,
                ProjectManager = new UserDto
                {
                    UserId = project.ProjectManager.UserId,
                    Username = project.ProjectManager.Username,
                    Email = project.ProjectManager.Email,
                    FullName = project.ProjectManager.FullName,
                    RoleName = project.ProjectManager.Role.RoleName,
                    IsActive = project.ProjectManager.IsActive
                },
                Team = project.Team,
                ConnectionType = project.ConnectionType,
                ConnectionNotes = project.ConnectionNotes,
                TotalCapacityKw = project.TotalCapacityKw,
                PvModuleCount = project.PvModuleCount,
                FtsValue = project.FtsValue,
                RevenueValue = project.RevenueValue,
                PqmValue = project.PqmValue,
                CreatedAt = project.CreatedAt
            };

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProjectDto>.ErrorResult($"Error retrieving project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request)
    {
        return await CreateProjectAsync(request, "System");
    }

    public async Task<ServiceResult<ProjectDto>> CreateProjectAsync(CreateProjectRequest request, string? createdBy)
    {
        try
        {
            // Check if project manager exists (only if provided)
            if (request.ProjectManagerId.HasValue)
            {
                var projectManager = await _context.Users.FindAsync(request.ProjectManagerId.Value);
                if (projectManager == null)
                {
                    return ServiceResult<ProjectDto>.ErrorResult("Project manager not found");
                }
            }

            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                ProjectName = request.ProjectName,
                Address = request.Address,
                ClientInfo = request.ClientInfo,
                Status = ProjectStatus.Planning,
                StartDate = request.StartDate,
                EstimatedEndDate = request.EstimatedEndDate,
                ProjectManagerId = request.ProjectManagerId,
                Team = request.Team,
                ConnectionType = request.ConnectionType,
                ConnectionNotes = request.ConnectionNotes,
                TotalCapacityKw = request.TotalCapacityKw,
                PvModuleCount = request.PvModuleCount,
                FtsValue = request.FtsValue,
                RevenueValue = request.RevenueValue,
                PqmValue = request.PqmValue,
                // Map equipment details
                Inverter125kw = request.EquipmentDetails?.Inverter125kw ?? 0,
                Inverter80kw = request.EquipmentDetails?.Inverter80kw ?? 0,
                Inverter60kw = request.EquipmentDetails?.Inverter60kw ?? 0,
                Inverter40kw = request.EquipmentDetails?.Inverter40kw ?? 0,
                // Map location coordinates
                Latitude = request.LocationCoordinates?.Latitude,
                Longitude = request.LocationCoordinates?.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Send enhanced real-time notification for project creation
            await _notificationService.SendEnhancedProjectCreatedNotificationAsync(
                project.ProjectId,
                project.ProjectName,
                project.Address,
                project.Status,
                project.Latitude,
                project.Longitude,
                createdBy ?? "System"
            );

            // Update dashboard statistics
            await _notificationService.SendDashboardStatsUpdatedNotificationAsync();

            // Fetch the created project with related data
            var createdProject = await _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .FirstAsync(p => p.ProjectId == project.ProjectId);

            var projectDto = new ProjectDto
            {
                ProjectId = createdProject.ProjectId,
                ProjectName = createdProject.ProjectName,
                Address = createdProject.Address,
                ClientInfo = createdProject.ClientInfo,
                Status = createdProject.Status.ToString(),
                StartDate = createdProject.StartDate,
                EstimatedEndDate = createdProject.EstimatedEndDate,
                ActualEndDate = createdProject.ActualEndDate,
                ProjectManager = createdProject.ProjectManager != null ? new UserDto
                {
                    UserId = createdProject.ProjectManager.UserId,
                    Username = createdProject.ProjectManager.Username,
                    Email = createdProject.ProjectManager.Email,
                    FullName = createdProject.ProjectManager.FullName,
                    RoleName = createdProject.ProjectManager.Role?.RoleName ?? "Unknown",
                    IsActive = createdProject.ProjectManager.IsActive
                } : null,
                Team = createdProject.Team,
                ConnectionType = createdProject.ConnectionType,
                ConnectionNotes = createdProject.ConnectionNotes,
                TotalCapacityKw = createdProject.TotalCapacityKw,
                PvModuleCount = createdProject.PvModuleCount,
                FtsValue = createdProject.FtsValue,
                RevenueValue = createdProject.RevenueValue,
                PqmValue = createdProject.PqmValue,
                CreatedAt = createdProject.CreatedAt
            };

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project created successfully");
        }
        catch (Exception ex)
        {
            // Log detailed exception information for debugging
            var innerException = ex.InnerException?.Message ?? "No inner exception";
            var fullMessage = $"Error creating project: {ex.Message}. Inner exception: {innerException}";
            return ServiceResult<ProjectDto>.ErrorResult(fullMessage);
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        return await UpdateProjectAsync(id, request, "System");
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request, string? updatedBy)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");
            }

            // Check if new project manager exists
            if (request.ProjectManagerId != project.ProjectManagerId)
            {
                var newProjectManager = await _context.Users.FindAsync(request.ProjectManagerId);
                if (newProjectManager == null)
                {
                    return ServiceResult<ProjectDto>.ErrorResult("New project manager not found");
                }
            }

            // Store old values for comparison
            var oldStatus = project.Status;
            var oldAddress = project.Address;
            var oldLatitude = project.Latitude;
            var oldLongitude = project.Longitude;

            // Update project properties (only those available in UpdateProjectRequest)
            project.ProjectName = request.ProjectName;
            project.Address = request.Address;
            project.ClientInfo = request.ClientInfo;
            project.StartDate = request.StartDate;
            project.EstimatedEndDate = request.EstimatedEndDate;
            project.ActualEndDate = request.ActualEndDate;
            project.ProjectManagerId = request.ProjectManagerId;

            // Parse and set status
            if (Enum.TryParse<ProjectStatus>(request.Status, true, out var statusEnum))
            {
                project.Status = statusEnum;
            }

            await _context.SaveChangesAsync();

            // Send real-time notifications for changes
            var changedFields = new Dictionary<string, object>();
            
            // Check for status changes
            if (oldStatus != project.Status)
            {
                changedFields.Add("Status", new { Old = oldStatus.ToString(), New = project.Status.ToString() });
                await _notificationService.SendProjectStatusChangedNotificationAsync(
                    project.ProjectId,
                    project.ProjectName,
                    oldStatus,
                    project.Status,
                    project.ActualEndDate,
                    null, // completion percentage - could be calculated if needed
                    updatedBy ?? "System"
                );
            }

            // Check for location/address changes
            if (oldAddress != project.Address || oldLatitude != project.Latitude || oldLongitude != project.Longitude)
            {
                changedFields.Add("Location", new { 
                    Old = new { Address = oldAddress, Latitude = oldLatitude, Longitude = oldLongitude },
                    New = new { Address = project.Address, Latitude = project.Latitude, Longitude = project.Longitude }
                });
                await _notificationService.SendProjectLocationUpdatedNotificationAsync(
                    project.ProjectId,
                    project.ProjectName,
                    project.Address,
                    project.Latitude,
                    project.Longitude,
                    updatedBy ?? "System"
                );
            }

            // Send comprehensive update notification
            await _notificationService.SendEnhancedProjectUpdatedNotificationAsync(
                project.ProjectId,
                project.ProjectName,
                project.Address,
                project.Status,
                project.Latitude,
                project.Longitude,
                project.ActualEndDate,
                null, // completion percentage
                updatedBy ?? "System",
                changedFields
            );

            // Update dashboard statistics if status changed
            if (oldStatus != project.Status)
            {
                await _notificationService.SendDashboardStatsUpdatedNotificationAsync();
            }

            // Refresh the project with updated manager data
            await _context.Entry(project).Reference(p => p.ProjectManager).LoadAsync();
            await _context.Entry(project.ProjectManager).Reference(pm => pm.Role).LoadAsync();

            var projectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Address = project.Address,
                ClientInfo = project.ClientInfo,
                Status = project.Status.ToString(),
                StartDate = project.StartDate,
                EstimatedEndDate = project.EstimatedEndDate,
                ActualEndDate = project.ActualEndDate,
                ProjectManager = new UserDto
                {
                    UserId = project.ProjectManager.UserId,
                    Username = project.ProjectManager.Username,
                    Email = project.ProjectManager.Email,
                    FullName = project.ProjectManager.FullName,
                    RoleName = project.ProjectManager.Role.RoleName,
                    IsActive = project.ProjectManager.IsActive
                },
                Team = project.Team,
                ConnectionType = project.ConnectionType,
                ConnectionNotes = project.ConnectionNotes,
                TotalCapacityKw = project.TotalCapacityKw,
                PvModuleCount = project.PvModuleCount,
                FtsValue = project.FtsValue,
                RevenueValue = project.RevenueValue,
                PqmValue = project.PqmValue,
                CreatedAt = project.CreatedAt
            };

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProjectDto>.ErrorResult($"Error updating project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request)
    {
        return await PatchProjectAsync(id, request, "System");
    }

    public async Task<ServiceResult<ProjectDto>> PatchProjectAsync(Guid id, PatchProjectRequest request, string? updatedBy)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");
            }

            // Store old values for change tracking
            var oldStatus = project.Status;
            var oldAddress = project.Address;
            var changedFields = new Dictionary<string, object>();

            // Apply partial updates only for non-null properties available in PatchProjectRequest
            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                changedFields.Add("ProjectName", new { Old = project.ProjectName, New = request.ProjectName });
                project.ProjectName = request.ProjectName;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                changedFields.Add("Address", new { Old = project.Address, New = request.Address });
                project.Address = request.Address;
            }

            if (!string.IsNullOrEmpty(request.ClientInfo))
            {
                changedFields.Add("ClientInfo", new { Old = project.ClientInfo, New = request.ClientInfo });
                project.ClientInfo = request.ClientInfo;
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ProjectStatus>(request.Status, true, out var statusEnum))
                {
                    if (project.Status != statusEnum)
                    {
                        changedFields.Add("Status", new { Old = project.Status.ToString(), New = statusEnum.ToString() });
                    }
                    project.Status = statusEnum;
                }
            }

            if (request.StartDate.HasValue)
            {
                changedFields.Add("StartDate", new { Old = project.StartDate, New = request.StartDate.Value });
                project.StartDate = request.StartDate.Value;
            }

            if (request.EstimatedEndDate.HasValue)
            {
                changedFields.Add("EstimatedEndDate", new { Old = project.EstimatedEndDate, New = request.EstimatedEndDate.Value });
                project.EstimatedEndDate = request.EstimatedEndDate.Value;
            }

            if (request.ActualEndDate.HasValue)
            {
                changedFields.Add("ActualEndDate", new { Old = project.ActualEndDate, New = request.ActualEndDate.Value });
                project.ActualEndDate = request.ActualEndDate.Value;
            }

            if (request.ProjectManagerId.HasValue)
            {
                var newProjectManager = await _context.Users.FindAsync(request.ProjectManagerId.Value);
                if (newProjectManager == null)
                {
                    return ServiceResult<ProjectDto>.ErrorResult("New project manager not found");
                }
                changedFields.Add("ProjectManagerId", new { Old = project.ProjectManagerId, New = request.ProjectManagerId.Value });
                project.ProjectManagerId = request.ProjectManagerId.Value;
            }

            await _context.SaveChangesAsync();

            // Send real-time notifications for specific changes
            if (oldStatus != project.Status)
            {
                await _notificationService.SendProjectStatusChangedNotificationAsync(
                    project.ProjectId,
                    project.ProjectName,
                    oldStatus,
                    project.Status,
                    project.ActualEndDate,
                    null,
                    updatedBy ?? "System"
                );
            }

            if (oldAddress != project.Address)
            {
                await _notificationService.SendProjectLocationUpdatedNotificationAsync(
                    project.ProjectId,
                    project.ProjectName,
                    project.Address,
                    project.Latitude,
                    project.Longitude,
                    updatedBy ?? "System"
                );
            }

            // Send comprehensive patch notification
            if (changedFields.Any())
            {
                await _notificationService.SendEnhancedProjectUpdatedNotificationAsync(
                    project.ProjectId,
                    project.ProjectName,
                    project.Address,
                    project.Status,
                    project.Latitude,
                    project.Longitude,
                    project.ActualEndDate,
                    null,
                    updatedBy ?? "System",
                    changedFields
                );

                // Update dashboard if status changed
                if (oldStatus != project.Status)
                {
                    await _notificationService.SendDashboardStatsUpdatedNotificationAsync();
                }
            }

            // Refresh the project with updated manager data
            await _context.Entry(project).Reference(p => p.ProjectManager).LoadAsync();
            await _context.Entry(project.ProjectManager).Reference(pm => pm.Role).LoadAsync();

            var projectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Address = project.Address,
                ClientInfo = project.ClientInfo,
                Status = project.Status.ToString(),
                StartDate = project.StartDate,
                EstimatedEndDate = project.EstimatedEndDate,
                ActualEndDate = project.ActualEndDate,
                ProjectManager = new UserDto
                {
                    UserId = project.ProjectManager.UserId,
                    Username = project.ProjectManager.Username,
                    Email = project.ProjectManager.Email,
                    FullName = project.ProjectManager.FullName,
                    RoleName = project.ProjectManager.Role.RoleName,
                    IsActive = project.ProjectManager.IsActive
                },
                Team = project.Team,
                ConnectionType = project.ConnectionType,
                ConnectionNotes = project.ConnectionNotes,
                TotalCapacityKw = project.TotalCapacityKw,
                PvModuleCount = project.PvModuleCount,
                FtsValue = project.FtsValue,
                RevenueValue = project.RevenueValue,
                PqmValue = project.PqmValue,
                CreatedAt = project.CreatedAt
            };

            return ServiceResult<ProjectDto>.SuccessResult(projectDto, "Project patched successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<ProjectDto>.ErrorResult($"Error patching project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id)
    {
        return await DeleteProjectAsync(id, "System");
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(Guid id, string? deletedBy)
    {
        try
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return ServiceResult<bool>.ErrorResult("Project not found");
            }

            // Check if project has dependent records (tasks, reports, etc.)
            var hasTaskReferences = await _context.Tasks.AnyAsync(t => t.ProjectId == id);
            if (hasTaskReferences)
            {
                return ServiceResult<bool>.ErrorResult("Cannot delete project with existing tasks. Please delete all related tasks first.");
            }

            // Store project name for notification before deletion
            var projectName = project.ProjectName;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            // Send real-time deletion notification
            await _notificationService.SendProjectDeletedNotificationAsync(projectName, deletedBy ?? "System");

            // Send universal entity deletion event
            await _notificationService.SendDashboardStatsUpdatedNotificationAsync();

            return ServiceResult<bool>.SuccessResult(true, "Project deleted successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Error deleting project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<PagedResult<ProjectDto>>> GetUserProjectsAsync(Guid userId, int pageNumber, int pageSize)
    {
        try
        {
            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return ServiceResult<PagedResult<ProjectDto>>.ErrorResult("User not found");
            }

            var query = _context.Projects
                .Include(p => p.ProjectManager)
                .ThenInclude(pm => pm.Role)
                .Where(p => p.ProjectManagerId == userId);

            var totalCount = await query.CountAsync();

            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Address = p.Address,
                    ClientInfo = p.ClientInfo,
                    Status = p.Status.ToString(),
                    StartDate = p.StartDate,
                    EstimatedEndDate = p.EstimatedEndDate,
                    ActualEndDate = p.ActualEndDate,
                    ProjectManager = new UserDto
                    {
                        UserId = p.ProjectManager.UserId,
                        Username = p.ProjectManager.Username,
                        Email = p.ProjectManager.Email,
                        FullName = p.ProjectManager.FullName,
                        RoleName = p.ProjectManager.Role.RoleName,
                        IsActive = p.ProjectManager.IsActive
                    },
                    Team = p.Team,
                    ConnectionType = p.ConnectionType,
                    ConnectionNotes = p.ConnectionNotes,
                    TotalCapacityKw = p.TotalCapacityKw,
                    PvModuleCount = p.PvModuleCount,
                    FtsValue = p.FtsValue,
                    RevenueValue = p.RevenueValue,
                    PqmValue = p.PqmValue,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResult<ProjectDto>
            {
                Items = projects,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<ProjectDto>>.SuccessResult(result, "User projects retrieved successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<PagedResult<ProjectDto>>.ErrorResult($"Error retrieving user projects: {ex.Message}");
        }
    }
}
