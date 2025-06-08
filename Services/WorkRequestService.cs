using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public class WorkRequestService : IWorkRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly IQueryService _queryService;

    public WorkRequestService(ApplicationDbContext context, IQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<ApiResponse<WorkRequestDto>> GetWorkRequestByIdAsync(Guid requestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                    .ThenInclude(c => c.Author)
                .Include(wr => wr.Images)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == requestId);

            if (workRequest == null)
            {
                return new ApiResponse<WorkRequestDto>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var workRequestDto = MapToDto(workRequest);

            return new ApiResponse<WorkRequestDto>
            {
                Success = true,
                Data = workRequestDto
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the work request",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters)
    {
        try
        {
            var query = _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                .Include(wr => wr.Images)
                .AsQueryable();

            // Apply filters
            if (parameters.ProjectId.HasValue)
                query = query.Where(wr => wr.ProjectId == parameters.ProjectId.Value);

            if (parameters.RequestedById.HasValue)
                query = query.Where(wr => wr.RequestedById == parameters.RequestedById.Value);

            if (parameters.AssignedToId.HasValue)
                query = query.Where(wr => wr.AssignedToId == parameters.AssignedToId.Value);

            if (!string.IsNullOrEmpty(parameters.Status))
                query = query.Where(wr => wr.Status.ToString().ToLower() == parameters.Status.ToLower());

            if (!string.IsNullOrEmpty(parameters.Priority))
                query = query.Where(wr => wr.Priority.ToString().ToLower() == parameters.Priority.ToLower());

            if (!string.IsNullOrEmpty(parameters.Type))
                query = query.Where(wr => wr.Type.ToString().ToLower() == parameters.Type.ToLower());

            if (!string.IsNullOrEmpty(parameters.Title))
                query = query.Where(wr => wr.Title.ToLower().Contains(parameters.Title.ToLower()));

            if (parameters.DueDateAfter.HasValue)
                query = query.Where(wr => wr.DueDate >= parameters.DueDateAfter.Value);

            if (parameters.DueDateBefore.HasValue)
                query = query.Where(wr => wr.DueDate <= parameters.DueDateBefore.Value);

            if (parameters.CreatedAfter.HasValue)
                query = query.Where(wr => wr.CreatedAt >= parameters.CreatedAfter.Value);

            if (parameters.CreatedBefore.HasValue)
                query = query.Where(wr => wr.CreatedAt <= parameters.CreatedBefore.Value);

            if (parameters.CompletedDateAfter.HasValue)
                query = query.Where(wr => wr.CompletedDate >= parameters.CompletedDateAfter.Value);

            if (parameters.CompletedDateBefore.HasValue)
                query = query.Where(wr => wr.CompletedDate <= parameters.CompletedDateBefore.Value);

            if (parameters.HasTasks.HasValue)
                query = query.Where(wr => wr.Tasks.Any() == parameters.HasTasks.Value);

            if (parameters.HasComments.HasValue)
                query = query.Where(wr => wr.Comments.Any() == parameters.HasComments.Value);

            var result = await _queryService.QueryAsync(query, parameters, MapToDto);

            return new ApiResponse<EnhancedPagedResult<WorkRequestDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EnhancedPagedResult<WorkRequestDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving work requests",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<WorkRequestDto>>> GetProjectWorkRequestsAsync(Guid projectId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                .Include(wr => wr.Images)
                .Where(wr => wr.ProjectId == projectId)
                .OrderByDescending(wr => wr.CreatedAt);

            var totalCount = await query.CountAsync();
            var workRequests = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var workRequestDtos = workRequests.Select(MapToDto).ToList();

            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = true,
                Data = new PagedResult<WorkRequestDto>
                {
                    Items = workRequestDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving project work requests",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PagedResult<WorkRequestDto>>> GetUserWorkRequestsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var query = _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                .Include(wr => wr.Images)
                .Where(wr => wr.RequestedById == userId || wr.AssignedToId == userId)
                .OrderByDescending(wr => wr.CreatedAt);

            var totalCount = await query.CountAsync();
            var workRequests = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var workRequestDtos = workRequests.Select(MapToDto).ToList();

            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = true,
                Data = new PagedResult<WorkRequestDto>
                {
                    Items = workRequestDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving user work requests",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid requestedById)
    {
        try
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
            {
                return new ApiResponse<WorkRequestDto>
                {
                    Success = false,
                    Message = "Project not found"
                };
            }

            // Validate assigned user exists if provided
            if (request.AssignedToId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(request.AssignedToId.Value);
                if (assignedUser == null)
                {
                    return new ApiResponse<WorkRequestDto>
                    {
                        Success = false,
                        Message = "Assigned user not found"
                    };
                }
            }

            var workRequest = new WorkRequest
            {
                WorkRequestId = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                RequestedById = requestedById,
                AssignedToId = request.AssignedToId,
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                Priority = request.Priority,
                Status = WorkRequestStatus.Open,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkRequests.Add(workRequest);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdWorkRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                .Include(wr => wr.Images)
                .FirstAsync(wr => wr.WorkRequestId == workRequest.WorkRequestId);

            return new ApiResponse<WorkRequestDto>
            {
                Success = true,
                Data = MapToDto(createdWorkRequest),
                Message = "Work request created successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestDto>
            {
                Success = false,
                Message = "An error occurred while creating the work request",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<WorkRequestDto>> UpdateWorkRequestAsync(Guid requestId, UpdateWorkRequestRequest request)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.Tasks)
                .Include(wr => wr.Comments)
                .Include(wr => wr.Images)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == requestId);

            if (workRequest == null)
            {
                return new ApiResponse<WorkRequestDto>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            // Validate assigned user exists if provided
            if (request.AssignedToId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(request.AssignedToId.Value);
                if (assignedUser == null)
                {
                    return new ApiResponse<WorkRequestDto>
                    {
                        Success = false,
                        Message = "Assigned user not found"
                    };
                }
            }

            // Update fields
            workRequest.AssignedToId = request.AssignedToId;
            workRequest.Title = request.Title;
            workRequest.Description = request.Description;
            workRequest.Type = request.Type;
            workRequest.Priority = request.Priority;
            workRequest.DueDate = request.DueDate;
            workRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<WorkRequestDto>
            {
                Success = true,
                Data = MapToDto(workRequest),
                Message = "Work request updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestDto>
            {
                Success = false,
                Message = "An error occurred while updating the work request",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteWorkRequestAsync(Guid requestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            // Only allow deletion if status is Open or Draft
            if (workRequest.Status != WorkRequestStatus.Open)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only open work requests can be deleted"
                };
            }

            _context.WorkRequests.Remove(workRequest);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the work request",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> AssignWorkRequestAsync(Guid requestId, Guid assignedToId)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            // Validate assigned user exists
            var assignedUser = await _context.Users.FindAsync(assignedToId);
            if (assignedUser == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Assigned user not found"
                };
            }

            workRequest.AssignedToId = assignedToId;
            if (workRequest.Status == WorkRequestStatus.Open)
            {
                workRequest.Status = WorkRequestStatus.InProgress;
            }
            workRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request assigned successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while assigning the work request",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateWorkRequestStatusAsync(Guid requestId, WorkRequestStatus status)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            workRequest.Status = status;
            if (status == WorkRequestStatus.Completed)
            {
                workRequest.CompletedDate = DateTime.UtcNow;
            }
            workRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request status updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the work request status",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateWorkRequestPriorityAsync(Guid requestId, WorkRequestPriority priority)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            workRequest.Priority = priority;
            workRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request priority updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the work request priority",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #region Work Request Tasks

    public async Task<ApiResponse<WorkRequestTaskDto>> AddWorkRequestTaskAsync(Guid requestId, CreateWorkRequestTaskRequest request)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<WorkRequestTaskDto>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var workRequestTask = new WorkRequestTask
            {
                WorkRequestTaskId = Guid.NewGuid(),
                WorkRequestId = requestId,
                Title = request.Title,
                Description = request.Description,
                Status = WorkRequestStatus.Open,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.WorkRequestTasks.Add(workRequestTask);
            await _context.SaveChangesAsync();

            return new ApiResponse<WorkRequestTaskDto>
            {
                Success = true,
                Data = MapWorkRequestTaskToDto(workRequestTask),
                Message = "Work request task added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestTaskDto>
            {
                Success = false,
                Message = "An error occurred while adding the work request task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<WorkRequestTaskDto>> UpdateWorkRequestTaskAsync(Guid taskId, UpdateWorkRequestTaskRequest request)
    {
        try
        {
            var task = await _context.WorkRequestTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<WorkRequestTaskDto>
                {
                    Success = false,
                    Message = "Work request task not found"
                };
            }

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<WorkRequestTaskDto>
            {
                Success = true,
                Data = MapWorkRequestTaskToDto(task),
                Message = "Work request task updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestTaskDto>
            {
                Success = false,
                Message = "An error occurred while updating the work request task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteWorkRequestTaskAsync(Guid taskId)
    {
        try
        {
            var task = await _context.WorkRequestTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request task not found"
                };
            }

            _context.WorkRequestTasks.Remove(task);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request task deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the work request task",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateWorkRequestTaskStatusAsync(Guid taskId, WorkRequestStatus status)
    {
        try
        {
            var task = await _context.WorkRequestTasks.FindAsync(taskId);
            if (task == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request task not found"
                };
            }

            task.Status = status;
            if (status == WorkRequestStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request task status updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the work request task status",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Work Request Comments

    public async Task<ApiResponse<WorkRequestCommentDto>> AddWorkRequestCommentAsync(Guid requestId, CreateWorkRequestCommentRequest request, Guid authorId)
    {
        try
        {
            var workRequest = await _context.WorkRequests.FindAsync(requestId);
            if (workRequest == null)
            {
                return new ApiResponse<WorkRequestCommentDto>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var comment = new WorkRequestComment
            {
                WorkRequestCommentId = Guid.NewGuid(),
                WorkRequestId = requestId,
                AuthorId = authorId,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkRequestComments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdComment = await _context.WorkRequestComments
                .Include(c => c.Author)
                .FirstAsync(c => c.WorkRequestCommentId == comment.WorkRequestCommentId);

            return new ApiResponse<WorkRequestCommentDto>
            {
                Success = true,
                Data = MapWorkRequestCommentToDto(createdComment),
                Message = "Comment added successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<WorkRequestCommentDto>
            {
                Success = false,
                Message = "An error occurred while adding the comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteWorkRequestCommentAsync(Guid commentId)
    {
        try
        {
            var comment = await _context.WorkRequestComments.FindAsync(commentId);
            if (comment == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Comment not found"
                };
            }

            _context.WorkRequestComments.Remove(comment);
            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Comment deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the comment",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    #endregion

    #region Mapping Methods

    private WorkRequestDto MapToDto(WorkRequest workRequest)
    {
        return new WorkRequestDto
        {
            WorkRequestId = workRequest.WorkRequestId,
            ProjectId = workRequest.ProjectId,
            ProjectName = workRequest.Project?.ProjectName,
            RequestedById = workRequest.RequestedById,
            RequestedByName = workRequest.RequestedBy?.FullName,
            AssignedToId = workRequest.AssignedToId,
            AssignedToName = workRequest.AssignedTo?.FullName,
            Title = workRequest.Title,
            Description = workRequest.Description,
            Type = workRequest.Type.ToString(),
            Priority = workRequest.Priority.ToString(),
            Status = workRequest.Status.ToString(),
            DueDate = workRequest.DueDate,
            CompletedDate = workRequest.CompletedDate,
            CreatedAt = workRequest.CreatedAt,
            UpdatedAt = workRequest.UpdatedAt,
            Tasks = workRequest.Tasks?.Select(MapWorkRequestTaskToDto).ToList() ?? new List<WorkRequestTaskDto>(),
            Comments = workRequest.Comments?.Select(MapWorkRequestCommentToDto).ToList() ?? new List<WorkRequestCommentDto>(),
            ImageCount = workRequest.Images?.Count ?? 0
        };
    }

    private WorkRequestTaskDto MapWorkRequestTaskToDto(WorkRequestTask task)
    {
        return new WorkRequestTaskDto
        {
            WorkRequestTaskId = task.WorkRequestTaskId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    private WorkRequestCommentDto MapWorkRequestCommentToDto(WorkRequestComment comment)
    {
        return new WorkRequestCommentDto
        {
            WorkRequestCommentId = comment.WorkRequestCommentId,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author?.FullName,
            Comment = comment.Comment,
            CreatedAt = comment.CreatedAt
        };
    }

    #endregion
}
