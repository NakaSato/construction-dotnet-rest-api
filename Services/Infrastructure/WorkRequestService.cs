using dotnet_rest_api.Common;
using AutoMapper;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services.Infrastructure;

public interface IWorkRequestService
{
    Task<Result<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters);
    Task<Result<WorkRequestDto>> GetWorkRequestByIdAsync(Guid id);
    Task<Result<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request);
    Task<Result<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid userId);
    Task<Result<WorkRequestDto>> UpdateWorkRequestAsync(Guid id, UpdateWorkRequestRequest request);
    Task<Result<bool>> DeleteWorkRequestAsync(Guid id);
    Task<Result<bool>> AssignWorkRequestAsync(Guid id, Guid userId);
    Task<Result<WorkRequestDto>> CompleteWorkRequestAsync(Guid id);
}

public interface IWorkRequestApprovalService
{
    Task<Result<bool>> ApproveWorkRequestAsync(Guid id, string approverId);
    Task<Result<bool>> RejectWorkRequestAsync(Guid id, string approverId, string reason);
    Task<Result<bool>> SubmitForApprovalAsync(Guid id, SubmitForApprovalRequest request, Guid userId);
    Task<Result<bool>> ProcessApprovalAsync(Guid id, ApprovalRequest request, Guid userId);
    Task<Result<ApprovalWorkflowStatusDto>> GetApprovalStatusAsync(Guid id);
}

/// <summary>
/// Real EF Core-backed implementation of <see cref="IWorkRequestService"/>.
/// Replaces the former StubWorkRequestService. Covers CRUD, assignment and
/// completion. The multi-level approval workflow lives in
/// <see cref="WorkRequestApprovalService"/>.
/// </summary>
public class WorkRequestService : IWorkRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notifications;
    private readonly ILogger<WorkRequestService> _logger;

    public WorkRequestService(
        ApplicationDbContext context,
        IMapper mapper,
        INotificationService notifications,
        ILogger<WorkRequestService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notifications = notifications;
        _logger = logger;
    }

    /// <summary>Best-effort work-request notification; never breaks the core operation.</summary>
    private async System.Threading.Tasks.Task SafeNotifyAsync(
        Guid workRequestId, Guid? recipientId, string type, string subject, string message, Guid? senderId)
    {
        if (!recipientId.HasValue || recipientId.Value == Guid.Empty)
            return;
        try
        {
            await _notifications.CreateWorkRequestNotificationAsync(
                workRequestId, recipientId.Value, type, subject, message, senderId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send {Type} notification for work request {WorkRequestId}", type, workRequestId);
        }
    }

    public async Task<Result<EnhancedPagedResult<WorkRequestDto>>> GetWorkRequestsAsync(WorkRequestQueryParameters parameters)
    {
        var query = BuildBaseQuery();

        if (parameters.ProjectId.HasValue)
            query = query.Where(w => w.ProjectId == parameters.ProjectId.Value);
        if (parameters.RequestedById.HasValue)
            query = query.Where(w => w.RequestedById == parameters.RequestedById.Value);
        if (parameters.AssignedToId.HasValue)
            query = query.Where(w => w.AssignedToId == parameters.AssignedToId.Value);
        if (!string.IsNullOrWhiteSpace(parameters.Status) &&
            Enum.TryParse<WorkRequestStatus>(parameters.Status, true, out var status))
            query = query.Where(w => w.Status == status);
        if (!string.IsNullOrWhiteSpace(parameters.Priority) &&
            Enum.TryParse<WorkRequestPriority>(parameters.Priority, true, out var priority))
            query = query.Where(w => w.Priority == priority);
        if (!string.IsNullOrWhiteSpace(parameters.Type) &&
            Enum.TryParse<WorkRequestType>(parameters.Type, true, out var type))
            query = query.Where(w => w.Type == type);
        if (!string.IsNullOrWhiteSpace(parameters.Title))
            query = query.Where(w => w.Title.Contains(parameters.Title));
        if (parameters.DueDateAfter.HasValue)
            query = query.Where(w => w.DueDate >= parameters.DueDateAfter.Value);
        if (parameters.DueDateBefore.HasValue)
            query = query.Where(w => w.DueDate <= parameters.DueDateBefore.Value);
        if (parameters.CreatedAfter.HasValue)
            query = query.Where(w => w.CreatedAt >= parameters.CreatedAfter.Value);
        if (parameters.CreatedBefore.HasValue)
            query = query.Where(w => w.CreatedAt <= parameters.CreatedBefore.Value);
        if (parameters.UpdatedAfter.HasValue)
            query = query.Where(w => w.UpdatedAt >= parameters.UpdatedAfter.Value);
        if (parameters.UpdatedBefore.HasValue)
            query = query.Where(w => w.UpdatedAt <= parameters.UpdatedBefore.Value);
        if (parameters.CompletedDateAfter.HasValue)
            query = query.Where(w => w.CompletedDate >= parameters.CompletedDateAfter.Value);
        if (parameters.CompletedDateBefore.HasValue)
            query = query.Where(w => w.CompletedDate <= parameters.CompletedDateBefore.Value);
        if (parameters.HasTasks == true)
            query = query.Where(w => w.Tasks.Any());
        if (parameters.HasComments == true)
            query = query.Where(w => w.Comments.Any());

        query = ApplySorting(query, parameters.SortBy, parameters.SortOrder);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var result = new EnhancedPagedResult<WorkRequestDto>
        {
            Items = await ToDtosAsync(items),
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };

        return Result<EnhancedPagedResult<WorkRequestDto>>.SuccessResult(result, "Work requests retrieved successfully");
    }

    public async Task<Result<WorkRequestDto>> GetWorkRequestByIdAsync(Guid id)
    {
        var workRequest = await BuildBaseQuery().FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<WorkRequestDto>.NotFoundResult("Work request not found");

        return Result<WorkRequestDto>.SuccessResult(await ToDtoAsync(workRequest), "Work request retrieved successfully");
    }

    public Task<Result<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request)
        => CreateWorkRequestAsync(request, Guid.Empty);

    public async Task<Result<WorkRequestDto>> CreateWorkRequestAsync(CreateWorkRequestRequest request, Guid userId)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.ProjectId == request.ProjectId);
        if (!projectExists)
            return Result<WorkRequestDto>.ErrorResult("Project not found");

        if (request.AssignedToId.HasValue &&
            !await _context.Users.AnyAsync(u => u.UserId == request.AssignedToId.Value))
            return Result<WorkRequestDto>.ErrorResult("Assigned user not found");

        var workRequest = _mapper.Map<WorkRequest>(request);
        workRequest.WorkRequestId = Guid.NewGuid();
        workRequest.RequestedById = userId;
        workRequest.Status = WorkRequestStatus.Open;
        workRequest.RequestedDate = DateTime.UtcNow;
        workRequest.CreatedAt = DateTime.UtcNow;

        _context.WorkRequests.Add(workRequest);
        await _context.SaveChangesAsync();

        var created = await BuildBaseQuery().FirstOrDefaultAsync(w => w.WorkRequestId == workRequest.WorkRequestId);
        if (created == null)
            return Result<WorkRequestDto>.ErrorResult("Failed to load created work request");
        return Result<WorkRequestDto>.SuccessResult(await ToDtoAsync(created), "Work request created successfully");
    }

    public async Task<Result<WorkRequestDto>> UpdateWorkRequestAsync(Guid id, UpdateWorkRequestRequest request)
    {
        var workRequest = await _context.WorkRequests.FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<WorkRequestDto>.NotFoundResult("Work request not found");

        if (request.AssignedToId.HasValue &&
            !await _context.Users.AnyAsync(u => u.UserId == request.AssignedToId.Value))
            return Result<WorkRequestDto>.ErrorResult("Assigned user not found");

        _mapper.Map(request, workRequest);

        // Status is ignored by the mapper (DTO allows "Pending", not an enum member);
        // apply it here only when it parses to a real WorkRequestStatus.
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<WorkRequestStatus>(request.Status, true, out var status))
            workRequest.Status = status;

        workRequest.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var updated = await BuildBaseQuery().FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (updated == null)
            return Result<WorkRequestDto>.ErrorResult("Failed to load updated work request");
        return Result<WorkRequestDto>.SuccessResult(await ToDtoAsync(updated), "Work request updated successfully");
    }

    public async Task<Result<bool>> DeleteWorkRequestAsync(Guid id)
    {
        var workRequest = await _context.WorkRequests
            .Include(w => w.Tasks)
            .Include(w => w.Comments)
            .Include(w => w.ApprovalHistory)
            .Include(w => w.Notifications)
            .FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<bool>.NotFoundResult("Work request not found");

        _context.WorkRequestTasks.RemoveRange(workRequest.Tasks);
        _context.WorkRequestComments.RemoveRange(workRequest.Comments);
        _context.WorkRequestApprovals.RemoveRange(workRequest.ApprovalHistory);
        _context.WorkRequestNotifications.RemoveRange(workRequest.Notifications);
        _context.WorkRequests.Remove(workRequest);
        await _context.SaveChangesAsync();

        return Result<bool>.SuccessResult(true, "Work request deleted successfully");
    }

    public async Task<Result<bool>> AssignWorkRequestAsync(Guid id, Guid userId)
    {
        var workRequest = await _context.WorkRequests.FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<bool>.NotFoundResult("Work request not found");

        if (!await _context.Users.AnyAsync(u => u.UserId == userId))
            return Result<bool>.ErrorResult("Assigned user not found");

        workRequest.AssignedToId = userId;
        // Moving an open request to a person starts it; approved requests likewise.
        if (workRequest.Status is WorkRequestStatus.Open or WorkRequestStatus.Approved)
        {
            workRequest.Status = WorkRequestStatus.InProgress;
            workRequest.StartedAt ??= DateTime.UtcNow;
        }
        workRequest.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await SafeNotifyAsync(workRequest.WorkRequestId, userId, nameof(NotificationType.WorkRequestAssigned),
            "Work request assigned", $"You have been assigned work request '{workRequest.Title}'.", null);

        return Result<bool>.SuccessResult(true, "Work request assigned successfully");
    }

    public async Task<Result<WorkRequestDto>> CompleteWorkRequestAsync(Guid id)
    {
        var workRequest = await _context.WorkRequests.FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<WorkRequestDto>.NotFoundResult("Work request not found");

        if (workRequest.Status is WorkRequestStatus.Completed or WorkRequestStatus.Cancelled)
            return Result<WorkRequestDto>.ErrorResult($"Cannot complete a work request with status '{workRequest.Status}'");

        workRequest.Status = WorkRequestStatus.Completed;
        workRequest.CompletedDate = DateTime.UtcNow;
        workRequest.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.RequestedById, nameof(NotificationType.WorkRequestCompleted),
            "Work request completed", $"Work request '{workRequest.Title}' has been marked completed.", null);

        var completed = await BuildBaseQuery().FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (completed == null)
            return Result<WorkRequestDto>.ErrorResult("Failed to load completed work request");
        return Result<WorkRequestDto>.SuccessResult(await ToDtoAsync(completed), "Work request completed successfully");
    }

    // -------------------------------------------------------------- Helpers --

    private IQueryable<WorkRequest> BuildBaseQuery()
    {
        // Only collection navigations (left-join safe) are eager-loaded. The required
        // references Project/RequestedBy — and nullable AssignedTo/approvers — are
        // resolved in-memory by FillNamesAsync so an orphaned FK never drops a row.
        return _context.WorkRequests
            .Include(w => w.Tasks)
            .Include(w => w.Comments)
            .Include(w => w.Images)
            .AsQueryable();
    }

    private async Task<WorkRequestDto> ToDtoAsync(WorkRequest workRequest)
    {
        var dto = _mapper.Map<WorkRequestDto>(workRequest);
        await FillNamesAsync(new[] { dto });
        return dto;
    }

    private async Task<List<WorkRequestDto>> ToDtosAsync(List<WorkRequest> workRequests)
    {
        var dtos = _mapper.Map<List<WorkRequestDto>>(workRequests);
        await FillNamesAsync(dtos);
        return dtos;
    }

    /// <summary>
    /// Backfills project and user display names by loading the referenced rows
    /// separately (avoids INNER JOINs that would drop rows with orphaned FKs) and
    /// computes derived approval fields. Only names left empty by mapping are filled.
    /// </summary>
    private async System.Threading.Tasks.Task FillNamesAsync(IReadOnlyList<WorkRequestDto> dtos)
    {
        if (dtos.Count == 0)
            return;

        var projectIds = dtos.Select(d => d.ProjectId).Distinct().ToList();
        var userIds = dtos
            .SelectMany(d => new[] { (Guid?)d.RequestedById, d.AssignedToId, d.ManagerApproverId, d.AdminApproverId })
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.ProjectId))
            .Select(p => new { p.ProjectId, p.ProjectName })
            .ToDictionaryAsync(p => p.ProjectId, p => p.ProjectName);

        var users = await _context.Users
            .Where(u => userIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.FullName })
            .ToDictionaryAsync(u => u.UserId, u => u.FullName);

        string? NameOf(Guid? id) => id.HasValue && users.TryGetValue(id.Value, out var n) ? n : null;

        foreach (var dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.ProjectName) && projects.TryGetValue(dto.ProjectId, out var projectName))
                dto.ProjectName = projectName;
            if (string.IsNullOrEmpty(dto.RequestedByName))
                dto.RequestedByName = NameOf(dto.RequestedById) ?? dto.RequestedByName;
            if (string.IsNullOrEmpty(dto.AssignedToName))
                dto.AssignedToName = NameOf(dto.AssignedToId) ?? dto.AssignedToName;
            if (string.IsNullOrEmpty(dto.ManagerApproverName))
                dto.ManagerApproverName = NameOf(dto.ManagerApproverId) ?? dto.ManagerApproverName;
            if (string.IsNullOrEmpty(dto.AdminApproverName))
                dto.AdminApproverName = NameOf(dto.AdminApproverId) ?? dto.AdminApproverName;

            dto.ImageCount = dto.Images.Count;
            dto.DaysPendingApproval = WorkRequestApprovalHelpers.DaysPending(dto.SubmittedForApprovalDate, dto.Status);
            dto.CurrentApproverName = WorkRequestApprovalHelpers.CurrentApproverName(dto.Status, dto.ManagerApproverName, dto.AdminApproverName);
        }
    }

    private static IQueryable<WorkRequest> ApplySorting(IQueryable<WorkRequest> query, string? sortBy, string? sortOrder)
    {
        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => descending ? query.OrderByDescending(w => w.Title) : query.OrderBy(w => w.Title),
            "priority" => descending ? query.OrderByDescending(w => w.Priority) : query.OrderBy(w => w.Priority),
            "status" => descending ? query.OrderByDescending(w => w.Status) : query.OrderBy(w => w.Status),
            "duedate" => descending ? query.OrderByDescending(w => w.DueDate) : query.OrderBy(w => w.DueDate),
            "updatedat" => descending ? query.OrderByDescending(w => w.UpdatedAt) : query.OrderBy(w => w.UpdatedAt),
            "createdat" => descending ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt),
            _ => query.OrderByDescending(w => w.CreatedAt)
        };
    }
}

/// <summary>
/// Real EF Core-backed implementation of <see cref="IWorkRequestApprovalService"/>.
/// Implements the two-level (Manager → Admin) approval workflow, recording every
/// transition as a <see cref="WorkRequestApproval"/> history row.
/// </summary>
public class WorkRequestApprovalService : IWorkRequestApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly INotificationService _notifications;
    private readonly ILogger<WorkRequestApprovalService> _logger;

    public WorkRequestApprovalService(
        ApplicationDbContext context,
        IMapper mapper,
        INotificationService notifications,
        ILogger<WorkRequestApprovalService> logger)
    {
        _context = context;
        _mapper = mapper;
        _notifications = notifications;
        _logger = logger;
    }

    /// <summary>
    /// Fire-and-persist a work-request notification without letting a notification
    /// failure roll back or break the approval workflow. Skips unknown recipients.
    /// </summary>
    private async System.Threading.Tasks.Task SafeNotifyAsync(
        Guid workRequestId, Guid? recipientId, string type, string subject, string message, Guid? senderId)
    {
        if (!recipientId.HasValue || recipientId.Value == Guid.Empty)
            return;
        try
        {
            await _notifications.CreateWorkRequestNotificationAsync(
                workRequestId, recipientId.Value, type, subject, message, senderId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send {Type} notification for work request {WorkRequestId}", type, workRequestId);
        }
    }

    public async Task<Result<bool>> SubmitForApprovalAsync(Guid id, SubmitForApprovalRequest request, Guid userId)
    {
        var workRequest = await _context.WorkRequests.FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<bool>.NotFoundResult("Work request not found");

        if (workRequest.Status is not (WorkRequestStatus.Open or WorkRequestStatus.Rejected or WorkRequestStatus.OnHold))
            return Result<bool>.ErrorResult($"Cannot submit a work request with status '{workRequest.Status}' for approval");

        if (request.RequiresAdminApproval)
            workRequest.RequiresAdminApproval = true;
        if (request.PreferredManagerId.HasValue)
            workRequest.ManagerApproverId = request.PreferredManagerId.Value;

        var previousStatus = workRequest.Status;
        // Reset any prior decision so a resubmission starts a clean workflow.
        workRequest.ManagerApprovalDate = null;
        workRequest.AdminApprovalDate = null;
        workRequest.RejectionReason = string.Empty;
        workRequest.IsAutoApproved = false;
        workRequest.SubmittedForApprovalDate = DateTime.UtcNow;

        WorkRequestStatus newStatus;
        ApprovalAction action;
        ApprovalLevel level;
        if (workRequest.RequiresManagerApproval)
        {
            newStatus = WorkRequestStatus.PendingManagerApproval;
            action = ApprovalAction.Submitted;
            level = ApprovalLevel.Manager;
        }
        else if (workRequest.RequiresAdminApproval)
        {
            newStatus = WorkRequestStatus.PendingAdminApproval;
            action = ApprovalAction.Submitted;
            level = ApprovalLevel.Admin;
        }
        else
        {
            newStatus = WorkRequestStatus.Approved;
            action = ApprovalAction.AutoApproved;
            level = ApprovalLevel.None;
            workRequest.IsAutoApproved = true;
        }

        workRequest.Status = newStatus;
        workRequest.UpdatedAt = DateTime.UtcNow;
        AddHistory(workRequest, userId, action, level, previousStatus, newStatus, request.Comments, null, null);

        await _context.SaveChangesAsync();

        // Notify the pending approver that a decision is required.
        if (newStatus == WorkRequestStatus.PendingManagerApproval)
            await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.ManagerApproverId, nameof(NotificationType.ApprovalRequired),
                "Approval required", $"Work request '{workRequest.Title}' is awaiting your approval.", userId);
        else if (newStatus == WorkRequestStatus.PendingAdminApproval)
            await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.AdminApproverId, nameof(NotificationType.ApprovalRequired),
                "Approval required", $"Work request '{workRequest.Title}' is awaiting your approval.", userId);

        return Result<bool>.SuccessResult(true, "Work request submitted for approval");
    }

    public async Task<Result<bool>> ProcessApprovalAsync(Guid id, ApprovalRequest request, Guid userId)
    {
        var workRequest = await _context.WorkRequests.FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<bool>.NotFoundResult("Work request not found");

        if (workRequest.Status is not (WorkRequestStatus.PendingManagerApproval or WorkRequestStatus.PendingAdminApproval))
            return Result<bool>.ErrorResult($"Work request is not pending approval (status '{workRequest.Status}')");

        var level = workRequest.Status == WorkRequestStatus.PendingManagerApproval
            ? ApprovalLevel.Manager
            : ApprovalLevel.Admin;
        var previousStatus = workRequest.Status;

        WorkRequestStatus newStatus;
        ApprovalAction action;
        string? rejectionReason = null;
        string? escalationReason = null;

        switch (request.Action?.ToLowerInvariant())
        {
            case "approve":
                if (level == ApprovalLevel.Manager)
                {
                    workRequest.ManagerApproverId = userId;
                    workRequest.ManagerApprovalDate = DateTime.UtcNow;
                    workRequest.ManagerComments = request.Comments;
                    newStatus = workRequest.RequiresAdminApproval
                        ? WorkRequestStatus.PendingAdminApproval
                        : WorkRequestStatus.Approved;
                    action = ApprovalAction.ManagerApproved;
                }
                else
                {
                    workRequest.AdminApproverId = userId;
                    workRequest.AdminApprovalDate = DateTime.UtcNow;
                    workRequest.AdminComments = request.Comments;
                    newStatus = WorkRequestStatus.Approved;
                    action = ApprovalAction.AdminApproved;
                }
                break;

            case "reject":
                newStatus = WorkRequestStatus.Rejected;
                rejectionReason = string.IsNullOrWhiteSpace(request.RejectionReason) ? request.Comments : request.RejectionReason;
                workRequest.RejectionReason = rejectionReason;
                action = level == ApprovalLevel.Manager ? ApprovalAction.ManagerRejected : ApprovalAction.AdminRejected;
                break;

            case "escalate":
                // Escalate the decision to admin level.
                workRequest.RequiresAdminApproval = true;
                if (request.EscalateToUserId.HasValue)
                    workRequest.AdminApproverId = request.EscalateToUserId.Value;
                newStatus = WorkRequestStatus.PendingAdminApproval;
                escalationReason = request.EscalationReason;
                action = ApprovalAction.Escalated;
                break;

            default:
                return Result<bool>.ErrorResult("Action must be one of: Approve, Reject, Escalate");
        }

        workRequest.Status = newStatus;
        workRequest.UpdatedAt = DateTime.UtcNow;
        AddHistory(workRequest, userId, action, level, previousStatus, newStatus, request.Comments, rejectionReason, escalationReason);

        await _context.SaveChangesAsync();

        // Notify the relevant party of the outcome.
        switch (newStatus)
        {
            case WorkRequestStatus.PendingAdminApproval:
                await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.AdminApproverId,
                    action == ApprovalAction.Escalated ? nameof(NotificationType.WorkRequestEscalated) : nameof(NotificationType.ApprovalRequired),
                    action == ApprovalAction.Escalated ? "Work request escalated" : "Approval required",
                    $"Work request '{workRequest.Title}' requires admin approval.", userId);
                break;
            case WorkRequestStatus.Approved:
                await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.RequestedById, nameof(NotificationType.WorkRequestApproved),
                    "Work request approved", $"Your work request '{workRequest.Title}' was approved.", userId);
                break;
            case WorkRequestStatus.Rejected:
                await SafeNotifyAsync(workRequest.WorkRequestId, workRequest.RequestedById, nameof(NotificationType.WorkRequestRejected),
                    "Work request rejected", $"Your work request '{workRequest.Title}' was rejected. Reason: {workRequest.RejectionReason}", userId);
                break;
        }

        return Result<bool>.SuccessResult(true, $"Work request {action}");
    }

    public async Task<Result<bool>> ApproveWorkRequestAsync(Guid id, string approverId)
    {
        if (!Guid.TryParse(approverId, out var userId))
            return Result<bool>.ErrorResult("Invalid approver ID");
        return await ProcessApprovalAsync(id, new ApprovalRequest { Action = "Approve" }, userId);
    }

    public async Task<Result<bool>> RejectWorkRequestAsync(Guid id, string approverId, string reason)
    {
        if (!Guid.TryParse(approverId, out var userId))
            return Result<bool>.ErrorResult("Invalid approver ID");
        return await ProcessApprovalAsync(id, new ApprovalRequest { Action = "Reject", RejectionReason = reason }, userId);
    }

    public async Task<Result<ApprovalWorkflowStatusDto>> GetApprovalStatusAsync(Guid id)
    {
        var workRequest = await _context.WorkRequests
            .Include(w => w.ApprovalHistory)
            .FirstOrDefaultAsync(w => w.WorkRequestId == id);
        if (workRequest == null)
            return Result<ApprovalWorkflowStatusDto>.NotFoundResult("Work request not found");

        var history = workRequest.ApprovalHistory.OrderBy(h => h.CreatedAt).ToList();
        var approverIds = history.Select(h => h.ApproverId)
            .Concat(new[] { workRequest.ManagerApproverId ?? Guid.Empty, workRequest.AdminApproverId ?? Guid.Empty })
            .Where(g => g != Guid.Empty)
            .Distinct()
            .ToList();

        var names = await _context.Users
            .Where(u => approverIds.Contains(u.UserId))
            .Select(u => new { u.UserId, u.FullName })
            .ToDictionaryAsync(u => u.UserId, u => u.FullName);

        string? NameOf(Guid? gid) => gid.HasValue && names.TryGetValue(gid.Value, out var n) ? n : null;

        var historyDtos = _mapper.Map<List<WorkRequestApprovalDto>>(history);
        foreach (var h in historyDtos)
            if (string.IsNullOrEmpty(h.ApproverName))
                h.ApproverName = NameOf(h.ApproverId) ?? string.Empty;

        var dto = new ApprovalWorkflowStatusDto
        {
            WorkRequestId = workRequest.WorkRequestId,
            Title = workRequest.Title,
            CurrentStatus = workRequest.Status.ToString(),
            RequiresManagerApproval = workRequest.RequiresManagerApproval,
            RequiresAdminApproval = workRequest.RequiresAdminApproval,
            IsManagerApproved = workRequest.ManagerApprovalDate.HasValue,
            IsAdminApproved = workRequest.AdminApprovalDate.HasValue,
            CurrentApproverName = WorkRequestApprovalHelpers.CurrentApproverName(
                workRequest.Status, NameOf(workRequest.ManagerApproverId), NameOf(workRequest.AdminApproverId)),
            NextApproverName = workRequest.Status == WorkRequestStatus.PendingManagerApproval && workRequest.RequiresAdminApproval
                ? NameOf(workRequest.AdminApproverId)
                : null,
            ApprovalHistory = historyDtos,
            SubmittedForApprovalDate = workRequest.SubmittedForApprovalDate,
            LastActionDate = history.Count > 0 ? history[^1].CreatedAt : workRequest.UpdatedAt,
            DaysPendingApproval = WorkRequestApprovalHelpers.DaysPending(workRequest.SubmittedForApprovalDate, workRequest.Status.ToString())
        };

        return Result<ApprovalWorkflowStatusDto>.SuccessResult(dto, "Approval status retrieved successfully");
    }

    private void AddHistory(
        WorkRequest workRequest, Guid approverId, ApprovalAction action, ApprovalLevel level,
        WorkRequestStatus previousStatus, WorkRequestStatus newStatus,
        string? comments, string? rejectionReason, string? escalationReason)
    {
        _context.WorkRequestApprovals.Add(new WorkRequestApproval
        {
            ApprovalId = Guid.NewGuid(),
            WorkRequestId = workRequest.WorkRequestId,
            ApproverId = approverId,
            Action = action,
            Level = level,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Comments = comments ?? string.Empty,
            RejectionReason = rejectionReason ?? string.Empty,
            EscalationReason = escalationReason ?? string.Empty,
            EscalationDate = action == ApprovalAction.Escalated ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            IsActive = true
        });
    }
}

/// <summary>Shared derived-field helpers for work-request approval status.</summary>
internal static class WorkRequestApprovalHelpers
{
    public static int DaysPending(DateTime? submittedForApprovalDate, string status)
    {
        var pending = status is nameof(WorkRequestStatus.PendingManagerApproval) or nameof(WorkRequestStatus.PendingAdminApproval);
        if (!pending || !submittedForApprovalDate.HasValue)
            return 0;
        return Math.Max(0, (int)(DateTime.UtcNow - submittedForApprovalDate.Value).TotalDays);
    }

    public static string? CurrentApproverName(string status, string? managerApproverName, string? adminApproverName)
        => status switch
        {
            nameof(WorkRequestStatus.PendingManagerApproval) => managerApproverName,
            nameof(WorkRequestStatus.PendingAdminApproval) => adminApproverName,
            _ => null
        };

    public static string? CurrentApproverName(WorkRequestStatus status, string? managerApproverName, string? adminApproverName)
        => CurrentApproverName(status.ToString(), managerApproverName, adminApproverName);
}
