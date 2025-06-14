using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public class WorkRequestApprovalService : IWorkRequestApprovalService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WorkRequestApprovalService> _logger;
    private readonly IConfiguration _configuration;

    public WorkRequestApprovalService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<WorkRequestApprovalService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ApiResponse<bool>> SubmitForApprovalAsync(
        Guid workRequestId, 
        SubmitForApprovalRequest request, 
        Guid submitterId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            // Verify user can submit for approval
            if (workRequest.RequestedById != submitterId)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Only the requestor can submit for approval"
                };
            }

            if (workRequest.Status != WorkRequestStatus.Open)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request must be in Open status to submit for approval"
                };
            }

            // Determine approval requirements
            var requiresManagerApproval = ShouldRequireManagerApproval(workRequest);
            var requiresAdminApproval = request.RequiresAdminApproval || ShouldRequireAdminApproval(workRequest);

            // Check for auto-approval
            var autoApprovalThreshold = decimal.Parse(_configuration["WorkRequest:AutoApprovalThreshold"] ?? "1000");
            var isAutoApproved = workRequest.EstimatedCost.HasValue && 
                                workRequest.EstimatedCost.Value <= autoApprovalThreshold &&
                                !requiresAdminApproval;

            if (isAutoApproved)
            {
                workRequest.Status = WorkRequestStatus.Approved;
                workRequest.IsAutoApproved = true;
                
                // Create approval record
                var autoApproval = new WorkRequestApproval
                {
                    ApprovalId = Guid.NewGuid(),
                    WorkRequestId = workRequestId,
                    ApproverId = submitterId, // System auto-approval
                    Action = ApprovalAction.AutoApproved,
                    Level = ApprovalLevel.None,
                    PreviousStatus = WorkRequestStatus.Open,
                    NewStatus = WorkRequestStatus.Approved,
                    Comments = "Auto-approved based on cost threshold",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.WorkRequestApprovals.Add(autoApproval);
            }
            else
            {
                workRequest.Status = requiresManagerApproval ? 
                    WorkRequestStatus.PendingManagerApproval : 
                    WorkRequestStatus.PendingAdminApproval;
                
                workRequest.RequiresManagerApproval = requiresManagerApproval;
                workRequest.RequiresAdminApproval = requiresAdminApproval;
                
                // Set preferred manager if provided
                if (request.PreferredManagerId.HasValue)
                {
                    var manager = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserId == request.PreferredManagerId.Value);
                    
                    if (manager?.Role.RoleName == "Manager")
                    {
                        workRequest.ManagerApproverId = request.PreferredManagerId.Value;
                    }
                }
                
                // If no manager assigned, find project manager or any manager
                if (workRequest.ManagerApproverId == null && requiresManagerApproval)
                {                var projectManager = await _context.Projects
                    .Where(p => p.ProjectId == workRequest.ProjectId)
                    .Select(p => p.ProjectManagerId)
                    .FirstOrDefaultAsync();
                         if (projectManager != Guid.Empty)
                {
                    workRequest.ManagerApproverId = projectManager;
                    }
                    else
                    {
                        // Find any available manager
                        var anyManager = await _context.Users
                            .Include(u => u.Role)
                            .Where(u => u.Role.RoleName == "Manager" && u.IsActive)
                            .Select(u => u.UserId)
                            .FirstOrDefaultAsync();
                        
                        workRequest.ManagerApproverId = anyManager;
                    }
                }

                // Create submission approval record
                var submissionApproval = new WorkRequestApproval
                {
                    ApprovalId = Guid.NewGuid(),
                    WorkRequestId = workRequestId,
                    ApproverId = submitterId,
                    Action = ApprovalAction.Submitted,
                    Level = ApprovalLevel.None,
                    PreviousStatus = WorkRequestStatus.Open,
                    NewStatus = workRequest.Status,
                    Comments = request.Comments ?? "Submitted for approval",
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.WorkRequestApprovals.Add(submissionApproval);
            }

            workRequest.SubmittedForApprovalDate = DateTime.UtcNow;
            workRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send notifications
            if (isAutoApproved)
            {
                await _notificationService.SendWorkRequestNotificationAsync(
                    workRequestId, 
                    workRequest.RequestedById, 
                    NotificationType.WorkRequestApproved,
                    "Your work request has been auto-approved and is ready to proceed.");
            }
            else
            {
                // Notify approver
                var approverId = workRequest.ManagerApproverId ?? workRequest.AdminApproverId;
                if (approverId.HasValue)
                {
                    await _notificationService.SendWorkRequestNotificationAsync(
                        workRequestId, 
                        approverId.Value, 
                        NotificationType.ApprovalRequired);
                }
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = isAutoApproved ? "Work request auto-approved" : "Work request submitted for approval"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error submitting work request for approval");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to submit for approval"
            };
        }
    }

    public async Task<ApiResponse<bool>> ProcessApprovalAsync(
        Guid workRequestId, 
        ApprovalRequest request, 
        Guid approverId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var approver = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == approverId);

            if (approver == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Approver not found"
                };
            }

            // Determine approval level and validate permissions
            var (canApprove, approvalLevel, approvalAction) = ValidateApprovalPermissions(workRequest, approver, request.Action);
            
            if (!canApprove)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "You don't have permission to approve this work request"
                };
            }

            var previousStatus = workRequest.Status;
            var newStatus = DetermineNewStatus(workRequest, request.Action, approvalLevel);

            // Handle escalation
            if (request.Action == "Escalate")
            {
                if (!request.EscalateToUserId.HasValue)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Escalation target user is required"
                    };
                }

                return await EscalateApprovalAsync(workRequestId, request.EscalateToUserId.Value, 
                    request.EscalationReason ?? "Escalated for review", approverId);
            }

            // Update work request
            workRequest.Status = newStatus;
            workRequest.UpdatedAt = DateTime.UtcNow;

            if (approvalLevel == ApprovalLevel.Manager)
            {
                workRequest.ManagerApproverId = approverId;
                workRequest.ManagerApprovalDate = DateTime.UtcNow;
                workRequest.ManagerComments = request.Comments;
            }
            else if (approvalLevel == ApprovalLevel.Admin)
            {
                workRequest.AdminApproverId = approverId;
                workRequest.AdminApprovalDate = DateTime.UtcNow;
                workRequest.AdminComments = request.Comments;
            }

            if (request.Action == "Reject")
            {
                workRequest.RejectionReason = request.RejectionReason;
            }

            // Create approval record
            var approval = new WorkRequestApproval
            {
                ApprovalId = Guid.NewGuid(),
                WorkRequestId = workRequestId,
                ApproverId = approverId,
                Action = approvalAction,
                Level = approvalLevel,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Comments = request.Comments,
                RejectionReason = request.RejectionReason,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.WorkRequestApprovals.Add(approval);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send notifications
            var notificationType = request.Action switch
            {
                "Approve" => NotificationType.WorkRequestApproved,
                "Reject" => NotificationType.WorkRequestRejected,
                _ => NotificationType.WorkRequestApproved
            };

            await _notificationService.SendWorkRequestNotificationAsync(
                workRequestId, 
                workRequest.RequestedById, 
                notificationType,
                request.Comments);

            // If approved but needs further approval, notify next approver
            if (request.Action == "Approve" && newStatus == WorkRequestStatus.PendingAdminApproval)
            {
                var adminApprovers = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Admin" && u.IsActive)
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var adminId in adminApprovers)
                {
                    await _notificationService.SendWorkRequestNotificationAsync(
                        workRequestId, 
                        adminId, 
                        NotificationType.ApprovalRequired);
                }
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Work request {request.Action.ToLower()}ed successfully"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing approval");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to process approval"
            };
        }
    }

    public async Task<ApiResponse<bool>> BulkApprovalAsync(BulkApprovalRequest request, Guid approverId)
    {
        var results = new List<bool>();
        
        foreach (var workRequestId in request.WorkRequestIds)
        {
            var approvalRequest = new ApprovalRequest
            {
                Action = request.Action,
                Comments = request.Comments,
                RejectionReason = request.RejectionReason
            };

            var result = await ProcessApprovalAsync(workRequestId, approvalRequest, approverId);
            results.Add(result.Success);
        }

        var successCount = results.Count(r => r);
        var failureCount = results.Count - successCount;

        return new ApiResponse<bool>
        {
            Success = failureCount == 0,
            Data = failureCount == 0,
            Message = $"Processed {successCount} approvals successfully, {failureCount} failed"
        };
    }

    public async Task<ApiResponse<ApprovalWorkflowStatusDto>> GetApprovalStatusAsync(Guid workRequestId)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.ManagerApprover)
                .Include(wr => wr.AdminApprover)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest == null)
            {
                return new ApiResponse<ApprovalWorkflowStatusDto>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var approvalHistory = await _context.WorkRequestApprovals
                .Include(a => a.Approver)
                .Where(a => a.WorkRequestId == workRequestId)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new WorkRequestApprovalDto
                {
                    ApprovalId = a.ApprovalId,
                    WorkRequestId = a.WorkRequestId,
                    ApproverId = a.ApproverId,
                    ApproverName = a.Approver.FullName,
                    Action = a.Action.ToString(),
                    Level = a.Level.ToString(),
                    PreviousStatus = a.PreviousStatus.ToString(),
                    NewStatus = a.NewStatus.ToString(),
                    Comments = a.Comments,
                    RejectionReason = a.RejectionReason,
                    CreatedAt = a.CreatedAt,
                    ProcessedAt = a.ProcessedAt,
                    IsActive = a.IsActive,
                    EscalationReason = a.EscalationReason,
                    EscalationDate = a.EscalationDate
                })
                .ToListAsync();

            var daysPending = workRequest.SubmittedForApprovalDate.HasValue
                ? (DateTime.UtcNow - workRequest.SubmittedForApprovalDate.Value).Days
                : 0;

            var status = new ApprovalWorkflowStatusDto
            {
                WorkRequestId = workRequestId,
                Title = workRequest.Title,
                CurrentStatus = workRequest.Status.ToString(),
                RequiresManagerApproval = workRequest.RequiresManagerApproval,
                RequiresAdminApproval = workRequest.RequiresAdminApproval,
                IsManagerApproved = workRequest.ManagerApprovalDate.HasValue,
                IsAdminApproved = workRequest.AdminApprovalDate.HasValue,
                CurrentApproverName = GetCurrentApproverName(workRequest),
                NextApproverName = GetNextApproverName(workRequest),
                ApprovalHistory = approvalHistory,
                SubmittedForApprovalDate = workRequest.SubmittedForApprovalDate,
                LastActionDate = approvalHistory.LastOrDefault()?.ProcessedAt,
                DaysPendingApproval = daysPending
            };

            return new ApiResponse<ApprovalWorkflowStatusDto>
            {
                Success = true,
                Data = status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval status");
            return new ApiResponse<ApprovalWorkflowStatusDto>
            {
                Success = false,
                Message = "Failed to get approval status"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<WorkRequestDto>>> GetPendingApprovalsAsync(
        Guid approverId, 
        int pageNumber = 1, 
        int pageSize = 20)
    {
        try
        {
            var approver = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == approverId);

            if (approver == null)
            {
                return new ApiResponse<PagedResult<WorkRequestDto>>
                {
                    Success = false,
                    Message = "Approver not found"
                };
            }

            var query = _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Include(wr => wr.AssignedTo)
                .Include(wr => wr.ManagerApprover)
                .Include(wr => wr.AdminApprover)
                .AsQueryable();

            // Filter based on user role and pending status
            if (approver.Role.RoleName == "Manager")
            {
                query = query.Where(wr => 
                    wr.Status == WorkRequestStatus.PendingManagerApproval &&
                    (wr.ManagerApproverId == approverId || wr.ManagerApproverId == null));
            }
            else if (approver.Role.RoleName == "Admin")
            {
                query = query.Where(wr => 
                    wr.Status == WorkRequestStatus.PendingAdminApproval ||
                    (wr.Status == WorkRequestStatus.PendingManagerApproval && wr.ManagerApproverId == null));
            }
            else
            {
                return new ApiResponse<PagedResult<WorkRequestDto>>
                {
                    Success = false,
                    Message = "User does not have approval permissions"
                };
            }

            var totalCount = await query.CountAsync();
            var workRequests = await query
                .OrderByDescending(wr => wr.SubmittedForApprovalDate ?? wr.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var workRequestDtos = workRequests.Select(wr => MapToWorkRequestDto(wr)).ToList();

            var result = new PagedResult<WorkRequestDto>
            {
                Items = workRequestDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return new ApiResponse<PagedResult<WorkRequestDto>>
            {
                Success = false,
                Message = "Failed to get pending approvals"
            };
        }
    }

    public async Task<ApiResponse<ApprovalStatisticsDto>> GetApprovalStatisticsAsync(Guid? userId = null)
    {
        try
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var query = _context.WorkRequests.AsQueryable();

            // Filter by user if provided
            if (userId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId.Value);

                if (user?.Role.RoleName == "Manager")
                {
                    query = query.Where(wr => wr.ManagerApproverId == userId.Value || 
                                            (wr.Status == WorkRequestStatus.PendingManagerApproval && wr.ManagerApproverId == null));
                }
                else if (user?.Role.RoleName == "Admin")
                {
                    query = query.Where(wr => wr.AdminApproverId == userId.Value ||
                                            wr.Status == WorkRequestStatus.PendingAdminApproval);
                }
            }

            var totalPending = await query
                .CountAsync(wr => wr.Status == WorkRequestStatus.PendingManagerApproval || 
                                 wr.Status == WorkRequestStatus.PendingAdminApproval);

            var managerPending = await query
                .CountAsync(wr => wr.Status == WorkRequestStatus.PendingManagerApproval);

            var adminPending = await query
                .CountAsync(wr => wr.Status == WorkRequestStatus.PendingAdminApproval);

            var overdueThreshold = now.AddDays(-7); // 7 days old
            var overdue = await query
                .CountAsync(wr => (wr.Status == WorkRequestStatus.PendingManagerApproval || 
                                  wr.Status == WorkRequestStatus.PendingAdminApproval) &&
                                 wr.SubmittedForApprovalDate < overdueThreshold);

            var approvalsToday = await _context.WorkRequestApprovals
                .CountAsync(a => a.ProcessedAt >= todayStart && 
                               (a.Action == ApprovalAction.ManagerApproved || a.Action == ApprovalAction.AdminApproved));

            var approvalsThisWeek = await _context.WorkRequestApprovals
                .CountAsync(a => a.ProcessedAt >= weekStart && 
                               (a.Action == ApprovalAction.ManagerApproved || a.Action == ApprovalAction.AdminApproved));

            var approvalsThisMonth = await _context.WorkRequestApprovals
                .CountAsync(a => a.ProcessedAt >= monthStart && 
                               (a.Action == ApprovalAction.ManagerApproved || a.Action == ApprovalAction.AdminApproved));

            // Calculate average approval time
            var approvalTimes = await _context.WorkRequestApprovals
                .Where(a => a.ProcessedAt.HasValue && 
                           (a.Action == ApprovalAction.ManagerApproved || a.Action == ApprovalAction.AdminApproved))
                .Join(_context.WorkRequests, a => a.WorkRequestId, wr => wr.WorkRequestId, 
                     (a, wr) => new { a.ProcessedAt, wr.SubmittedForApprovalDate })
                .Where(x => x.SubmittedForApprovalDate.HasValue)
                .Select(x => (x.ProcessedAt!.Value - x.SubmittedForApprovalDate!.Value).TotalHours)
                .ToListAsync();

            var avgApprovalTime = approvalTimes.Any() ? approvalTimes.Average() : 0;

            // Get urgent and overdue items
            var urgentPending = await query
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Where(wr => (wr.Status == WorkRequestStatus.PendingManagerApproval || 
                             wr.Status == WorkRequestStatus.PendingAdminApproval) &&
                            wr.Priority == WorkRequestPriority.Critical)
                .Take(5)
                .Select(wr => MapToWorkRequestDto(wr))
                .ToListAsync();

            var overdueItems = await query
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .Where(wr => (wr.Status == WorkRequestStatus.PendingManagerApproval || 
                             wr.Status == WorkRequestStatus.PendingAdminApproval) &&
                            wr.SubmittedForApprovalDate < overdueThreshold)
                .Take(5)
                .Select(wr => MapToWorkRequestDto(wr))
                .ToListAsync();

            var statistics = new ApprovalStatisticsDto
            {
                TotalPendingApprovals = totalPending,
                ManagerPendingApprovals = managerPending,
                AdminPendingApprovals = adminPending,
                OverdueApprovalsCount = overdue,
                ApprovalsTodayCount = approvalsToday,
                ApprovalsThisWeekCount = approvalsThisWeek,
                ApprovalsThisMonthCount = approvalsThisMonth,
                AverageApprovalTimeHours = avgApprovalTime,
                UrgentPendingApprovals = urgentPending,
                OverdueApprovalsList = overdueItems
            };

            return new ApiResponse<ApprovalStatisticsDto>
            {
                Success = true,
                Data = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval statistics");
            return new ApiResponse<ApprovalStatisticsDto>
            {
                Success = false,
                Message = "Failed to get approval statistics"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<WorkRequestApprovalDto>>> GetApprovalHistoryAsync(
        Guid workRequestId, 
        int pageNumber = 1, 
        int pageSize = 20)
    {
        try
        {
            var totalCount = await _context.WorkRequestApprovals
                .CountAsync(a => a.WorkRequestId == workRequestId);

            var approvals = await _context.WorkRequestApprovals
                .Include(a => a.Approver)
                .Include(a => a.WorkRequest)
                .Where(a => a.WorkRequestId == workRequestId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new WorkRequestApprovalDto
                {
                    ApprovalId = a.ApprovalId,
                    WorkRequestId = a.WorkRequestId,
                    WorkRequestTitle = a.WorkRequest.Title,
                    ApproverId = a.ApproverId,
                    ApproverName = a.Approver.FullName,
                    Action = a.Action.ToString(),
                    Level = a.Level.ToString(),
                    PreviousStatus = a.PreviousStatus.ToString(),
                    NewStatus = a.NewStatus.ToString(),
                    Comments = a.Comments,
                    RejectionReason = a.RejectionReason,
                    CreatedAt = a.CreatedAt,
                    ProcessedAt = a.ProcessedAt,
                    IsActive = a.IsActive,
                    EscalationReason = a.EscalationReason,
                    EscalationDate = a.EscalationDate
                })
                .ToListAsync();

            var result = new PagedResult<WorkRequestApprovalDto>
            {
                Items = approvals,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<WorkRequestApprovalDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval history");
            return new ApiResponse<PagedResult<WorkRequestApprovalDto>>
            {
                Success = false,
                Message = "Failed to get approval history"
            };
        }
    }

    public async Task<ApiResponse<bool>> EscalateApprovalAsync(
        Guid workRequestId, 
        Guid escalateToUserId, 
        string reason, 
        Guid escalatedById)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var workRequest = await _context.WorkRequests
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var escalateToUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == escalateToUserId);

            if (escalateToUser == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Escalation target user not found"
                };
            }

            // Create escalation approval record
            var escalation = new WorkRequestApproval
            {
                ApprovalId = Guid.NewGuid(),
                WorkRequestId = workRequestId,
                ApproverId = escalateToUserId,
                Action = ApprovalAction.Escalated,
                Level = escalateToUser.Role.RoleName == "Admin" ? ApprovalLevel.Admin : ApprovalLevel.Manager,
                PreviousStatus = workRequest.Status,
                NewStatus = escalateToUser.Role.RoleName == "Admin" ? 
                    WorkRequestStatus.PendingAdminApproval : WorkRequestStatus.PendingManagerApproval,
                Comments = reason,
                EscalationReason = reason,
                EscalationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Update work request
            workRequest.Status = escalation.NewStatus;
            if (escalateToUser.Role.RoleName == "Admin")
            {
                workRequest.AdminApproverId = escalateToUserId;
            }
            else
            {
                workRequest.ManagerApproverId = escalateToUserId;
            }

            _context.WorkRequestApprovals.Add(escalation);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send notification
            await _notificationService.SendWorkRequestNotificationAsync(
                workRequestId,
                escalateToUserId,
                NotificationType.WorkRequestEscalated,
                $"Work request has been escalated to you. Reason: {reason}");

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Work request escalated successfully"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error escalating approval");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to escalate approval"
            };
        }
    }

    public async Task<ApiResponse<bool>> SendApprovalRemindersAsync()
    {
        try
        {
            var reminderThreshold = DateTime.UtcNow.AddDays(-3); // 3 days old
            
            var pendingApprovals = await _context.WorkRequests
                .Include(wr => wr.ManagerApprover)
                .Include(wr => wr.AdminApprover)
                .Where(wr => (wr.Status == WorkRequestStatus.PendingManagerApproval || 
                             wr.Status == WorkRequestStatus.PendingAdminApproval) &&
                            wr.SubmittedForApprovalDate < reminderThreshold)
                .ToListAsync();

            var reminderCount = 0;
            foreach (var workRequest in pendingApprovals)
            {
                var approverId = workRequest.Status == WorkRequestStatus.PendingManagerApproval
                    ? workRequest.ManagerApproverId
                    : workRequest.AdminApproverId;

                if (approverId.HasValue)
                {
                    await _notificationService.SendWorkRequestNotificationAsync(
                        workRequest.WorkRequestId,
                        approverId.Value,
                        NotificationType.ApprovalReminder,
                        $"Reminder: This work request has been pending approval for {(DateTime.UtcNow - workRequest.SubmittedForApprovalDate!.Value).Days} days.");
                    
                    reminderCount++;
                }
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Sent {reminderCount} approval reminders"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending approval reminders");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to send approval reminders"
            };
        }
    }

    #region Private Helper Methods

    private bool ShouldRequireManagerApproval(WorkRequest workRequest)
    {
        // Business logic for determining if manager approval is required
        var managerApprovalThreshold = decimal.Parse(_configuration["WorkRequest:ManagerApprovalThreshold"] ?? "500");
        
        return workRequest.EstimatedCost.HasValue && workRequest.EstimatedCost.Value > managerApprovalThreshold ||
               workRequest.Priority == WorkRequestPriority.High ||
               workRequest.Priority == WorkRequestPriority.Critical;
    }

    private bool ShouldRequireAdminApproval(WorkRequest workRequest)
    {
        // Business logic for determining if admin approval is required
        var adminApprovalThreshold = decimal.Parse(_configuration["WorkRequest:AdminApprovalThreshold"] ?? "5000");
        
        return workRequest.EstimatedCost.HasValue && workRequest.EstimatedCost.Value > adminApprovalThreshold ||
               workRequest.Priority == WorkRequestPriority.Critical;
    }

    private (bool canApprove, ApprovalLevel level, ApprovalAction action) ValidateApprovalPermissions(
        WorkRequest workRequest, User approver, string actionString)
    {
        var action = actionString switch
        {
            "Approve" when approver.Role.RoleName == "Manager" => ApprovalAction.ManagerApproved,
            "Approve" when approver.Role.RoleName == "Admin" => ApprovalAction.AdminApproved,
            "Reject" when approver.Role.RoleName == "Manager" => ApprovalAction.ManagerRejected,
            "Reject" when approver.Role.RoleName == "Admin" => ApprovalAction.AdminRejected,
            "Escalate" => ApprovalAction.Escalated,
            _ => ApprovalAction.Submitted
        };

        var level = approver.Role.RoleName switch
        {
            "Manager" => ApprovalLevel.Manager,
            "Admin" => ApprovalLevel.Admin,
            _ => ApprovalLevel.None
        };

        var canApprove = level switch
        {
            ApprovalLevel.Manager => workRequest.Status == WorkRequestStatus.PendingManagerApproval &&
                                   (workRequest.ManagerApproverId == approver.UserId || workRequest.ManagerApproverId == null),
            ApprovalLevel.Admin => workRequest.Status == WorkRequestStatus.PendingAdminApproval ||
                                 (workRequest.Status == WorkRequestStatus.PendingManagerApproval && workRequest.ManagerApproverId == null),
            _ => false
        };

        return (canApprove, level, action);
    }

    private WorkRequestStatus DetermineNewStatus(WorkRequest workRequest, string action, ApprovalLevel level)
    {
        if (action == "Reject")
        {
            return WorkRequestStatus.Rejected;
        }

        if (action == "Approve")
        {
            if (level == ApprovalLevel.Manager)
            {
                return workRequest.RequiresAdminApproval 
                    ? WorkRequestStatus.PendingAdminApproval 
                    : WorkRequestStatus.Approved;
            }
            else if (level == ApprovalLevel.Admin)
            {
                return WorkRequestStatus.Approved;
            }
        }

        return workRequest.Status; // No change
    }

    private string? GetCurrentApproverName(WorkRequest workRequest)
    {
        return workRequest.Status switch
        {
            WorkRequestStatus.PendingManagerApproval => workRequest.ManagerApprover?.FullName,
            WorkRequestStatus.PendingAdminApproval => workRequest.AdminApprover?.FullName,
            _ => null
        };
    }

    private string? GetNextApproverName(WorkRequest workRequest)
    {
        if (workRequest.Status == WorkRequestStatus.PendingManagerApproval && workRequest.RequiresAdminApproval)
        {
            return workRequest.AdminApprover?.FullName ?? "Admin (TBD)";
        }
        return null;
    }

    private WorkRequestDto MapToWorkRequestDto(WorkRequest workRequest)
    {
        var daysPending = workRequest.SubmittedForApprovalDate.HasValue
            ? (DateTime.UtcNow - workRequest.SubmittedForApprovalDate.Value).Days
            : 0;

        return new WorkRequestDto
        {
            WorkRequestId = workRequest.WorkRequestId,
            ProjectId = workRequest.ProjectId,
            ProjectName = workRequest.Project?.ProjectName ?? "",
            Title = workRequest.Title,
            Description = workRequest.Description,
            Type = workRequest.Type.ToString(),
            Priority = workRequest.Priority.ToString(),
            Status = workRequest.Status.ToString(),
            RequestedById = workRequest.RequestedById,
            RequestedByName = workRequest.RequestedBy?.FullName,
            AssignedToId = workRequest.AssignedToId,
            AssignedToName = workRequest.AssignedTo?.FullName,
            RequestedDate = workRequest.RequestedDate,
            DueDate = workRequest.DueDate,
            StartedAt = workRequest.StartedAt,
            CompletedDate = workRequest.CompletedDate,
            Resolution = workRequest.Resolution,
            EstimatedCost = workRequest.EstimatedCost,
            ActualCost = workRequest.ActualCost,
            EstimatedHours = workRequest.EstimatedHours,
            ActualHours = workRequest.ActualHours,
            Location = workRequest.Location,
            Notes = workRequest.Notes,
            CreatedAt = workRequest.CreatedAt,
            UpdatedAt = workRequest.UpdatedAt,
            
            // Approval fields
            ManagerApproverId = workRequest.ManagerApproverId,
            ManagerApproverName = workRequest.ManagerApprover?.FullName,
            AdminApproverId = workRequest.AdminApproverId,
            AdminApproverName = workRequest.AdminApprover?.FullName,
            ManagerApprovalDate = workRequest.ManagerApprovalDate,
            AdminApprovalDate = workRequest.AdminApprovalDate,
            SubmittedForApprovalDate = workRequest.SubmittedForApprovalDate,
            ManagerComments = workRequest.ManagerComments,
            AdminComments = workRequest.AdminComments,
            RejectionReason = workRequest.RejectionReason,
            RequiresManagerApproval = workRequest.RequiresManagerApproval,
            RequiresAdminApproval = workRequest.RequiresAdminApproval,
            IsAutoApproved = workRequest.IsAutoApproved,
            CurrentApproverName = GetCurrentApproverName(workRequest),
            NextApproverName = GetNextApproverName(workRequest),
            DaysPendingApproval = daysPending,
            
            Tasks = new List<WorkRequestTaskDto>(),
            Comments = new List<WorkRequestCommentDto>(),
            ApprovalHistory = new List<WorkRequestApprovalDto>(),
            Images = new List<ImageMetadataDto>(),
            ImageCount = 0,
            Links = new List<LinkDto>()
        };
    }

    #endregion
}
