using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing daily reports
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/daily-reports")]
[Authorize]
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
                return CreateErrorResponse<DailyReportDto>("Invalid input data", 400, errors);
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
                return CreateErrorResponse<DailyReportDto>("Invalid input data", 400, errors);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<DailyReportDto>("Invalid user ID in token", 401);

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var result = await _dailyReportService.UpdateDailyReportAsync(id, request, userId, userRole);
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
            var result = await _dailyReportService.AddAttachmentAsync(id, file, userId, userRole);
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

            var result = await _dailyReportService.GetWeeklySummaryAsync(projectId, weekStartDate);
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

            var result = await _dailyReportService.ExportDailyReportsAsync(projectId, startDate, endDate, format);
            
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
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "SubmitDailyReport", new { id });

            var result = await _dailyReportService.SubmitDailyReportAsync(id);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Daily report submitted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"submitting daily report {id}");
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
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "ApproveDailyReport", new { id });

            var result = await _dailyReportService.ApproveDailyReportAsync(id);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Daily report approved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"approving daily report {id}");
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
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> RejectDailyReport(Guid id, [FromBody] string? rejectionReason)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "RejectDailyReport", new { id, rejectionReason });

            var result = await _dailyReportService.RejectDailyReportAsync(id, rejectionReason);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Daily report rejected successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"rejecting daily report {id}");
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
                return CreateErrorResponse("Invalid request data", 400);
            }

            var result = await _dailyReportService.AddWorkProgressItemAsync(reportId, request);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data!, "Work progress item added successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"adding work progress item to daily report {reportId}");
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
    public async Task<ActionResult<ApiResponse<WorkProgressItemDto>>> UpdateWorkProgressItem(Guid reportId, Guid itemId, [FromBody] CreateWorkProgressItemRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateWorkProgressItem", new { reportId, itemId, request });

            if (!ModelState.IsValid)
            {
                return CreateErrorResponse("Invalid request data", 400);
            }

            var result = await _dailyReportService.UpdateWorkProgressItemAsync(itemId, request);

            if (!result.Success)
            {
                return result.Message == "Work progress item not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data!, "Work progress item updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"updating work progress item {itemId}");
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

            if (!result.Success)
            {
                return result.Message == "Work progress item not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Work progress item deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"deleting work progress item {itemId}");
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private void AddHateoasLinks(DailyReportDto report)
    {
        report.Links = new List<LinkDto>
        {
            new LinkDto
            {
                Href = Url.Action(nameof(GetDailyReport), new { id = report.DailyReportId }),
                Rel = "self",
                Method = "GET"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(UpdateDailyReport), new { id = report.DailyReportId }),
                Rel = "update",
                Method = "PUT"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(DeleteDailyReport), new { id = report.DailyReportId }),
                Rel = "delete",
                Method = "DELETE"
            }
        };

        // Add status-specific actions
        if (report.Status == "Draft" || report.Status == "RevisionRequested")
        {
            report.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(SubmitDailyReport), new { id = report.DailyReportId }),
                Rel = "submit",
                Method = "POST"
            });
        }

        if (report.Status == "Submitted")
        {
            report.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(ApproveDailyReport), new { id = report.DailyReportId }),
                Rel = "approve",
                Method = "POST"
            });

            report.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(RejectDailyReport), new { id = report.DailyReportId }),
                Rel = "reject",
                Method = "POST"
            });
        }
    }

    #endregion
}
