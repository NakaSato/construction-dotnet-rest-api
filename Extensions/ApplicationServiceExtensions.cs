using dotnet_rest_api.Services;
using dotnet_rest_api.Services.Infrastructure;
using dotnet_rest_api.Services.MasterPlans;
using dotnet_rest_api.Services.Projects;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Services.Tasks;
using dotnet_rest_api.Services.Users;
using dotnet_rest_api.Services.WBS;

namespace dotnet_rest_api.Extensions;

/// <summary>
/// Registers the application's business/feature services. Extracted from Program.cs
/// to keep composition-root wiring in one focused place.
///
/// NOTE: several registrations are still Stub* implementations (daily reports,
/// notifications, work requests, weekly reports, calendar, images, resources,
/// documents) — see docs/API_DESIGN_REVIEW.md Phase 5.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Business Services
        // (IDailyReportService exists in both Infrastructure and Shared — use Infrastructure.)
        services.AddScoped<Services.Infrastructure.IDailyReportService, DailyReportService>();

        // WBS Services
        services.AddScoped<WbsDataSeeder>();
        services.AddScoped<IWbsService, WbsService>();

        // Core / shared abstractions
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IValidationHelperService, ValidationHelperService>();

        // Service implementations
        // (ICacheService/CacheService exist in both Services and Services.Shared — use root Services.)
        services.AddScoped<Services.ICacheService, Services.CacheService>();
        services.AddScoped<IAuthService, AuthService>();

        // Project services (feature-based)
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectAnalyticsService, ProjectAnalyticsService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IMasterPlanService, MasterPlanService>();

        // Notifications
        services.AddScoped<INotificationService, NotificationService>();

        // Work requests
        services.AddScoped<IWorkRequestService, WorkRequestService>();
        services.AddScoped<IWorkRequestApprovalService, WorkRequestApprovalService>();

        // Weekly
        services.AddScoped<IWeeklyReportService, WeeklyReportService>();
        services.AddScoped<IWeeklyWorkRequestService, WeeklyWorkRequestService>();

        // Misc
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IImageService, ImageService>();

        // Other
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IQueryService, QueryService>();

        // Background task queue for async operations (reports, notifications, ...)
        var queueCapacity = configuration.GetValue<int>("BackgroundQueue:Capacity", 100);
        services.AddSingleton<IBackgroundTaskQueue>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<BackgroundTaskQueue>>();
            return new BackgroundTaskQueue(queueCapacity, logger);
        });
        services.AddHostedService<QueuedHostedService>();

        // Periodic purge of expired refresh tokens
        services.AddHostedService<RefreshTokenCleanupService>();

        // Notification background processor: singleton so controllers (e.g. Dashboard)
        // can enqueue work, and a hosted service so the queue is actually drained.
        services.AddSingleton<NotificationBackgroundService>();
        services.AddHostedService(sp => sp.GetRequiredService<NotificationBackgroundService>());

        return services;
    }
}
