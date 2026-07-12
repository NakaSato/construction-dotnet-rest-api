using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Common;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Services.Users;
using dotnet_rest_api.Services.Tasks;
using dotnet_rest_api.Services.Projects;
using dotnet_rest_api.Services.MasterPlans;
using dotnet_rest_api.Services.WBS;
using dotnet_rest_api.Services.Infrastructure;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Calendar API controller for managing calendar events and scheduling.
/// The service returns <see cref="Result{T}"/>; this controller converts those
/// to the <see cref="ApiResponse{T}"/> envelope via BaseApiController helpers.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/calendar")]
[ApiController]
[Authorize]
public class CalendarController : BaseApiController
{
    private readonly ICalendarService _calendarService;

    public CalendarController(
        ICalendarService calendarService,
        ILogger<CalendarController> logger,
        IUserContextService userContextService)
        : base(logger, userContextService)
    {
        _calendarService = calendarService;
    }

    /// <summary>
    /// Get all calendar events with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedCalendarEventsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedCalendarEventsDto>), 400)]
    public async Task<ActionResult<ApiResponse<PaginatedCalendarEventsDto>>> GetEvents([FromQuery] CalendarQueryDto query)
    {
        if (query.Page < 1)
            query.Page = 1;
        if (query.PageSize < 1 || query.PageSize > 100)
            query.PageSize = 20;

        return ToApiResponse(await _calendarService.GetEventsAsync(query));
    }

    /// <summary>
    /// Get a specific calendar event by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 404)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> GetEvent(Guid id)
    {
        return ToApiResponse(await _calendarService.GetEventByIdAsync(id));
    }

    /// <summary>
    /// Create a new calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> CreateEvent([FromBody] CreateCalendarEventDto request)
    {
        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<CalendarEventResponseDto>();

        var createdByUserId = GetCurrentUserId();
        if (createdByUserId == null)
            return CreateErrorResponse<CalendarEventResponseDto>("Invalid user authentication", 401);

        return ToCreatedResponse(await _calendarService.CreateEventAsync(request, createdByUserId.Value));
    }

    /// <summary>
    /// Update an existing calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 404)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> UpdateEvent(Guid id, [FromBody] UpdateCalendarEventDto request)
    {
        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<CalendarEventResponseDto>();

        return ToApiResponse(await _calendarService.UpdateEventAsync(id, request));
    }

    /// <summary>
    /// Delete a calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvent(Guid id)
    {
        return ToApiResponse(await _calendarService.DeleteEventAsync(id));
    }

    /// <summary>
    /// Get calendar events for a specific project
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetProjectEvents(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        return ToApiResponse(await _calendarService.GetProjectEventsAsync(projectId, pageNumber, pageSize));
    }

    /// <summary>
    /// Get calendar events for a specific task
    /// </summary>
    [HttpGet("task/{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetTaskEvents(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        return ToApiResponse(await _calendarService.GetTaskEventsAsync(taskId, pageNumber, pageSize));
    }

    /// <summary>
    /// Get calendar events for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetUserEvents(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        return ToApiResponse(await _calendarService.GetUserEventsAsync(userId, pageNumber, pageSize));
    }

    /// <summary>
    /// Get upcoming events
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 400)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventSummaryDto>>>> GetUpcomingEvents(
        [FromQuery] int days = 7,
        [FromQuery] Guid? userId = null)
    {
        if (days < 1 || days > 365) days = 7;

        return ToApiResponse(await _calendarService.GetUpcomingEventsAsync(days, userId));
    }

    /// <summary>
    /// Check for conflicting events in a time range
    /// </summary>
    [HttpPost("conflicts")]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResult>), 400)]
    public async Task<ActionResult<ApiResponse<ConflictCheckResult>>> CheckEventConflicts([FromBody] ConflictCheckRequest request)
    {
        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<ConflictCheckResult>();

        return ToApiResponse(await _calendarService.CheckConflictsAsync(
            request.StartDateTime, request.EndDateTime, request.UserId ?? Guid.Empty, request.ExcludeEventId));
    }

    /// <summary>
    /// Get recurring events
    /// </summary>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 200)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventSummaryDto>>>> GetRecurringEvents()
    {
        return ToApiResponse(await _calendarService.GetRecurringEventsAsync());
    }

    /// <summary>
    /// Create a recurring calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPost("recurring")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> CreateRecurringEvent([FromBody] CreateCalendarEventDto request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return CreateErrorResponse<CalendarEventResponseDto>("User not authenticated", 401);

        return ToCreatedResponse(await _calendarService.CreateRecurringEventAsync(request, currentUserId.Value));
    }

    /// <summary>
    /// Update a recurring calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpPut("recurring/{seriesId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRecurringEvent(Guid seriesId, [FromBody] UpdateCalendarEventDto request)
    {
        return ToApiResponse(await _calendarService.UpdateRecurringEventAsync(seriesId, request, true));
    }

    /// <summary>
    /// Delete a recurring calendar event
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    [HttpDelete("recurring/{seriesId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRecurringEvent(Guid seriesId)
    {
        return ToApiResponse(await _calendarService.DeleteRecurringEventAsync(seriesId, true));
    }
}
