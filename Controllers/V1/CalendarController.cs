using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Controllers;
using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Calendar API controller for managing calendar events and scheduling
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class CalendarController : BaseApiController
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    /// <summary>
    /// Get all calendar events with filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering events</param>
    /// <returns>Paginated list of calendar events</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedCalendarEventsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedCalendarEventsDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedCalendarEventsDto>), 500)]
    public async Task<ActionResult<ApiResponse<PaginatedCalendarEventsDto>>> GetEvents([FromQuery] CalendarQueryDto query)
    {
        // Validate pagination parameters
        if (query.Page < 1)
            query.Page = 1;
        
        if (query.PageSize < 1 || query.PageSize > 100)
            query.PageSize = 20;

        var result = await _calendarService.GetEventsAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific calendar event by ID
    /// </summary>
    /// <param name="id">The calendar event ID</param>
    /// <returns>Calendar event details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 404)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 500)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> GetEvent(Guid id)
    {
        var result = await _calendarService.GetEventByIdAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return result.Message.Contains("not found") ? NotFound(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Create a new calendar event
    /// </summary>
    /// <param name="request">Calendar event creation request</param>
    /// <returns>Created calendar event</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 500)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> CreateEvent([FromBody] CreateCalendarEventDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<CalendarEventResponseDto>
            {
                Success = false,
                Message = "Invalid request data",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        // Get current user ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var createdByUserId))
        {
            return BadRequest(new ApiResponse<CalendarEventResponseDto>
            {
                Success = false,
                Message = "Invalid user authentication",
                Data = null,
                Errors = new List<string> { "User ID not found in authentication token" }
            });
        }

        var result = await _calendarService.CreateEventAsync(request, createdByUserId);
        
        if (result.Success)
            return StatusCode(201, result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Update an existing calendar event
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 404)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 500)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> UpdateEvent(Guid id, [FromBody] UpdateCalendarEventDto request)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _calendarService.UpdateEventAsync(id, request);
            
            if (!result.Success)
            {
                return result.Message.Contains("not found") 
                    ? CreateNotFoundResponse(result.Message)
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data!, "Calendar event updated successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, CreateErrorResponse("An error occurred while updating the calendar event", 500));
        }
    }

    /// <summary>
    /// Delete a calendar event
    /// </summary>
    /// <param name="id">The calendar event ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvent(Guid id)
    {
        var result = await _calendarService.DeleteEventAsync(id);
        
        if (result.Success)
            return Ok(result);
        
        return result.Message.Contains("not found") ? NotFound(result) : StatusCode(500, result);
    }

    /// <summary>
    /// Get calendar events for a specific project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size (max 100)</param>
    /// <returns>Paginated list of project calendar events</returns>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetProjectEvents(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _calendarService.GetProjectEventsAsync(projectId, pageNumber, pageSize);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get calendar events for a specific task
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size (max 100)</param>
    /// <returns>Paginated list of task calendar events</returns>
    [HttpGet("task/{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetTaskEvents(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _calendarService.GetTaskEventsAsync(taskId, pageNumber, pageSize);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get calendar events for a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size (max 100)</param>
    /// <returns>Paginated list of user calendar events</returns>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 400)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CalendarEventSummaryDto>>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<CalendarEventSummaryDto>>>> GetUserEvents(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _calendarService.GetUserEventsAsync(userId, pageNumber, pageSize);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get upcoming events
    /// </summary>
    /// <param name="days">Number of days to look ahead (default: 7)</param>
    /// <param name="userId">Optional user ID to filter events</param>
    /// <returns>List of upcoming events</returns>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 400)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventSummaryDto>>>> GetUpcomingEvents(
        [FromQuery] int days = 7,
        [FromQuery] Guid? userId = null)
    {
        if (days < 1 || days > 365) days = 7;

        var result = await _calendarService.GetUpcomingEventsAsync(days, userId);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Check for conflicting events in a time range
    /// </summary>
    /// <param name="request">Conflict check request</param>
    /// <returns>Conflict check result</returns>
    [HttpPost("conflicts")]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResult>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ConflictCheckResult>), 500)]
    public async Task<ActionResult<ApiResponse<ConflictCheckResult>>> CheckEventConflicts([FromBody] ConflictCheckRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ConflictCheckResult>
            {
                Success = false,
                Message = "Invalid request data",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var result = await _calendarService.CheckConflictsAsync(request.StartDateTime, request.EndDateTime, request.UserId, request.ExcludeEventId);
        
        if (result.Success)
            return Ok(result);
        
        return BadRequest(result);
    }

    /// <summary>
    /// Get recurring events (placeholder)
    /// </summary>
    /// <returns>List of recurring events</returns>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CalendarEventSummaryDto>>), 501)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CalendarEventSummaryDto>>>> GetRecurringEvents()
    {
        var result = await _calendarService.GetRecurringEventsAsync();
        
        if (result.Success)
            return Ok(result);
        
        return StatusCode(501, result);  // Not implemented
    }

    /// <summary>
    /// Create a recurring calendar event (placeholder)
    /// </summary>
    /// <param name="request">Recurring calendar event creation request</param>
    /// <returns>Created recurring calendar event</returns>
    [HttpPost("recurring")]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<CalendarEventResponseDto>), 501)]
    public async Task<ActionResult<ApiResponse<CalendarEventResponseDto>>> CreateRecurringEvent([FromBody] CreateCalendarEventDto request)
    {
        // For now, using a hardcoded user ID. In a real application, this would come from authentication
        Guid createdByUserId = Guid.NewGuid(); // TODO: Get from authenticated user context

        var result = await _calendarService.CreateRecurringEventAsync(request, createdByUserId);
        
        if (result.Success)
            return StatusCode(201, result);
        
        return StatusCode(501, result);  // Not implemented
    }

    /// <summary>
    /// Update a recurring calendar event (placeholder)
    /// </summary>
    /// <param name="seriesId">The recurring event series ID</param>
    /// <param name="request">Recurring calendar event update request</param>
    /// <returns>Success status</returns>
    [HttpPut("recurring/{seriesId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 501)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateRecurringEvent(Guid seriesId, [FromBody] UpdateCalendarEventDto request)
    {
        var result = await _calendarService.UpdateRecurringEventAsync(seriesId, request, true);
        
        if (result.Success)
            return Ok(result);
        
        return StatusCode(501, result);  // Not implemented
    }

    /// <summary>
    /// Delete a recurring calendar event (placeholder)
    /// </summary>
    /// <param name="seriesId">The recurring event series ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("recurring/{seriesId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 501)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRecurringEvent(Guid seriesId)
    {
        var result = await _calendarService.DeleteRecurringEventAsync(seriesId, true);
        
        if (result.Success)
            return Ok(result);
        
        return StatusCode(501, result);  // Not implemented
    }
}
