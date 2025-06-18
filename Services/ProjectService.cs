using dotnet_rest_api.DTOs;
using dotnet_rest_api.Data;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public ProjectService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
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

            var result = new EnhancedPagedResult<ProjectDto>
            {
                Items = projects,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
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
        try
        {
            // Check if project manager exists
            var projectManager = await _context.Users.FindAsync(request.ProjectManagerId);
            if (projectManager == null)
            {
                return ServiceResult<ProjectDto>.ErrorResult("Project manager not found");
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
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

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
                ProjectManager = new UserDto
                {
                    UserId = createdProject.ProjectManager.UserId,
                    Username = createdProject.ProjectManager.Username,
                    Email = createdProject.ProjectManager.Email,
                    FullName = createdProject.ProjectManager.FullName,
                    RoleName = createdProject.ProjectManager.Role.RoleName,
                    IsActive = createdProject.ProjectManager.IsActive
                },
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
            return ServiceResult<ProjectDto>.ErrorResult($"Error creating project: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProjectDto>> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
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
        try
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return ServiceResult<ProjectDto>.ErrorResult("Project not found");
            }

            // Apply partial updates only for non-null properties available in PatchProjectRequest
            if (!string.IsNullOrEmpty(request.ProjectName))
                project.ProjectName = request.ProjectName;

            if (!string.IsNullOrEmpty(request.Address))
                project.Address = request.Address;

            if (!string.IsNullOrEmpty(request.ClientInfo))
                project.ClientInfo = request.ClientInfo;

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ProjectStatus>(request.Status, true, out var statusEnum))
                {
                    project.Status = statusEnum;
                }
            }

            if (request.StartDate.HasValue)
                project.StartDate = request.StartDate.Value;

            if (request.EstimatedEndDate.HasValue)
                project.EstimatedEndDate = request.EstimatedEndDate.Value;

            if (request.ActualEndDate.HasValue)
                project.ActualEndDate = request.ActualEndDate.Value;

            if (request.ProjectManagerId.HasValue)
            {
                var newProjectManager = await _context.Users.FindAsync(request.ProjectManagerId.Value);
                if (newProjectManager == null)
                {
                    return ServiceResult<ProjectDto>.ErrorResult("New project manager not found");
                }
                project.ProjectManagerId = request.ProjectManagerId.Value;
            }

            await _context.SaveChangesAsync();

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

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

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
