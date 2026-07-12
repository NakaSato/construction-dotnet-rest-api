using dotnet_rest_api.Common;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace dotnet_rest_api.Services.Infrastructure;

public interface ICalendarService
{
    Task<Result<object>> GetCalendarEventsAsync();
    Task<Result<object>> CreateCalendarEventAsync(object request);
    Task<Result<PaginatedCalendarEventsDto>> GetEventsAsync(CalendarQueryDto query);
    Task<Result<CalendarEventResponseDto>> GetEventByIdAsync(Guid id);
    Task<Result<CalendarEventResponseDto>> CreateEventAsync(CreateCalendarEventDto request, Guid createdByUserId);
    Task<Result<CalendarEventResponseDto>> UpdateEventAsync(Guid id, UpdateCalendarEventDto request);
    Task<Result<bool>> DeleteEventAsync(Guid id);
    Task<Result<PagedResult<CalendarEventSummaryDto>>> GetProjectEventsAsync(Guid projectId, int pageNumber, int pageSize);
    Task<Result<PagedResult<CalendarEventSummaryDto>>> GetTaskEventsAsync(Guid taskId, int pageNumber, int pageSize);
    Task<Result<PagedResult<CalendarEventSummaryDto>>> GetUserEventsAsync(Guid userId, int pageNumber, int pageSize);
    Task<Result<IEnumerable<CalendarEventSummaryDto>>> GetUpcomingEventsAsync(int days, Guid? userId);
    Task<Result<ConflictCheckResult>> CheckConflictsAsync(DateTime startDateTime, DateTime endDateTime, Guid userId, Guid? excludeEventId);
    Task<Result<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventsAsync();
    Task<Result<CalendarEventResponseDto>> CreateRecurringEventAsync(CreateCalendarEventDto request, Guid createdByUserId);
    Task<Result<bool>> UpdateRecurringEventAsync(Guid seriesId, UpdateCalendarEventDto request, bool updateAllInstances);
    Task<Result<bool>> DeleteRecurringEventAsync(Guid seriesId, bool deleteAllInstances);
}

/// <summary>
/// Real EF Core-backed implementation of <see cref="ICalendarService"/>. Replaces
/// the former StubCalendarService. Persists calendar events to the CalendarEvents
/// table and supports filtering, pagination, conflict detection and upcoming/
/// recurring queries. A recurring event is stored as a single master row
/// (IsRecurring + RecurrencePattern); occurrence expansion is intentionally not
/// implemented — callers read the pattern and expand client-side.
/// </summary>
public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(ApplicationDbContext context, ILogger<CalendarService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<object>> GetCalendarEventsAsync()
    {
        var events = await BuildBaseQuery()
            .OrderBy(e => e.StartDateTime)
            .Take(100)
            .ToListAsync();
        return Result<object>.SuccessResult(events.Select(ToSummary).ToList(), "Calendar events retrieved successfully");
    }

    public Task<Result<object>> CreateCalendarEventAsync(object request)
        => Task.FromResult(Result<object>.ErrorResult("Use CreateEventAsync with a CreateCalendarEventDto"));

    public async Task<Result<PaginatedCalendarEventsDto>> GetEventsAsync(CalendarQueryDto query)
    {
        var q = BuildBaseQuery();

        if (query.StartDate.HasValue)
            q = q.Where(e => e.StartDateTime >= query.StartDate.Value);
        if (query.EndDate.HasValue)
            q = q.Where(e => e.StartDateTime <= query.EndDate.Value);
        if (query.EventType.HasValue)
            q = q.Where(e => e.EventType == query.EventType.Value);
        if (query.Status.HasValue)
            q = q.Where(e => e.Status == query.Status.Value);
        if (query.Priority.HasValue)
            q = q.Where(e => e.Priority == query.Priority.Value);
        if (query.ProjectId.HasValue)
            q = q.Where(e => e.ProjectId == query.ProjectId.Value);
        if (query.TaskId.HasValue)
            q = q.Where(e => e.TaskId == query.TaskId.Value);
        if (query.AssignedToUserId.HasValue)
            q = q.Where(e => e.AssignedToUserId == query.AssignedToUserId.Value);
        if (query.CreatedByUserId.HasValue)
            q = q.Where(e => e.CreatedByUserId == query.CreatedByUserId.Value);
        if (!query.IncludePrivate)
            q = q.Where(e => !e.IsPrivate);
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(e => e.Title.Contains(query.Search) || (e.Description != null && e.Description.Contains(query.Search)));

        var descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        q = (query.SortBy?.ToLowerInvariant()) switch
        {
            "title" => descending ? q.OrderByDescending(e => e.Title) : q.OrderBy(e => e.Title),
            "createdat" => descending ? q.OrderByDescending(e => e.CreatedAt) : q.OrderBy(e => e.CreatedAt),
            _ => descending ? q.OrderByDescending(e => e.StartDateTime) : q.OrderBy(e => e.StartDateTime)
        };

        var totalCount = await q.CountAsync();
        var events = await q
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var dto = new PaginatedCalendarEventsDto
        {
            Events = events.Select(ToSummary).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages,
            HasNextPage = query.Page < totalPages,
            HasPreviousPage = query.Page > 1
        };
        return Result<PaginatedCalendarEventsDto>.SuccessResult(dto, "Calendar events retrieved successfully");
    }

    public async Task<Result<CalendarEventResponseDto>> GetEventByIdAsync(Guid id)
    {
        var ev = await BuildBaseQuery().FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null)
            return Result<CalendarEventResponseDto>.NotFoundResult("Calendar event not found");
        return Result<CalendarEventResponseDto>.SuccessResult(ToResponse(ev), "Calendar event retrieved successfully");
    }

    public async Task<Result<CalendarEventResponseDto>> CreateEventAsync(CreateCalendarEventDto request, Guid createdByUserId)
    {
        var validation = await ValidateEventAsync(request.StartDateTime, request.EndDateTime, request.ProjectId, request.TaskId, request.AssignedToUserId);
        if (validation != null)
            return Result<CalendarEventResponseDto>.ErrorResult(validation);

        var ev = new CalendarEvent
        {
            EventId = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
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
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(ev);
        await _context.SaveChangesAsync();

        var created = await BuildBaseQuery().FirstOrDefaultAsync(e => e.EventId == ev.EventId) ?? ev;
        return Result<CalendarEventResponseDto>.SuccessResult(ToResponse(created), "Calendar event created successfully");
    }

    public async Task<Result<CalendarEventResponseDto>> UpdateEventAsync(Guid id, UpdateCalendarEventDto request)
    {
        var ev = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null)
            return Result<CalendarEventResponseDto>.NotFoundResult("Calendar event not found");

        if (request.Title != null) ev.Title = request.Title;
        if (request.Description != null) ev.Description = request.Description;
        if (request.StartDateTime.HasValue) ev.StartDateTime = request.StartDateTime.Value;
        if (request.EndDateTime.HasValue) ev.EndDateTime = request.EndDateTime.Value;
        if (request.IsAllDay.HasValue) ev.IsAllDay = request.IsAllDay.Value;
        if (request.EventType.HasValue) ev.EventType = request.EventType.Value;
        if (request.Status.HasValue) ev.Status = request.Status.Value;
        if (request.Priority.HasValue) ev.Priority = request.Priority.Value;
        if (request.Location != null) ev.Location = request.Location;
        if (request.ProjectId.HasValue) ev.ProjectId = request.ProjectId.Value;
        if (request.TaskId.HasValue) ev.TaskId = request.TaskId.Value;
        if (request.AssignedToUserId.HasValue) ev.AssignedToUserId = request.AssignedToUserId.Value;
        if (request.IsRecurring.HasValue) ev.IsRecurring = request.IsRecurring.Value;
        if (request.RecurrencePattern != null) ev.RecurrencePattern = request.RecurrencePattern;
        if (request.RecurrenceEndDate.HasValue) ev.RecurrenceEndDate = request.RecurrenceEndDate.Value;
        if (request.ReminderMinutes.HasValue) ev.ReminderMinutes = request.ReminderMinutes.Value;
        if (request.IsPrivate.HasValue) ev.IsPrivate = request.IsPrivate.Value;
        if (request.MeetingUrl != null) ev.MeetingUrl = request.MeetingUrl;
        if (request.Attendees != null) ev.Attendees = request.Attendees;
        if (request.Notes != null) ev.Notes = request.Notes;

        if (ev.EndDateTime <= ev.StartDateTime)
            return Result<CalendarEventResponseDto>.ErrorResult("End time must be after start time");

        ev.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var updated = await BuildBaseQuery().FirstOrDefaultAsync(e => e.EventId == id) ?? ev;
        return Result<CalendarEventResponseDto>.SuccessResult(ToResponse(updated), "Calendar event updated successfully");
    }

    public async Task<Result<bool>> DeleteEventAsync(Guid id)
    {
        var ev = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null)
            return Result<bool>.NotFoundResult("Calendar event not found");

        _context.CalendarEvents.Remove(ev);
        await _context.SaveChangesAsync();
        return Result<bool>.SuccessResult(true, "Calendar event deleted successfully");
    }

    public Task<Result<PagedResult<CalendarEventSummaryDto>>> GetProjectEventsAsync(Guid projectId, int pageNumber, int pageSize)
        => PagedSummariesAsync(e => e.ProjectId == projectId, pageNumber, pageSize, "Project events retrieved successfully");

    public Task<Result<PagedResult<CalendarEventSummaryDto>>> GetTaskEventsAsync(Guid taskId, int pageNumber, int pageSize)
        => PagedSummariesAsync(e => e.TaskId == taskId, pageNumber, pageSize, "Task events retrieved successfully");

    public Task<Result<PagedResult<CalendarEventSummaryDto>>> GetUserEventsAsync(Guid userId, int pageNumber, int pageSize)
        => PagedSummariesAsync(e => e.CreatedByUserId == userId || e.AssignedToUserId == userId, pageNumber, pageSize, "User events retrieved successfully");

    public async Task<Result<IEnumerable<CalendarEventSummaryDto>>> GetUpcomingEventsAsync(int days, Guid? userId)
    {
        var now = DateTime.UtcNow;
        var until = now.AddDays(days);
        var q = BuildBaseQuery().Where(e => e.StartDateTime >= now && e.StartDateTime <= until);
        if (userId.HasValue && userId.Value != Guid.Empty)
            q = q.Where(e => e.CreatedByUserId == userId.Value || e.AssignedToUserId == userId.Value);

        var events = await q.OrderBy(e => e.StartDateTime).ToListAsync();
        return Result<IEnumerable<CalendarEventSummaryDto>>.SuccessResult(
            events.Select(ToSummary).ToList(), "Upcoming events retrieved successfully");
    }

    public async Task<Result<ConflictCheckResult>> CheckConflictsAsync(DateTime startDateTime, DateTime endDateTime, Guid userId, Guid? excludeEventId)
    {
        // Overlap: existing.Start < requested.End AND existing.End > requested.Start.
        var q = BuildBaseQuery().Where(e =>
            e.Status != CalendarEventStatus.Cancelled &&
            e.StartDateTime < endDateTime &&
            e.EndDateTime > startDateTime);

        if (userId != Guid.Empty)
            q = q.Where(e => e.CreatedByUserId == userId || e.AssignedToUserId == userId);
        if (excludeEventId.HasValue)
            q = q.Where(e => e.EventId != excludeEventId.Value);

        var conflicts = await q.OrderBy(e => e.StartDateTime).ToListAsync();
        var result = new ConflictCheckResult
        {
            HasConflicts = conflicts.Count > 0,
            ConflictingEvents = conflicts.Select(ToSummary).ToList()
        };
        return Result<ConflictCheckResult>.SuccessResult(result,
            result.HasConflicts ? $"{conflicts.Count} conflicting event(s) found" : "No conflicts found");
    }

    public async Task<Result<IEnumerable<CalendarEventSummaryDto>>> GetRecurringEventsAsync()
    {
        var events = await BuildBaseQuery()
            .Where(e => e.IsRecurring)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
        return Result<IEnumerable<CalendarEventSummaryDto>>.SuccessResult(
            events.Select(ToSummary).ToList(), "Recurring events retrieved successfully");
    }

    public async Task<Result<CalendarEventResponseDto>> CreateRecurringEventAsync(CreateCalendarEventDto request, Guid createdByUserId)
    {
        // Persist as a single master occurrence flagged recurring.
        request.IsRecurring = true;
        if (string.IsNullOrWhiteSpace(request.RecurrencePattern))
            return Result<CalendarEventResponseDto>.ErrorResult("RecurrencePattern is required for a recurring event");
        return await CreateEventAsync(request, createdByUserId);
    }

    public async Task<Result<bool>> UpdateRecurringEventAsync(Guid seriesId, UpdateCalendarEventDto request, bool updateAllInstances)
    {
        // Single master row model: the series id is the event id.
        var result = await UpdateEventAsync(seriesId, request);
        return result.IsSuccess
            ? Result<bool>.SuccessResult(true, "Recurring event updated successfully")
            : Result<bool>.ErrorResult(result.Message ?? "Recurring event not found");
    }

    public async Task<Result<bool>> DeleteRecurringEventAsync(Guid seriesId, bool deleteAllInstances)
        => await DeleteEventAsync(seriesId);

    // -------------------------------------------------------------- Helpers --

    private IQueryable<CalendarEvent> BuildBaseQuery()
        => _context.CalendarEvents
            .Include(e => e.Project)
            .Include(e => e.Task)
            .Include(e => e.CreatedBy)
            .Include(e => e.AssignedTo)
            .AsQueryable();

    private async Task<Result<PagedResult<CalendarEventSummaryDto>>> PagedSummariesAsync(
        System.Linq.Expressions.Expression<Func<CalendarEvent, bool>> predicate, int pageNumber, int pageSize, string message)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var q = BuildBaseQuery().Where(predicate);
        var totalCount = await q.CountAsync();
        var events = await q
            .OrderBy(e => e.StartDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<CalendarEventSummaryDto>
        {
            Items = events.Select(ToSummary).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        return Result<PagedResult<CalendarEventSummaryDto>>.SuccessResult(result, message);
    }

    /// <summary>Validates time ordering and optional foreign keys. Returns an error message or null.</summary>
    private async Task<string?> ValidateEventAsync(DateTime start, DateTime end, Guid? projectId, Guid? taskId, Guid? assignedToUserId)
    {
        if (end <= start)
            return "End time must be after start time";
        if (projectId.HasValue && !await _context.Projects.AnyAsync(p => p.ProjectId == projectId.Value))
            return "Project not found";
        if (taskId.HasValue && !await _context.ProjectTasks.AnyAsync(t => t.TaskId == taskId.Value))
            return "Task not found";
        if (assignedToUserId.HasValue && !await _context.Users.AnyAsync(u => u.UserId == assignedToUserId.Value))
            return "Assigned user not found";
        return null;
    }

    private static CalendarEventResponseDto ToResponse(CalendarEvent e) => new()
    {
        Id = e.EventId,
        Title = e.Title,
        Description = e.Description,
        StartDateTime = e.StartDateTime,
        EndDateTime = e.EndDateTime,
        IsAllDay = e.IsAllDay,
        EventType = e.EventType,
        EventTypeName = e.EventType.ToString(),
        Status = e.Status,
        StatusName = e.Status.ToString(),
        Priority = e.Priority,
        PriorityName = e.Priority.ToString(),
        Location = e.Location,
        ProjectId = e.ProjectId,
        ProjectName = e.Project?.ProjectName,
        TaskId = e.TaskId,
        TaskName = e.Task?.Title,
        CreatedByUserId = e.CreatedByUserId,
        CreatedByUserName = e.CreatedBy?.FullName,
        AssignedToUserId = e.AssignedToUserId,
        AssignedToUserName = e.AssignedTo?.FullName,
        IsRecurring = e.IsRecurring,
        RecurrencePattern = e.RecurrencePattern,
        RecurrenceEndDate = e.RecurrenceEndDate,
        ReminderMinutes = e.ReminderMinutes ?? 0,
        IsPrivate = e.IsPrivate,
        MeetingUrl = e.MeetingUrl,
        Attendees = e.Attendees,
        Notes = e.Notes,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt ?? e.CreatedAt
    };

    private static CalendarEventSummaryDto ToSummary(CalendarEvent e) => new()
    {
        Id = e.EventId,
        Title = e.Title,
        StartDateTime = e.StartDateTime,
        EndDateTime = e.EndDateTime,
        IsAllDay = e.IsAllDay,
        EventType = e.EventType,
        EventTypeName = e.EventType.ToString(),
        Status = e.Status,
        StatusName = e.Status.ToString(),
        Priority = e.Priority,
        PriorityName = e.Priority.ToString(),
        Location = e.Location,
        ProjectName = e.Project?.ProjectName,
        TaskName = e.Task?.Title,
        IsRecurring = e.IsRecurring
    };
}
