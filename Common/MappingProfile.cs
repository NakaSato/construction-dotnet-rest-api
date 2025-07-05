using AutoMapper;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Common;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));

        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

        // WBS Task mappings
        CreateMap<WbsTask, WbsTaskDto>()
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.Dependencies.Select(d => d.PrerequisiteTaskId).ToList()))
            .ForMember(dest => dest.EvidenceCount, opt => opt.MapFrom(src => src.Evidence.Count));

        CreateMap<CreateWbsTaskDto, WbsTask>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
            .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
            .ForMember(dest => dest.ChildTasks, opt => opt.Ignore())
            .ForMember(dest => dest.Dependencies, opt => opt.Ignore())
            .ForMember(dest => dest.DependentTasks, opt => opt.Ignore())
            .ForMember(dest => dest.Evidence, opt => opt.Ignore());

        CreateMap<UpdateWbsTaskDto, WbsTask>()
            .ForMember(dest => dest.WbsId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
            .ForMember(dest => dest.ParentTask, opt => opt.Ignore())
            .ForMember(dest => dest.ChildTasks, opt => opt.Ignore())
            .ForMember(dest => dest.Dependencies, opt => opt.Ignore())
            .ForMember(dest => dest.DependentTasks, opt => opt.Ignore())
            .ForMember(dest => dest.Evidence, opt => opt.Ignore());

        CreateMap<WbsTask, WbsTaskHierarchyDto>()
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Level, opt => opt.Ignore());

        CreateMap<WbsTaskEvidence, WbsTaskEvidenceDto>();

        CreateMap<CreateWbsTaskEvidenceDto, WbsTaskEvidence>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.WbsTaskId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.WbsTask, opt => opt.Ignore());

        CreateMap<WbsTaskDependency, WbsTaskDependencyDto>()
            .ForMember(dest => dest.DependentTaskName, opt => opt.MapFrom(src => src.DependentTask != null ? src.DependentTask.TaskNameEN : null))
            .ForMember(dest => dest.PrerequisiteTaskName, opt => opt.MapFrom(src => src.PrerequisiteTask != null ? src.PrerequisiteTask.TaskNameEN : null));

        // Master Plan mappings
        CreateMap<MasterPlan, MasterPlanDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MasterPlanId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.PlanName))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.PlannedStartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.PlannedEndDate))
            .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.TotalEstimatedBudget))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.ApprovalNotes))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FullName))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null))
            .ForMember(dest => dest.Priority, opt => opt.Ignore()); // Priority not in MasterPlan model

        CreateMap<CreateMasterPlanRequest, MasterPlan>()
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.PlannedStartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.PlannedEndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.TotalEstimatedBudget, opt => opt.MapFrom(src => src.Budget))
            .ForMember(dest => dest.TotalPlannedDays, opt => opt.MapFrom(src => (src.EndDate - src.StartDate).Days))
            .ForMember(dest => dest.MasterPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovalNotes, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Phases, opt => opt.Ignore())
            .ForMember(dest => dest.Milestones, opt => opt.Ignore())
            .ForMember(dest => dest.ProgressReports, opt => opt.Ignore());

        // Project Phase mappings
        CreateMap<ProjectPhase, ProjectPhaseDto>()
            .ForMember(dest => dest.TasksCompleted, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == dotnet_rest_api.Models.TaskStatus.Completed)))
            .ForMember(dest => dest.TotalTasks, opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.ActualDurationDays, opt => opt.MapFrom(src => 
                src.ActualStartDate.HasValue && src.ActualEndDate.HasValue 
                    ? (decimal)(src.ActualEndDate.Value - src.ActualStartDate.Value).TotalDays 
                    : src.ActualStartDate.HasValue 
                        ? (decimal)(DateTime.UtcNow - src.ActualStartDate.Value).TotalDays 
                        : 0))
            .ForMember(dest => dest.IsOnSchedule, opt => opt.MapFrom(src => 
                src.Status == PhaseStatus.Completed 
                    ? src.ActualEndDate <= src.PlannedEndDate 
                    : DateTime.UtcNow <= src.PlannedEndDate))
            .ForMember(dest => dest.IsOnBudget, opt => opt.MapFrom(src => src.ActualCost <= src.EstimatedBudget * 1.05m)); // 5% tolerance

        CreateMap<CreateProjectPhaseRequest, ProjectPhase>()
            .ForMember(dest => dest.PhaseId, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Resources, opt => opt.Ignore())
            .ForMember(dest => dest.PlannedDurationDays, opt => opt.MapFrom(src => (src.PlannedEndDate - src.PlannedStartDate).Days));

        // Project Milestone mappings
        CreateMap<ProjectMilestone, ProjectMilestoneDto>()
            .ForMember(dest => dest.PhaseName, opt => opt.MapFrom(src => src.Phase != null ? src.Phase.PhaseName : null))
            .ForMember(dest => dest.VerifiedByName, opt => opt.MapFrom(src => src.VerifiedBy != null ? src.VerifiedBy.FullName : null))
            .ForMember(dest => dest.DaysFromPlanned, opt => opt.MapFrom(src => 
                src.ActualDate.HasValue 
                    ? (int)(src.ActualDate.Value - src.PlannedDate).TotalDays 
                    : (int)(DateTime.UtcNow - src.PlannedDate).TotalDays))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => 
                src.Status != MilestoneStatus.Completed && DateTime.UtcNow > src.PlannedDate));

        CreateMap<CreateProjectMilestoneRequest, ProjectMilestone>()
            .ForMember(dest => dest.MilestoneId, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Phase, opt => opt.Ignore())
            .ForMember(dest => dest.VerifiedBy, opt => opt.Ignore());

        // Progress Report mappings
        CreateMap<ProgressReport, ProgressReportDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.FullName));

        CreateMap<CreateProgressReportRequest, ProgressReport>()
            .ForMember(dest => dest.ProgressReportId, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedById, opt => opt.Ignore())
            .ForMember(dest => dest.MasterPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.PhaseProgressDetails, opt => opt.Ignore());

        // Phase Progress mappings
        CreateMap<PhaseProgress, PhaseProgressDto>()
            .ForMember(dest => dest.PhaseName, opt => opt.MapFrom(src => src.Phase.PhaseName));

        CreateMap<UpdatePhaseProgressRequest, PhaseProgress>()
            .ForMember(dest => dest.PhaseProgressId, opt => opt.Ignore())
            .ForMember(dest => dest.ProgressReportId, opt => opt.Ignore())
            .ForMember(dest => dest.ProgressReport, opt => opt.Ignore())
            .ForMember(dest => dest.Phase, opt => opt.Ignore());

        // Phase Resource mappings
        CreateMap<PhaseResource, PhaseResourceDto>();

        CreateMap<CreatePhaseResourceRequest, PhaseResource>()
            .ForMember(dest => dest.PhaseResourceId, opt => opt.Ignore())
            .ForMember(dest => dest.PhaseId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Phase, opt => opt.Ignore())
            .ForMember(dest => dest.TotalEstimatedCost, opt => opt.MapFrom(src => src.QuantityRequired * src.UnitCost));

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == dotnet_rest_api.Models.TaskStatus.Completed)))
            .ForMember(dest => dest.ProjectManager, opt => opt.MapFrom(src => src.ProjectManager))
            .ForMember(dest => dest.EquipmentDetails, opt => opt.MapFrom(src => new EquipmentDetailsDto
            {
                Inverter125kw = src.Inverter125kw ?? 0,
                Inverter80kw = src.Inverter80kw ?? 0,
                Inverter60kw = src.Inverter60kw ?? 0,
                Inverter40kw = src.Inverter40kw ?? 0
            }))
            .ForMember(dest => dest.LocationCoordinates, opt => opt.MapFrom(src => 
                src.Latitude.HasValue && src.Longitude.HasValue 
                    ? new LocationCoordinatesDto { Latitude = src.Latitude.Value, Longitude = src.Longitude.Value }
                    : null));

        CreateMap<CreateProjectRequest, Project>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectManager, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ProjectStatus.Planning))
            .ForMember(dest => dest.Inverter125kw, opt => opt.MapFrom(src => src.EquipmentDetails != null ? src.EquipmentDetails.Inverter125kw : (int?)null))
            .ForMember(dest => dest.Inverter80kw, opt => opt.MapFrom(src => src.EquipmentDetails != null ? src.EquipmentDetails.Inverter80kw : (int?)null))
            .ForMember(dest => dest.Inverter60kw, opt => opt.MapFrom(src => src.EquipmentDetails != null ? src.EquipmentDetails.Inverter60kw : (int?)null))
            .ForMember(dest => dest.Inverter40kw, opt => opt.MapFrom(src => src.EquipmentDetails != null ? src.EquipmentDetails.Inverter40kw : (int?)null))
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.LocationCoordinates != null ? src.LocationCoordinates.Latitude : (decimal?)null))
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.LocationCoordinates != null ? src.LocationCoordinates.Longitude : (decimal?)null));

        CreateMap<UpdateProjectRequest, Project>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectManager, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Task mappings
        CreateMap<ProjectTask, TaskDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedTechnician, opt => opt.MapFrom(src => src.AssignedTechnician))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));

        CreateMap<CreateTaskRequest, ProjectTask>()
            .ForMember(dest => dest.TaskId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTechnician, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<UpdateTaskRequest, ProjectTask>()
            .ForMember(dest => dest.TaskId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTechnician, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Daily Report mappings
        CreateMap<DailyReport, DailyReportDto>()
            .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.FullName : string.Empty))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.WeatherCondition, opt => opt.MapFrom(src => src.WeatherCondition.ToString()));

        CreateMap<CreateDailyReportRequest, DailyReport>()
            .ForMember(dest => dest.DailyReportId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.Reporter, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.WorkProgressItems, opt => opt.Ignore())
            .ForMember(dest => dest.PersonnelLogs, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialUsages, opt => opt.Ignore())
            .ForMember(dest => dest.EquipmentLogs, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<UpdateDailyReportRequest, DailyReport>()
            .ForMember(dest => dest.DailyReportId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.ReporterId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.Reporter, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.WorkProgressItems, opt => opt.Ignore())
            .ForMember(dest => dest.PersonnelLogs, opt => opt.Ignore())
            .ForMember(dest => dest.MaterialUsages, opt => opt.Ignore())
            .ForMember(dest => dest.EquipmentLogs, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Work Progress Item mappings
        CreateMap<WorkProgressItem, WorkProgressItemDto>()
            .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => src.Task != null ? src.Task.Title : string.Empty));

        CreateMap<CreateWorkProgressItemRequest, WorkProgressItem>()
            .ForMember(dest => dest.WorkProgressItemId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DailyReport, opt => opt.Ignore())
            .ForMember(dest => dest.Task, opt => opt.Ignore());

        // Personnel Log mappings
        CreateMap<PersonnelLog, PersonnelLogDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // Material Usage mappings
        CreateMap<MaterialUsage, MaterialUsageDto>();

        // Equipment Log mappings
        CreateMap<EquipmentLog, EquipmentLogDto>();

        // Work Request mappings
        CreateMap<WorkRequest, WorkRequestDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy != null ? src.RequestedBy.FullName : string.Empty))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : string.Empty))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));

        CreateMap<CreateWorkRequestRequest, WorkRequest>()
            .ForMember(dest => dest.WorkRequestId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<UpdateWorkRequestRequest, WorkRequest>()
            .ForMember(dest => dest.WorkRequestId, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedById, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        // Work Request Task mappings
        CreateMap<WorkRequestTask, WorkRequestTaskDto>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.WorkRequestId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedToUser));

        CreateMap<CreateWorkRequestTaskRequest, WorkRequestTask>()
            .ForMember(dest => dest.WorkRequestTaskId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkRequestId, opt => opt.MapFrom(src => src.RequestId))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.WorkRequest, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore());

        // Work Request Comment mappings
        CreateMap<WorkRequestComment, WorkRequestCommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.FullName : string.Empty));

        CreateMap<CreateWorkRequestCommentRequest, WorkRequestComment>()
            .ForMember(dest => dest.WorkRequestCommentId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.WorkRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Author, opt => opt.Ignore());

        // Image Metadata mappings
        CreateMap<ImageMetadata, ImageMetadataDto>()
            .ForMember(dest => dest.UploadedBy, opt => opt.MapFrom(src => src.UploadedByUser));

        // Weekly Work Request mappings
        CreateMap<WeeklyWorkRequest, WeeklyWorkRequestDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RequestedBy, opt => opt.MapFrom(src => src.RequestedByUser))
            .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.ApprovedByUser))
            .ForMember(dest => dest.KeyTasks, opt => opt.MapFrom(src => ParseJsonArray(src.KeyTasks)))
            .ForMember(dest => dest.ResourceForecast, opt => opt.MapFrom(src => new WeeklyResourceForecastDto
            {
                Personnel = src.PersonnelForecast,
                MajorEquipment = src.MajorEquipment,
                CriticalMaterials = src.CriticalMaterials
            }));

        CreateMap<CreateWeeklyWorkRequestDto, WeeklyWorkRequest>()
            .ForMember(dest => dest.WeeklyRequestId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WeeklyRequestStatus.Draft))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.KeyTasks, opt => opt.MapFrom(src => SerializeJsonArray(src.KeyTasks)))
            .ForMember(dest => dest.PersonnelForecast, opt => opt.MapFrom(src => src.ResourceForecast != null ? src.ResourceForecast.Personnel : null))
            .ForMember(dest => dest.MajorEquipment, opt => opt.MapFrom(src => src.ResourceForecast != null ? src.ResourceForecast.MajorEquipment : null))
            .ForMember(dest => dest.CriticalMaterials, opt => opt.MapFrom(src => src.ResourceForecast != null ? src.ResourceForecast.CriticalMaterials : null))
            .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => "Normal"))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Standard"));

        // Weekly Report mappings
        CreateMap<WeeklyReport, WeeklyReportDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SubmittedBy, opt => opt.MapFrom(src => src.SubmittedByUser))
            .ForMember(dest => dest.ApprovedBy, opt => opt.MapFrom(src => src.ApprovedByUser))
            .ForMember(dest => dest.MajorAccomplishments, opt => opt.MapFrom(src => ParseJsonArray(src.MajorAccomplishments)))
            .ForMember(dest => dest.MajorIssues, opt => opt.MapFrom(src => ParseWeeklyIssues(src.MajorIssues)))
            .ForMember(dest => dest.AggregatedMetrics, opt => opt.MapFrom(src => new WeeklyAggregatedMetricsDto
            {
                TotalManHours = src.TotalManHours,
                PanelsInstalled = src.PanelsInstalled,
                SafetyIncidents = src.SafetyIncidents,
                DelaysReported = src.DelaysReported
            }));

        CreateMap<CreateWeeklyReportDto, WeeklyReport>()
            .ForMember(dest => dest.WeeklyReportId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WeeklyReportStatus.Draft))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.MajorAccomplishments, opt => opt.MapFrom(src => SerializeJsonArray(src.MajorAccomplishments)))
            .ForMember(dest => dest.MajorIssues, opt => opt.MapFrom(src => SerializeWeeklyIssues(src.MajorIssues)))
            .ForMember(dest => dest.TotalManHours, opt => opt.MapFrom(src => src.AggregatedMetrics != null ? src.AggregatedMetrics.TotalManHours : 0))
            .ForMember(dest => dest.PanelsInstalled, opt => opt.MapFrom(src => src.AggregatedMetrics != null ? src.AggregatedMetrics.PanelsInstalled : 0))
            .ForMember(dest => dest.SafetyIncidents, opt => opt.MapFrom(src => src.AggregatedMetrics != null ? src.AggregatedMetrics.SafetyIncidents : 0))
            .ForMember(dest => dest.DelaysReported, opt => opt.MapFrom(src => src.AggregatedMetrics != null ? src.AggregatedMetrics.DelaysReported : 0))
            .ForMember(dest => dest.CompletionPercentage, opt => opt.MapFrom(src => 0));
    }

    // Helper methods for JSON parsing
    private static List<string> ParseJsonArray(string jsonString)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonString ?? "[]") ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static List<WeeklyIssueDto> ParseWeeklyIssues(string jsonString)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<WeeklyIssueDto>>(jsonString ?? "[]") ?? new List<WeeklyIssueDto>();
        }
        catch
        {
            return new List<WeeklyIssueDto>();
        }
    }

    private static string SerializeJsonArray(List<string>? list)
    {
        return System.Text.Json.JsonSerializer.Serialize(list ?? new List<string>());
    }

    private static string SerializeWeeklyIssues(List<WeeklyIssueDto>? list)
    {
        return System.Text.Json.JsonSerializer.Serialize(list ?? new List<WeeklyIssueDto>());
    }
}
