using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using dotnet_rest_api.Models;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/weekly-reports")]
[Authorize]
public class WeeklyReportsController : BaseApiController
{
    private readonly IWeeklyReportService _weeklyReportService;
    private readonly ILogger<WeeklyReportsController> _logger;

    public WeeklyReportsController(
        IWeeklyReportService weeklyReportService, 
        ILogger<WeeklyReportsController> logger)
    {
        _weeklyReportService = weeklyReportService;
        _logger = logger;
    }

    /// <summary>
    /// Get weekly report by ID
    /// Available to: All authenticated users (view reports)
    /// </summary>
    [HttpGet("{reportId:guid}")]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> GetWeeklyReport(Guid reportId)
    {
        LogControllerAction(_logger, "GetWeeklyReport", reportId);

        var result = await _weeklyReportService.GetWeeklyReportByIdAsync(reportId);
        return result;
    }

    /// <summary>
    /// Get all weekly reports with advanced filtering and pagination
    /// Available to: All authenticated users (view reports)
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WeeklyReportDto>>>> GetWeeklyReports(
        [FromQuery] WeeklyReportQueryParameters parameters)
    {
        LogControllerAction(_logger, "GetWeeklyReports", parameters);

        // Parse dynamic filters from query string using the base controller method
        var filterString = Request.Query["filter"].FirstOrDefault();
        ApplyFiltersFromQuery(parameters, filterString);

        var result = await _weeklyReportService.GetWeeklyReportsAsync(parameters);
        return result;
    }

    /// <summary>
    /// Create a new weekly report
    /// Available to: Administrator, ProjectManager (reporting authority - Planner cannot create reports)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> CreateWeeklyReport(
        [FromBody] CreateWeeklyReportDto request)
    {
        LogControllerAction(_logger, "CreateWeeklyReport", request);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _weeklyReportService.CreateWeeklyReportAsync(request);
        return result;
    }

    /// <summary>
    /// Update an existing weekly report
    /// Available to: Administrator, ProjectManager (can manage reports)
    /// </summary>
    [HttpPut("{reportId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> UpdateWeeklyReport(
        Guid reportId, [FromBody] UpdateWeeklyReportDto request)
    {
        LogControllerAction(_logger, "UpdateWeeklyReport", new { reportId, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _weeklyReportService.UpdateWeeklyReportAsync(reportId, request);
        return result;
    }

    /// <summary>
    /// Submit weekly report for approval
    /// Available to: Administrator, ProjectManager (can submit reports)
    /// </summary>
    [HttpPost("{reportId:guid}/submit")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> SubmitWeeklyReport(Guid reportId)
    {
        LogControllerAction(_logger, "SubmitWeeklyReport", reportId);

        var result = await _weeklyReportService.UpdateWeeklyReportStatusAsync(
            reportId, WeeklyReportStatus.Submitted);
        return result;
    }

    /// <summary>
    /// Approve weekly report
    /// Available to: Administrator, ProjectManager (approval authority)
    /// </summary>
    [HttpPost("{reportId:guid}/approve")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> ApproveWeeklyReport(Guid reportId)
    {
        LogControllerAction(_logger, "ApproveWeeklyReport", reportId);

        var result = await _weeklyReportService.UpdateWeeklyReportStatusAsync(
            reportId, WeeklyReportStatus.Approved);
        return result;
    }

    /// <summary>
    /// Delete weekly report
    /// Available to: Administrator only (full system access)
    /// </summary>
    [HttpDelete("{reportId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWeeklyReport(Guid reportId)
    {
        LogControllerAction(_logger, "DeleteWeeklyReport", reportId);

        var result = await _weeklyReportService.DeleteWeeklyReportAsync(reportId);
        return result;
    }
}

/// <summary>
/// Project-specific weekly reports endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/weekly-reports")]
[Authorize]
public class ProjectWeeklyReportsController : BaseApiController
{
    private readonly IWeeklyReportService _weeklyReportService;
    private readonly ILogger<ProjectWeeklyReportsController> _logger;

    public ProjectWeeklyReportsController(
        IWeeklyReportService weeklyReportService, 
        ILogger<ProjectWeeklyReportsController> logger)
    {
        _weeklyReportService = weeklyReportService;
        _logger = logger;
    }

    /// <summary>
    /// Get weekly reports for a specific project
    /// Available to: All authenticated users (view project reports)
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WeeklyReportDto>>>> GetProjectWeeklyReports(
        Guid projectId, [FromQuery] WeeklyReportQueryParameters parameters)
    {
        LogControllerAction(_logger, "GetProjectWeeklyReports", new { projectId, parameters });

        // Parse dynamic filters from query string using the base controller method
        var filterString = Request.Query["filter"].FirstOrDefault();
        ApplyFiltersFromQuery(parameters, filterString);

        var result = await _weeklyReportService.GetProjectWeeklyReportsAsync(projectId, parameters);
        return result;
    }

    /// <summary>
    /// Create a new weekly report for a specific project
    /// Available to: Administrator, ProjectManager (reporting authority - Planner cannot create reports)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyReportDto>>> CreateProjectWeeklyReport(
        Guid projectId, [FromBody] CreateWeeklyReportDto request)
    {
        LogControllerAction(_logger, "CreateProjectWeeklyReport", new { projectId, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the project ID matches the route parameter
        request.ProjectId = projectId;

        var result = await _weeklyReportService.CreateWeeklyReportAsync(request);
        return result;
    }
}
