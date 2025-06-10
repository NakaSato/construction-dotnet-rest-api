using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Data;
using dotnet_rest_api.Common;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _context;

    public CalendarService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CalendarEventResponseDto>> GetEventByIdAsync(Guid eventId)
    {
        try
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (calendarEvent == null)
            {
                return ApiResponse<CalendarEventResponseDto>.ErrorResponse("Calendar event not found");
            }

            var response = MapToResponseDto(calendarEvent);
            return ApiResponse<CalendarEventResponseDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<CalendarEventResponseDto>.ErrorResponse($"Error retrieving calendar event: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaginatedCalendarEventsDto>> GetEventsAsync(CalendarQueryDto query)
    {
        try
        {
            var queryable = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .AsQueryable();

            // Apply filters
            queryable = ApplyFilters(queryable, query);

            // Apply sorting
            queryable = ApplySorting(queryable, query.SortBy, query.SortOrder);

            // Get total count before pagination
            var totalCount = await queryable.CountAsync();

            // Apply pagination
            var events = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto).ToList();

            var result = new PaginatedCalendarEventsDto
            {
                Events = eventDtos,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
                HasNextPage = query.Page < (int)Math.Ceiling((double)totalCount / query.PageSize),
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PaginatedCalendarEventsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PaginatedCalendarEventsDto>.ErrorResponse($"Error retrieving calendar events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<CalendarEventSummaryDto>>> GetProjectEventsAsync(Guid projectId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var queryable = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.ProjectId == projectId);

            var totalCount = await queryable.CountAsync();
            var events = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto).ToList();

            var result = new PagedResult<CalendarEventSummaryDto>
            {
                Items = eventDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving project events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<CalendarEventSummaryDto>>> GetTaskEventsAsync(Guid taskId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var queryable = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.TaskId == taskId);

            var totalCount = await queryable.CountAsync();
            var events = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto).ToList();

            var result = new PagedResult<CalendarEventSummaryDto>
            {
                Items = eventDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving task events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<CalendarEventSummaryDto>>> GetUserEventsAsync(Guid userId, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var queryable = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.CreatedByUserId == userId || e.AssignedToUserId == userId);

            var totalCount = await queryable.CountAsync();
            var events = await queryable
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto).ToList();

            var result = new PagedResult<CalendarEventSummaryDto>
            {
                Items = eventDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving user events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CalendarEventResponseDto>> CreateEventAsync(CreateCalendarEventDto request, Guid createdByUserId)
    {
        try
        {
            // Basic validation
            if (request.EndDateTime <= request.StartDateTime)
            {
                return ApiResponse<CalendarEventResponseDto>.ErrorResponse("End date/time must be after start date/time");
            }

            // Check for conflicts if needed
            var conflictCheck = await CheckConflictsAsync(request.StartDateTime, request.EndDateTime, createdByUserId);
            if (conflictCheck.Data?.HasConflicts == true)
            {
                return ApiResponse<CalendarEventResponseDto>.ErrorResponse("Event conflicts with existing user schedule");
            }

            var calendarEvent = new CalendarEvent
            {
                EventId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartDateTime = request.StartDateTime.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(request.StartDateTime, DateTimeKind.Utc)
                    : request.StartDateTime.ToUniversalTime(),
                EndDateTime = request.EndDateTime.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(request.EndDateTime, DateTimeKind.Utc)
                    : request.EndDateTime.ToUniversalTime(),
                IsAllDay = request.IsAllDay,
                EventType = request.EventType,
                Status = request.Status,
                Priority = request.Priority,
                Location = request.Location,
                ProjectId = request.ProjectId,
                TaskId = request.TaskId,
                CreatedByUserId = createdByUserId,
                AssignedToUserId = request.AssignedToUserId,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern,
                RecurrenceEndDate = request.RecurrenceEndDate,
                ReminderMinutes = request.ReminderMinutes,
                IsPrivate = request.IsPrivate,
                MeetingUrl = request.MeetingUrl,
                Attendees = request.Attendees,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdEvent = await _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .FirstAsync(e => e.EventId == calendarEvent.EventId);

            var response = MapToResponseDto(createdEvent);
            return ApiResponse<CalendarEventResponseDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<CalendarEventResponseDto>.ErrorResponse($"Error creating calendar event: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CalendarEventResponseDto>> UpdateEventAsync(Guid eventId, UpdateCalendarEventDto request)
    {
        try
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(eventId);

            if (calendarEvent == null)
            {
                return ApiResponse<CalendarEventResponseDto>.ErrorResponse("Calendar event not found");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(request.Title))
                calendarEvent.Title = request.Title;

            if (request.Description != null)
                calendarEvent.Description = request.Description;

            if (request.StartDateTime.HasValue)
            {
                calendarEvent.StartDateTime = request.StartDateTime.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(request.StartDateTime.Value, DateTimeKind.Utc)
                    : request.StartDateTime.Value.ToUniversalTime();
            }

            if (request.EndDateTime.HasValue)
            {
                calendarEvent.EndDateTime = request.EndDateTime.Value.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(request.EndDateTime.Value, DateTimeKind.Utc)
                    : request.EndDateTime.Value.ToUniversalTime();
            }

            if (request.IsAllDay.HasValue)
                calendarEvent.IsAllDay = request.IsAllDay.Value;

            if (request.EventType.HasValue)
                calendarEvent.EventType = request.EventType.Value;

            if (request.Status.HasValue)
                calendarEvent.Status = request.Status.Value;

            if (request.Priority.HasValue)
                calendarEvent.Priority = request.Priority.Value;

            if (request.Location != null)
                calendarEvent.Location = request.Location;

            if (request.ProjectId.HasValue)
                calendarEvent.ProjectId = request.ProjectId.Value;

            if (request.TaskId.HasValue)
                calendarEvent.TaskId = request.TaskId.Value;

            if (request.AssignedToUserId.HasValue)
                calendarEvent.AssignedToUserId = request.AssignedToUserId.Value;

            if (request.IsRecurring.HasValue)
                calendarEvent.IsRecurring = request.IsRecurring.Value;

            if (request.RecurrencePattern != null)
                calendarEvent.RecurrencePattern = request.RecurrencePattern;

            if (request.RecurrenceEndDate.HasValue)
                calendarEvent.RecurrenceEndDate = request.RecurrenceEndDate.Value;

            if (request.ReminderMinutes.HasValue)
                calendarEvent.ReminderMinutes = request.ReminderMinutes.Value;

            if (request.IsPrivate.HasValue)
                calendarEvent.IsPrivate = request.IsPrivate.Value;

            if (request.MeetingUrl != null)
                calendarEvent.MeetingUrl = request.MeetingUrl;

            if (request.Attendees != null)
                calendarEvent.Attendees = request.Attendees;

            if (request.Notes != null)
                calendarEvent.Notes = request.Notes;

            // Validation after updates
            if (calendarEvent.EndDateTime <= calendarEvent.StartDateTime)
            {
                return ApiResponse<CalendarEventResponseDto>.ErrorResponse("End date/time must be after start date/time");
            }

            calendarEvent.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload with includes
            var updatedEvent = await _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .FirstAsync(e => e.EventId == calendarEvent.EventId);

            var response = MapToResponseDto(updatedEvent);
            return ApiResponse<CalendarEventResponseDto>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<CalendarEventResponseDto>.ErrorResponse($"Error updating calendar event: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteEventAsync(Guid eventId)
    {
        try
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(eventId);

            if (calendarEvent == null)
            {
                return ApiResponse<bool>.ErrorResponse("Calendar event not found");
            }

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Error deleting calendar event: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> UpdateEventStatusAsync(Guid eventId, CalendarEventStatus status)
    {
        try
        {
            var calendarEvent = await _context.CalendarEvents.FindAsync(eventId);

            if (calendarEvent == null)
            {
                return ApiResponse<bool>.ErrorResponse("Calendar event not found");
            }

            calendarEvent.Status = status;
            calendarEvent.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResponse($"Error updating calendar event status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<CalendarEventSummaryDto>>> GetUpcomingEventsAsync(int days = 7, Guid? userId = null)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(days);

            var query = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.StartDateTime >= startDate && e.StartDateTime <= endDate);

            if (userId.HasValue)
            {
                query = query.Where(e => e.CreatedByUserId == userId.Value || e.AssignedToUserId == userId.Value);
            }

            var events = await query
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto);
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.SuccessResponse(eventDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving upcoming events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<CalendarEventSummaryDto>>> GetTodayEventsAsync(Guid userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var events = await _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => (e.CreatedByUserId == userId || e.AssignedToUserId == userId) &&
                           e.StartDateTime >= today && e.StartDateTime < tomorrow)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var eventDtos = events.Select(MapToSummaryDto);
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.SuccessResponse(eventDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving today's events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<CalendarEventSummaryDto>>> GetConflictingEventsAsync(DateTime startDateTime, DateTime endDateTime, Guid? excludeEventId = null)
    {
        try
        {
            var query = _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.StartDateTime < endDateTime && e.EndDateTime > startDateTime);

            if (excludeEventId.HasValue)
            {
                query = query.Where(e => e.EventId != excludeEventId.Value);
            }

            var events = await query.ToListAsync();
            var eventDtos = events.Select(MapToSummaryDto);
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.SuccessResponse(eventDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.ErrorResponse($"Error checking for conflicting events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ConflictCheckResult>> CheckConflictsAsync(DateTime startDateTime, DateTime endDateTime, Guid? userId = null, Guid? excludeEventId = null)
    {
        try
        {
            var query = _context.CalendarEvents
                .Where(e => e.StartDateTime < endDateTime && e.EndDateTime > startDateTime);

            if (userId.HasValue)
            {
                query = query.Where(e => e.AssignedToUserId == userId.Value || e.CreatedByUserId == userId.Value);
            }

            if (excludeEventId.HasValue)
            {
                query = query.Where(e => e.EventId != excludeEventId.Value);
            }

            var conflictingEvents = await query.ToListAsync();
            var hasConflict = conflictingEvents.Any();
            
            var result = new ConflictCheckResult
            {
                HasConflicts = hasConflict,
                ConflictingEvents = conflictingEvents.Select(MapToSummaryDto).ToList()
            };
            
            return ApiResponse<ConflictCheckResult>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ConflictCheckResult>.ErrorResponse($"Error checking for event conflicts: {ex.Message}");
        }
    }

    // Recurring events methods (placeholder implementations)
    public async Task<ApiResponse<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventsAsync()
    {
        try
        {
            var recurringEvents = await _context.CalendarEvents
                .Include(e => e.Project)
                .Include(e => e.Task)
                .Where(e => e.IsRecurring)
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            var eventDtos = recurringEvents.Select(MapToSummaryDto);
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.SuccessResponse(eventDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.ErrorResponse($"Error retrieving recurring events: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventInstancesAsync(Guid eventId, DateTime startDate, DateTime endDate)
    {
        // TODO: Implement recurring event logic
        await Task.CompletedTask;
        return ApiResponse<IEnumerable<CalendarEventSummaryDto>>.ErrorResponse("Recurring events not yet implemented");
    }

    public async Task<ApiResponse<CalendarEventResponseDto>> CreateRecurringEventAsync(CreateCalendarEventDto request, Guid createdByUserId)
    {
        // For now, just create a regular event
        var result = await CreateEventAsync(request, createdByUserId);
        return result;
    }

    public async Task<ApiResponse<bool>> UpdateRecurringEventAsync(Guid eventId, UpdateCalendarEventDto request, bool updateAllInstances = false)
    {
        // For now, just update the single event
        var result = await UpdateEventAsync(eventId, request);
        return ApiResponse<bool>.SuccessResponse(result.Success);
    }

    public async Task<ApiResponse<bool>> DeleteRecurringEventAsync(Guid eventId, bool deleteAllInstances = false)
    {
        // For now, just delete the single event
        return await DeleteEventAsync(eventId);
    }

    // Private helper methods
    private IQueryable<CalendarEvent> ApplyFilters(IQueryable<CalendarEvent> query, CalendarQueryDto filters)
    {
        if (filters.StartDate.HasValue)
        {
            var startDate = filters.StartDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(filters.StartDate.Value, DateTimeKind.Utc)
                : filters.StartDate.Value.ToUniversalTime();
            query = query.Where(e => e.StartDateTime >= startDate);
        }

        if (filters.EndDate.HasValue)
        {
            var endDate = filters.EndDate.Value.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(filters.EndDate.Value, DateTimeKind.Utc)
                : filters.EndDate.Value.ToUniversalTime();
            query = query.Where(e => e.EndDateTime <= endDate);
        }

        if (filters.EventType.HasValue)
            query = query.Where(e => e.EventType == filters.EventType.Value);

        if (filters.Status.HasValue)
            query = query.Where(e => e.Status == filters.Status.Value);

        if (filters.Priority.HasValue)
            query = query.Where(e => e.Priority == filters.Priority.Value);

        if (filters.ProjectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == filters.ProjectId.Value);
        }

        if (filters.TaskId.HasValue)
        {
            query = query.Where(e => e.TaskId == filters.TaskId.Value);
        }

        if (filters.AssignedToUserId.HasValue)
        {
            query = query.Where(e => e.AssignedToUserId == filters.AssignedToUserId.Value);
        }

        if (filters.CreatedByUserId.HasValue)
        {
            query = query.Where(e => e.CreatedByUserId == filters.CreatedByUserId.Value);
        }

        if (!filters.IncludePrivate)
            query = query.Where(e => !e.IsPrivate);

        if (!string.IsNullOrEmpty(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(searchLower) || 
                                    (e.Description != null && e.Description.ToLower().Contains(searchLower)));
        }

        return query;
    }

    private IQueryable<CalendarEvent> ApplySorting(IQueryable<CalendarEvent> query, string sortBy, string sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "createdat" => isDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt) : query.OrderBy(e => e.UpdatedAt ?? e.CreatedAt),
            _ => isDescending ? query.OrderByDescending(e => e.StartDateTime) : query.OrderBy(e => e.StartDateTime)
        };
    }

    private CalendarEventResponseDto MapToResponseDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventResponseDto
        {
            Id = calendarEvent.EventId,
            Title = calendarEvent.Title,
            Description = calendarEvent.Description,
            StartDateTime = calendarEvent.StartDateTime,
            EndDateTime = calendarEvent.EndDateTime,
            IsAllDay = calendarEvent.IsAllDay,
            EventType = calendarEvent.EventType,
            EventTypeName = calendarEvent.EventType.ToString(),
            Status = calendarEvent.Status,
            StatusName = calendarEvent.Status.ToString(),
            Priority = calendarEvent.Priority,
            PriorityName = calendarEvent.Priority.ToString(),
            Location = calendarEvent.Location,
            ProjectId = calendarEvent.ProjectId,
            ProjectName = calendarEvent.Project?.ProjectName,
            TaskId = calendarEvent.TaskId,
            TaskName = calendarEvent.Task?.Title,
            CreatedByUserId = calendarEvent.CreatedByUserId,
            CreatedByUserName = null, // Would need to include User navigation property
            AssignedToUserId = calendarEvent.AssignedToUserId,
            AssignedToUserName = null, // Would need to include User navigation property
            IsRecurring = calendarEvent.IsRecurring,
            RecurrencePattern = calendarEvent.RecurrencePattern,
            RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
            ReminderMinutes = calendarEvent.ReminderMinutes ?? 15,
            IsPrivate = calendarEvent.IsPrivate,
            MeetingUrl = calendarEvent.MeetingUrl,
            Attendees = calendarEvent.Attendees,
            Notes = calendarEvent.Notes,
            CreatedAt = calendarEvent.CreatedAt,
            UpdatedAt = calendarEvent.UpdatedAt ?? calendarEvent.CreatedAt
        };
    }

    private CalendarEventSummaryDto MapToSummaryDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventSummaryDto
        {
            Id = calendarEvent.EventId,
            Title = calendarEvent.Title,
            StartDateTime = calendarEvent.StartDateTime,
            EndDateTime = calendarEvent.EndDateTime,
            IsAllDay = calendarEvent.IsAllDay,
            EventType = calendarEvent.EventType,
            EventTypeName = calendarEvent.EventType.ToString(),
            Status = calendarEvent.Status,
            StatusName = calendarEvent.Status.ToString(),
            Priority = calendarEvent.Priority,
            PriorityName = calendarEvent.Priority.ToString(),
            Location = calendarEvent.Location,
            ProjectName = calendarEvent.Project?.ProjectName,
            TaskName = calendarEvent.Task?.Title,
            IsRecurring = calendarEvent.IsRecurring
        };
    }
}
