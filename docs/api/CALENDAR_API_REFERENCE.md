## Calendar API

Provides functionality for managing calendar events and scheduling.

**Base Path:** `/api/v1/calendar`

| HTTP Method | Endpoint                         | Description                                          | Request Body                     | Response                                                                 |
|-------------|----------------------------------|------------------------------------------------------|----------------------------------|--------------------------------------------------------------------------|
| GET         | `/`                              | Get all calendar events with filtering and pagination | `CalendarQueryDto` (query params)  | `ApiResponse<PaginatedCalendarEventsDto>`                                |
| GET         | `/{id}`                          | Get a specific calendar event by ID                  | None                             | `ApiResponse<CalendarEventResponseDto>`                                  |
| POST        | `/`                              | Create a new calendar event                          | `CreateCalendarEventDto`           | `ApiResponse<CalendarEventResponseDto>` (201 Created)                     |
| PUT         | `/{id}`                          | Update an existing calendar event                    | `UpdateCalendarEventDto`           | `ApiResponse<CalendarEventResponseDto>`                                  |
| DELETE      | `/{id}`                          | Delete a calendar event                              | None                             | `ApiResponse<bool>`                                                      |
| GET         | `/project/{projectId}`           | Get calendar events for a specific project           | Page/PageSize (query params)     | `ApiResponse<PagedResult<CalendarEventSummaryDto>>`                      |
| GET         | `/task/{taskId}`                 | Get calendar events for a specific task              | Page/PageSize (query params)     | `ApiResponse<PagedResult<CalendarEventSummaryDto>>`                      |
| GET         | `/user/{userId}`                 | Get calendar events for a specific user              | Page/PageSize (query params)     | `ApiResponse<PagedResult<CalendarEventSummaryDto>>`                      |
| GET         | `/date-range`                    | Get events within a specific date range              | `DateRangeQueryDto` (query params) | `ApiResponse<List<CalendarEventResponseDto>>`                            |
| POST        | `/{id}/share`                    | Share a calendar event with users                    | `ShareCalendarEventDto`          | `ApiResponse<bool>`                                                      |
| GET         | `/upcoming`                      | Get upcoming calendar events for the current user    | `UpcomingEventsQueryDto` (query params) | `ApiResponse<List<CalendarEventResponseDto>>`                            |
| POST        | `/{id}/complete`                 | Mark a calendar event as complete                    | None                             | `ApiResponse<CalendarEventResponseDto>`                                  |
| POST        | `/{id}/reschedule`               | Reschedule a calendar event                          | `RescheduleEventDto`             | `ApiResponse<CalendarEventResponseDto>`                                  |
| GET         | `/search`                        | Search calendar events by keyword                    | `SearchQueryDto` (query params)  | `ApiResponse<PagedResult<CalendarEventSummaryDto>>`                      |
| GET         | `/availability`                  | Check availability for a specific time slot          | `AvailabilityQueryDto` (query params) | `ApiResponse<AvailabilityResponseDto>`                                 |
| POST        | `/batch`                         | Create multiple calendar events in a batch           | `List<CreateCalendarEventDto>`   | `ApiResponse<List<CalendarEventResponseDto>>` (201 Created)              |
| GET         | `/{id}/attendees`                | Get attendees for a specific calendar event          | None                             | `ApiResponse<List<UserSummaryDto>>`                                      |
| POST        | `/{id}/attendees`                | Add attendees to a calendar event                    | `AddAttendeesDto`                | `ApiResponse<bool>`                                                      |
| DELETE      | `/{id}/attendees/{attendeeId}`   | Remove an attendee from a calendar event             | None                             | `ApiResponse<bool>`                                                      |
| GET         | `/types`                         | Get available calendar event types                   | None                             | `ApiResponse<List<string>>`                                              |
| GET         | `/reminders`                     | Get reminders for upcoming events for the current user | `ReminderQueryDto` (query params) | `ApiResponse<List<CalendarEventReminderDto>>`                          |

**Note:** `id`, `projectId`, `taskId`, `userId`, and `attendeeId` are GUIDs.
