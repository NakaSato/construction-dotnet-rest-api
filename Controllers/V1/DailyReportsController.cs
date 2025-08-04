using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Attributes;
using Asp.Versioning;
using IDailyReportService = dotnet_rest_api.Services.Infrastructure.IDailyReportService;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Enhanced API controller for managing daily reports with comprehensive project-centric functionality
/// Provides CRUD operations, analytics, reporting, and workflow management for solar project daily reports
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/daily-reports")]
[Authorize]
[Produces("application/json")]
public class DailyReportsController : BaseApiController
{
    private readonly IDailyReportService _dailyReportService;
    private readonly ILogger<DailyReportsController> _logger;

    public DailyReportsController(
        IDailyReportService dailyReportService,
        ILogger<DailyReportsController> logger)
    {
        _dailyReportService = dailyReportService;
        _logger = logger;
    }

    /// <summary>
    /// Get all daily reports with filtering
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<DailyReportDto>>>> GetDailyReports([FromQuery] DailyReportQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetDailyReports", parameters);

            // Apply dynamic filters from query string
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _dailyReportService.GetDailyReportsAsync(parameters);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<DailyReportDto>>(_logger, ex, "retrieving daily reports");
        }
    }

    /// <summary>
    /// Get daily report by ID
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet("{id:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> GetDailyReport(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetDailyReport", new { id });

            var result = await _dailyReportService.GetDailyReportByIdAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, "retrieving daily report");
        }
    }

    /// <summary>
    /// Create a new daily report
    /// Available to: All authenticated users
    /// </summary>
    [HttpPost]
    [NoCache]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> CreateDailyReport([FromBody] CreateDailyReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateDailyReport", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<DailyReportDto>("Invalid input data", 400);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportDto>("Invalid user ID in token", 401);

            var result = await _dailyReportService.CreateDailyReportAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, "creating daily report");
        }
    }

    /// <summary>
    /// Update daily report
    /// Available to: Admin, Manager, or report creator (within 24h)
    /// </summary>
    [HttpPut("{id:guid}")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> UpdateDailyReport(Guid id, [FromBody] UpdateDailyReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdateDailyReport", new { id, request });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<DailyReportDto>("Invalid input data", 400);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportDto>("Invalid user ID in token", 401);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _dailyReportService.UpdateDailyReportAsync(id, request, userId, userRole ?? "");
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, "updating daily report");
        }
    }

    /// <summary>
    /// Delete daily report
    /// Available to: Administrator, Manager only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator,Manager")]
    [DeleteRateLimit]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDailyReport(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "DeleteDailyReport", new { id });

            var result = await _dailyReportService.DeleteDailyReportAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "deleting daily report");
        }
    }

    /// <summary>
    /// Add attachment to daily report
    /// Available to: Admin, Manager, or report creator
    /// </summary>
    [HttpPost("{id:guid}/attachments")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<DailyReportAttachmentDto>>> AddAttachment(Guid id, IFormFile file)
    {
        try
        {
            LogControllerAction(_logger, "AddAttachment", new { id, fileName = file.FileName });

            if (file == null || file.Length == 0)
                return CreateErrorResponse<DailyReportAttachmentDto>("No file provided", 400);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportAttachmentDto>("Invalid user ID in token", 401);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _dailyReportService.AddAttachmentAsync(id, file, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportAttachmentDto>(_logger, ex, "adding attachment");
        }
    }

    /// <summary>
    /// Get weekly summary report
    /// Available to: Administrator, Manager
    /// </summary>
    [HttpGet("weekly-summary")]
    [Authorize(Roles = "Administrator,Manager")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<WeeklySummaryDto>>> GetWeeklySummary(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? weekStartDate = null)
    {
        try
        {
            LogControllerAction(_logger, "GetWeeklySummary", new { projectId, weekStartDate });

            var result = await _dailyReportService.GetWeeklySummaryAsync(projectId ?? Guid.Empty, weekStartDate ?? DateTime.UtcNow.Date);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WeeklySummaryDto>(_logger, ex, "retrieving weekly summary");
        }
    }

    /// <summary>
    /// Export daily reports
    /// Available to: Administrator, Manager
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Administrator,Manager")]
    [NoCache]
    public async Task<IActionResult> ExportDailyReports(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string format = "csv")
    {
        try
        {
            LogControllerAction(_logger, "ExportDailyReports", new { projectId, startDate, endDate, format });

            var parameters = new DailyReportQueryParameters();
            if (projectId.HasValue)
                parameters.ProjectId = projectId.Value;
            
            var result = await _dailyReportService.ExportDailyReportsAsync(parameters);
            
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            var contentType = format.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                _ => "text/csv"
            };

            var fileName = $"daily-reports-{DateTime.UtcNow:yyyyMMdd}.{format}";
            return File(result.Data!, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting daily reports");
            return StatusCode(500, "Error exporting daily reports");
        }
    }

    /// <summary>
    /// Submit a daily report for approval
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> SubmitDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "SubmitDailyReport", new { id });

            var result = await _dailyReportService.SubmitDailyReportAsync(id);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, $"submitting daily report {id}");
        }
    }

    /// <summary>
    /// Approve a daily report
    /// Available to: Administrator, ProjectManager (approval authority)
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> ApproveDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "ApproveDailyReport", new { id });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportDto>("Invalid user ID in token", 401);

            var result = await _dailyReportService.ApproveDailyReportAsync(id, userId);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, $"approving daily report {id}");
        }
    }

    /// <summary>
    /// Reject a daily report and request revisions
    /// Available to: Administrator, ProjectManager (approval authority)
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <param name="rejectionReason">Reason for rejection</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> RejectDailyReport(Guid id, [FromBody] string? rejectionReason)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "RejectDailyReport", new { id, rejectionReason });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportDto>("Invalid user ID in token", 401);

            var result = await _dailyReportService.RejectDailyReportAsync(id, userId, rejectionReason ?? "No reason provided");

            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportDto>(_logger, ex, $"rejecting daily report {id}");
        }
    }

    #region Work Progress Items

    /// <summary>
    /// Add a work progress item to a daily report
    /// </summary>
    /// <param name="reportId">The daily report ID</param>
    /// <param name="request">Work progress item creation request</param>
    /// <returns>Created work progress item</returns>
    [HttpPost("{reportId:guid}/work-progress")]
    [ProducesResponseType(typeof(ApiResponse<WorkProgressItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkProgressItemDto>>> AddWorkProgressItem(Guid reportId, [FromBody] CreateWorkProgressItemRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "AddWorkProgressItem", new { reportId, request });

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<WorkProgressItemDto> { Success = false, Message = "Invalid request data" });
            }

            var result = await _dailyReportService.AddWorkProgressItemAsync(reportId, request);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkProgressItemDto>(_logger, ex, $"adding work progress item to daily report {reportId}");
        }
    }

    /// <summary>
    /// Update a work progress item
    /// </summary>
    /// <param name="reportId">The daily report ID</param>
    /// <param name="itemId">The work progress item ID</param>
    /// <param name="request">Work progress item update request</param>
    /// <returns>Updated work progress item</returns>
    [HttpPut("{reportId:guid}/work-progress/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkProgressItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkProgressItemDto>>> UpdateWorkProgressItem(Guid reportId, Guid itemId, [FromBody] UpdateWorkProgressItemRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateWorkProgressItem", new { reportId, itemId, request });

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<WorkProgressItemDto> { Success = false, Message = "Invalid request data" });
            }

            var result = await _dailyReportService.UpdateWorkProgressItemAsync(itemId, request);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkProgressItemDto>(_logger, ex, $"updating work progress item {itemId}");
        }
    }

    /// <summary>
    /// Delete a work progress item
    /// </summary>
    /// <param name="reportId">The daily report ID</param>
    /// <param name="itemId">The work progress item ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{reportId:guid}/work-progress/{itemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkProgressItem(Guid reportId, Guid itemId)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteWorkProgressItem", new { reportId, itemId });

            var result = await _dailyReportService.DeleteWorkProgressItemAsync(itemId);


            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"deleting work progress item {itemId}");
        }
    }

    #endregion

    #region Enhanced Daily Report Operations

    /// <summary>
    /// Get daily reports for a specific project with enhanced filtering and analytics
    /// Available to: All authenticated users (filtered by project access)
    /// </summary>
    /// <param name="projectId">Project ID to filter reports</param>
    /// <param name="parameters">Enhanced query parameters</param>
    /// <returns>Paginated list of enhanced daily reports</returns>
    [HttpGet("projects/{projectId:guid}")]
    [MediumCache] // 15 minute cache
    [ProducesResponseType(typeof(ApiResponse<EnhancedPagedResult<EnhancedDailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<EnhancedDailyReportDto>>>> GetProjectDailyReports(
        Guid projectId, 
        [FromQuery] EnhancedDailyReportQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetProjectDailyReports", new { projectId, parameters });

            // Ensure project ID consistency
            parameters.ProjectId = projectId;

            // Apply dynamic filters from query string
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _dailyReportService.GetProjectDailyReportsAsync(projectId, parameters);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<EnhancedDailyReportDto>>(_logger, ex, "retrieving project daily reports");
        }
    }

    /// <summary>
    /// Create enhanced daily report with comprehensive validation and project context
    /// Available to: All authenticated users (within their assigned projects)
    /// </summary>
    /// <param name="request">Enhanced daily report creation request</param>
    /// <returns>Created daily report with full context</returns>
    [HttpPost("enhanced")]
    [NoCache]
    [ProducesResponseType(typeof(ApiResponse<EnhancedDailyReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<EnhancedDailyReportDto>>> CreateEnhancedDailyReport(
        [FromBody] EnhancedCreateDailyReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateEnhancedDailyReport", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<EnhancedDailyReportDto>("Invalid input data", 400, errors);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<EnhancedDailyReportDto>("Invalid user ID in token", 401);

            // Validate project access
            var hasProjectAccess = await _dailyReportService.ValidateProjectAccessAsync(request.ProjectId, userId);
            if (!hasProjectAccess.IsSuccess)
                return CreateErrorResponse<EnhancedDailyReportDto>("Access denied to project", 403);

            var result = await _dailyReportService.CreateEnhancedDailyReportAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedDailyReportDto>(_logger, ex, "creating enhanced daily report");
        }
    }

    /// <summary>
    /// Get comprehensive daily report analytics for a project
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="startDate">Analysis start date</param>
    /// <param name="endDate">Analysis end date</param>
    /// <returns>Comprehensive analytics data</returns>
    [HttpGet("projects/{projectId:guid}/analytics")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [LongCache] // 1 hour cache for analytics
    [ProducesResponseType(typeof(ApiResponse<DailyReportAnalyticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DailyReportAnalyticsDto>>> GetDailyReportAnalytics(
        Guid projectId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            LogControllerAction(_logger, "GetDailyReportAnalytics", new { projectId, startDate, endDate });

            // Default to last 30 days if no dates provided
            var analysisStart = startDate ?? DateTime.UtcNow.AddDays(-30);
            var analysisEnd = endDate ?? DateTime.UtcNow;

            if (analysisStart >= analysisEnd)
                return CreateErrorResponse<DailyReportAnalyticsDto>("Start date must be before end date", 400);

            var result = await _dailyReportService.GetDailyReportAnalyticsAsync(projectId, analysisStart, analysisEnd);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportAnalyticsDto>(_logger, ex, "retrieving daily report analytics");
        }
    }

    /// <summary>
    /// Generate weekly progress report for a project
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="weekStartDate">Week start date (defaults to current week)</param>
    /// <returns>Weekly progress report</returns>
    [HttpGet("projects/{projectId:guid}/weekly-report")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [ShortCache] // 5 minute cache for weekly reports
    [ProducesResponseType(typeof(ApiResponse<WeeklySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<WeeklySummaryDto>>> GetWeeklyProgressReport(
        Guid projectId,
        [FromQuery] DateTime? weekStartDate = null)
    {
        try
        {
            LogControllerAction(_logger, "GetWeeklyProgressReport", new { projectId, weekStartDate });

            // Default to current week start (Monday)
            var weekStart = weekStartDate ?? GetCurrentWeekStart();

            var result = await _dailyReportService.GetWeeklyProgressReportAsync(projectId, weekStart);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WeeklySummaryDto>(_logger, ex, "retrieving weekly progress report");
        }
    }

    /// <summary>
    /// Bulk approve multiple daily reports
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="request">Bulk approval request</param>
    /// <returns>Approval results</returns>
    [HttpPost("bulk-approve")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [NoCache]
    [ProducesResponseType(typeof(ApiResponse<BulkOperationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkApproveDailyReports(
        [FromBody] DailyReportBulkApprovalRequest request)
    {
        try
        {
            LogControllerAction(_logger, "BulkApproveDailyReports", request);

            if (!ModelState.IsValid || !request.ReportIds.Any())
                return CreateErrorResponse<BulkOperationResultDto>("Invalid request data", 400);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<BulkOperationResultDto>("Invalid user ID in token", 401);

            var result = await _dailyReportService.BulkApproveDailyReportsAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<BulkOperationResultDto>(_logger, ex, "bulk approving daily reports");
        }
    }

    /// <summary>
    /// Bulk reject multiple daily reports
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="request">Bulk rejection request</param>
    /// <returns>Rejection results</returns>
    [HttpPost("bulk-reject")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [NoCache]
    [ProducesResponseType(typeof(ApiResponse<BulkOperationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<BulkOperationResultDto>>> BulkRejectDailyReports(
        [FromBody] DailyReportBulkRejectionRequest request)
    {
        try
        {
            LogControllerAction(_logger, "BulkRejectDailyReports", request);

            if (!ModelState.IsValid || !request.ReportIds.Any())
                return CreateErrorResponse<BulkOperationResultDto>("Invalid request data", 400);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<BulkOperationResultDto>("Invalid user ID in token", 401);

            var result = await _dailyReportService.BulkRejectDailyReportsAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<BulkOperationResultDto>(_logger, ex, "bulk rejecting daily reports");
        }
    }

    /// <summary>
    /// Enhanced export with multiple formats and comprehensive data
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="request">Export request parameters</param>
    /// <returns>File download</returns>
    [HttpPost("export-enhanced")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [NoCache]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportEnhancedDailyReports([FromBody] DailyReportExportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "ExportEnhancedDailyReports", request);

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request data" });

            var result = await _dailyReportService.ExportEnhancedDailyReportsAsync(request);
            
            if (!result.IsSuccess)
                return BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });

            var contentType = request.Format.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "pdf" => "application/pdf",
                "json" => "application/json",
                _ => "text/csv"
            };

            var fileName = $"daily-reports-{request.ProjectId}-{DateTime.UtcNow:yyyyMMdd}.{request.Format}";
            return File(result.Data!, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting enhanced daily reports");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error exporting daily reports" });
        }
    }

    /// <summary>
    /// Get daily report insights and recommendations
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="reportId">Specific report ID (optional)</param>
    /// <returns>AI-generated insights and recommendations</returns>
    [HttpGet("projects/{projectId:guid}/insights")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [MediumCache] // 15 minute cache for insights
    [ProducesResponseType(typeof(ApiResponse<DailyReportInsightsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<DailyReportInsightsDto>>> GetDailyReportInsights(
        Guid projectId,
        [FromQuery] Guid? reportId = null)
    {
        try
        {
            LogControllerAction(_logger, "GetDailyReportInsights", new { projectId, reportId });

            var result = await _dailyReportService.GetDailyReportInsightsAsync(projectId, reportId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportInsightsDto>(_logger, ex, "retrieving daily report insights");
        }
    }

    /// <summary>
    /// Validate daily report data before submission
    /// Available to: All authenticated users
    /// </summary>
    /// <param name="request">Daily report data to validate</param>
    /// <returns>Validation results and suggestions</returns>
    [HttpPost("validate")]
    [ShortCache] // 5 minute cache for validation
    [ProducesResponseType(typeof(ApiResponse<DailyReportValidationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DailyReportValidationResultDto>>> ValidateDailyReport(
        [FromBody] EnhancedCreateDailyReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "ValidateDailyReport", request);

            var result = await _dailyReportService.ValidateDailyReportAsync(request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<DailyReportValidationResultDto>(_logger, ex, "validating daily report");
        }
    }

    #endregion

    #region Project Context and Workflow Management

    /// <summary>
    /// Get daily report templates for a specific project
    /// Available to: All authenticated users
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Project-specific daily report templates</returns>
    [HttpGet("projects/{projectId:guid}/templates")]
    [LongCache] // 1 hour cache for templates
    [ProducesResponseType(typeof(ApiResponse<List<DailyReportTemplateDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<DailyReportTemplateDto>>>> GetDailyReportTemplates(Guid projectId)
    {
        try
        {
            LogControllerAction(_logger, "GetDailyReportTemplates", new { projectId });

            var result = await _dailyReportService.GetDailyReportTemplatesAsync(projectId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<List<DailyReportTemplateDto>>(_logger, ex, "retrieving daily report templates");
        }
    }

    /// <summary>
    /// Get pending daily reports requiring approval
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="projectId">Project ID (optional)</param>
    /// <returns>List of reports pending approval</returns>
    [HttpGet("pending-approval")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [ShortCache] // 5 minute cache for pending approvals
    [ProducesResponseType(typeof(ApiResponse<EnhancedPagedResult<DailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<DailyReportDto>>>> GetPendingApprovals(
        [FromQuery] Guid? projectId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            LogControllerAction(_logger, "GetPendingApprovals", new { projectId, pageNumber, pageSize });

            var result = await _dailyReportService.GetPendingApprovalsAsync(projectId, pageNumber, pageSize);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<DailyReportDto>>(_logger, ex, "retrieving pending approvals");
        }
    }

    /// <summary>
    /// Get daily report approval history
    /// Available to: Administrator, Manager, ProjectManager
    /// </summary>
    /// <param name="reportId">Daily report ID</param>
    /// <returns>Complete approval history</returns>
    [HttpGet("{reportId:guid}/approval-history")]
    [Authorize(Roles = "Administrator,Manager,ProjectManager")]
    [MediumCache] // 15 minute cache for approval history
    [ProducesResponseType(typeof(ApiResponse<List<ApprovalHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<ApprovalHistoryDto>>>> GetApprovalHistory(Guid reportId)
    {
        try
        {
            LogControllerAction(_logger, "GetApprovalHistory", new { reportId });

            var result = await _dailyReportService.GetApprovalHistoryAsync(reportId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<List<ApprovalHistoryDto>>(_logger, ex, "retrieving approval history");
        }
    }

    #endregion

    #region Helper Methods

    private DateTime GetCurrentWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = (int)today.DayOfWeek;
        var daysUntilMonday = (dayOfWeek == 0) ? 6 : dayOfWeek - 1; // Sunday = 0, Monday = 1
        return today.AddDays(-daysUntilMonday);
    }

    private void ApplyFiltersFromQuery<T>(T parameters, string? filterString) where T : BaseQueryParameters
    {
        if (string.IsNullOrEmpty(filterString))
            return;

        try
        {
            // Parse and apply dynamic filters
            // This is a simplified implementation - in production, you'd want more robust filter parsing
            var filters = filterString.Split(';');
            foreach (var filter in filters)
            {
                var parts = filter.Split(':');
                if (parts.Length == 2)
                {
                    var property = parts[0].Trim();
                    var value = parts[1].Trim();
                    
                    // Apply filter based on property name
                    ApplyFilterToProperty(parameters, property, value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying dynamic filters: {FilterString}", filterString);
        }
    }

    private void ApplyFilterToProperty<T>(T parameters, string propertyName, string value)
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            try
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(parameters, convertedValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error applying filter {PropertyName}={Value}", propertyName, value);
            }
        }
    }

    protected ApiResponse<T> CreateErrorResponse<T>(string message, int statusCode, List<string>? errors = null)
    {
        Response.StatusCode = statusCode;
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    #endregion
}
