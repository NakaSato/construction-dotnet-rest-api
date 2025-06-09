using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using System.Security.Claims;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Controller for managing daily construction reports
/// </summary>
[Route("api/v{version:apiVersion}/daily-reports")]
[ApiController]
[ApiVersion("1.0")]
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
    /// Get all daily reports with filtering and pagination
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of daily reports</returns>
    [HttpGet]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<EnhancedPagedResult<DailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<DailyReportDto>>>> GetDailyReports([FromQuery] DailyReportQueryParameters parameters)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetDailyReports", parameters);

            // Apply dynamic filters from query string using base controller method
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _dailyReportService.GetDailyReportsAsync(parameters);

            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                foreach (var report in result.Data.Items)
                {
                    AddHateoasLinks(report);
                }
            }

            return CreateSuccessResponse(result.Data!, "Daily reports retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving daily reports");
        }
    }

    /// <summary>
    /// Get a specific daily report by ID
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <returns>Daily report details</returns>
    [HttpGet("{id:guid}")]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> GetDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetDailyReport", new { id });

            var result = await _dailyReportService.GetDailyReportByIdAsync(id);

            if (!result.Success)
            {
                return CreateNotFoundResponse(result.Message);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Daily report retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving daily report {id}");
        }
    }

    /// <summary>
    /// Get daily reports for a specific project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of daily reports for the project</returns>
    [HttpGet("project/{projectId:guid}")]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DailyReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<DailyReportDto>>>> GetProjectDailyReports(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetProjectDailyReports", new { projectId, pageNumber, pageSize });

            var result = await _dailyReportService.GetProjectDailyReportsAsync(projectId, pageNumber, pageSize);

            if (!result.Success)
            {
                return CreateNotFoundResponse(result.Message);
            }

            // Add HATEOAS links
            if (result.Data?.Items != null)
            {
                foreach (var report in result.Data.Items)
                {
                    AddHateoasLinks(report);
                }
            }

            return CreateSuccessResponse(result.Data!, "Project daily reports retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving daily reports for project {projectId}");
        }
    }

    /// <summary>
    /// Create a new daily report
    /// </summary>
    /// <param name="request">Daily report creation request</param>
    /// <returns>Created daily report</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> CreateDailyReport([FromBody] CreateDailyReportRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "CreateDailyReport", request);

            if (!ModelState.IsValid)
            {
                return CreateErrorResponse("Invalid request data", 400);
            }

            var userId = GetCurrentUserId();
            var result = await _dailyReportService.CreateDailyReportAsync(request, userId);

            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Daily report created successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "creating daily report");
        }
    }

    /// <summary>
    /// Update an existing daily report
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <param name="request">Daily report update request</param>
    /// <returns>Updated daily report</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<DailyReportDto>>> UpdateDailyReport(Guid id, [FromBody] UpdateDailyReportRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateDailyReport", new { id, request });

            if (!ModelState.IsValid)
            {
                return CreateErrorResponse("Invalid request data", 400);
            }

            var result = await _dailyReportService.UpdateDailyReportAsync(id, request);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Daily report updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"updating daily report {id}");
        }
    }

    /// <summary>
    /// Delete a daily report
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDailyReport(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteDailyReport", new { id });

            var result = await _dailyReportService.DeleteDailyReportAsync(id);

            if (!result.Success)
            {
                return result.Message == "Daily report not found" ? CreateNotFoundResponse(result.Message) : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Daily report deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"deleting daily report {id}");
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
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Manager,Admin")]
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
    /// </summary>
    /// <param name="id">The daily report ID</param>
    /// <param name="rejectionReason">Reason for rejection</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Manager,Admin")]
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
