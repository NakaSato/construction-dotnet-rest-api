using dotnet_rest_api.Services;
// using dotnet_rest_api.Services.Commands;
using dotnet_rest_api.Services.Handlers;
using dotnet_rest_api.Services.Interfaces;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Extensions;

/// <summary>
/// Extension methods for registering refactored services with dependency injection
/// This replaces the original large service registrations with focused ones
/// </summary>
public static class RefactoredServiceExtensions
{
    /// <summary>
    /// Registers all refactored master plan services
    /// Replaces the single large MasterPlanService with focused services
    /// </summary>
    public static IServiceCollection AddRefactoredMasterPlanServices(this IServiceCollection services)
    {
        // Register focused services
        // TODO: Implement these services
        // services.AddScoped<IMasterPlanCrudService, MasterPlanCrudService>();
        // services.AddScoped<IMasterPlanAnalyticsService, MasterPlanAnalyticsService>();
        // services.AddScoped<IPhaseManagementService, PhaseManagementService>();
        // services.AddScoped<IMilestoneService, MilestoneService>();
        // services.AddScoped<IMasterPlanReportingService, MasterPlanReportingService>();

        // Register the orchestrator service that implements the original interface
        // services.AddScoped<IMasterPlanService, MasterPlanOrchestratorService>();

        return services;
    }

    /// <summary>
    /// Registers command and query handlers for CQRS pattern
    /// Reduces controller complexity by moving business logic to handlers
    /// </summary>
    public static IServiceCollection AddMasterPlanCommandHandlers(this IServiceCollection services)
    {
        // Command handlers - commented out due to missing Command classes
        /*
        services.AddScoped<ICommandHandler<CreateMasterPlanCommand, MasterPlanDto>, CreateMasterPlanHandler>();
        services.AddScoped<ICommandHandler<UpdateMasterPlanCommand, MasterPlanDto>, UpdateMasterPlanHandler>();
        services.AddScoped<ICommandHandler<ApproveMasterPlanCommand, bool>, ApproveMasterPlanHandler>();
        services.AddScoped<ICommandHandler<ActivateMasterPlanCommand, bool>, ActivateMasterPlanHandler>();
        services.AddScoped<ICommandHandler<AddPhaseCommand, ProjectPhaseDto>, AddPhaseHandler>();
        services.AddScoped<ICommandHandler<UpdatePhaseProgressCommand, bool>, UpdatePhaseProgressHandler>();
        services.AddScoped<ICommandHandler<AddMilestoneCommand, ProjectMilestoneDto>, AddMilestoneHandler>();
        services.AddScoped<ICommandHandler<CompleteMilestoneCommand, bool>, CompleteMilestoneHandler>();
        services.AddScoped<ICommandHandler<CreateProgressReportCommand, ProgressReportDto>, CreateProgressReportHandler>();
        */

        // Advanced command handlers - implement these when handlers are created:
        // services.AddScoped<ICommandHandler<DeleteMasterPlanCommand, bool>, DeleteMasterPlanHandler>();
        // services.AddScoped<ICommandHandler<CreateTaskDependencyCommand, TaskDependencyDto>, CreateTaskDependencyHandler>();
        // services.AddScoped<ICommandHandler<ValidateConstraintsCommand, ConstraintValidationResultDto>, ValidateConstraintsHandler>();
        // services.AddScoped<ICommandHandler<TriggerWorkflowCommand, WorkflowExecutionResultDto>, TriggerWorkflowHandler>();
        // services.AddScoped<ICommandHandler<ExportProjectReportCommand, ProjectExportDto>, ExportProjectReportHandler>();
        // services.AddScoped<ICommandHandler<CreateStakeholderCommunicationCommand, StakeholderCommunicationDto>, CreateStakeholderCommunicationHandler>();

        return services;
    }

    /// <summary>
    /// Registers query handlers for read operations
    /// Separates read and write operations following CQRS pattern
    /// </summary>
    public static IServiceCollection AddMasterPlanQueryHandlers(this IServiceCollection services)
    {
        // Query handlers - commented out due to missing Query classes
        /*
        services.AddScoped<IQueryHandler<GetAllMasterPlansQuery, List<MasterPlanDto>>, GetAllMasterPlansQueryHandler>();
        services.AddScoped<IQueryHandler<GetMasterPlanQuery, MasterPlanDto>, GetMasterPlanQueryHandler>();
        services.AddScoped<IQueryHandler<GetMasterPlanByProjectQuery, MasterPlanDto>, GetMasterPlanByProjectQueryHandler>();
        services.AddScoped<IQueryHandler<GetProgressSummaryQuery, ProgressSummaryDto>, GetProgressSummaryQueryHandler>();
        services.AddScoped<IQueryHandler<GetOverallProgressQuery, decimal>, GetOverallProgressQueryHandler>();
        services.AddScoped<IQueryHandler<GetPhasesQuery, List<ProjectPhaseDto>>, GetPhasesQueryHandler>();
        services.AddScoped<IQueryHandler<GetMilestonesQuery, List<ProjectMilestoneDto>>, GetMilestonesQueryHandler>();
        services.AddScoped<IQueryHandler<GetUpcomingMilestonesQuery, List<ProjectMilestoneDto>>, GetUpcomingMilestonesQueryHandler>();
        services.AddScoped<IQueryHandler<GetProgressReportsQuery, List<ProgressReportDto>>, GetProgressReportsQueryHandler>();
        */

        // Advanced analytics query handlers - implement these when handlers are created:
        // services.AddScoped<IQueryHandler<GetCriticalPathQuery, CriticalPathAnalysisDto>, GetCriticalPathQueryHandler>();
        // services.AddScoped<IQueryHandler<GetEarnedValueAnalysisQuery, EarnedValueAnalysisDto>, GetEarnedValueAnalysisQueryHandler>();
        // services.AddScoped<IQueryHandler<GetResourceUtilizationQuery, ResourceUtilizationReportDto>, GetResourceUtilizationQueryHandler>();
        // services.AddScoped<IQueryHandler<GetExecutiveDashboardQuery, ExecutiveDashboardDto>, GetExecutiveDashboardQueryHandler>();
        // services.AddScoped<IQueryHandler<GetGanttChartDataQuery, GanttChartDataDto>, GetGanttChartDataQueryHandler>();
        // services.AddScoped<IQueryHandler<GetWeeklyViewQuery, WeeklyViewDto>, GetWeeklyViewQueryHandler>();
        // services.AddScoped<IQueryHandler<GetProjectFromMasterPlanQuery, ProjectDto>, GetProjectFromMasterPlanQueryHandler>();

        return services;
    }

    /// <summary>
    /// Registers all refactored services in one call
    /// Convenience method for easy registration
    /// </summary>
    public static IServiceCollection AddAllRefactoredServices(this IServiceCollection services)
    {
        return services
            .AddRefactoredMasterPlanServices()
            .AddMasterPlanCommandHandlers()
            .AddMasterPlanQueryHandlers();
    }

    /// <summary>
    /// Registers validation services for better error handling
    /// Centralizes validation logic across the application
    /// </summary>
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Add FluentValidation if available
        // services.AddFluentValidationAutoValidation();
        // services.AddFluentValidationClientsideAdapters();
        // services.AddValidatorsFromAssemblyContaining<CreateMasterPlanRequestValidator>();

        return services;
    }

    /// <summary>
    /// Registers business rule services
    /// Encapsulates complex business logic outside of controllers and basic services
    /// </summary>
    public static IServiceCollection AddBusinessRuleServices(this IServiceCollection services)
    {
        // Business rule services - implement these when needed:
        // services.AddScoped<IProjectHealthCalculator, ProjectHealthCalculator>();
        // services.AddScoped<IProgressCalculator, ProgressCalculator>();
        // services.AddScoped<ICriticalPathAnalyzer, CriticalPathAnalyzer>();
        // services.AddScoped<IProjectMetricsCalculator, ProjectMetricsCalculator>();

        return services;
    }
}
