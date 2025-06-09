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

        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src => src.Tasks.Count))
            .ForMember(dest => dest.CompletedTaskCount, opt => opt.MapFrom(src => src.Tasks.Count(t => t.Status == dotnet_rest_api.Models.TaskStatus.Completed)))
            .ForMember(dest => dest.ProjectManager, opt => opt.MapFrom(src => src.ProjectManager));

        CreateMap<CreateProjectRequest, Project>()
            .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectManager, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore());

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

        // Note: CreatePersonnelLogRequest class not found in codebase - mapping removed

        // Material Usage mappings
        CreateMap<MaterialUsage, MaterialUsageDto>();

        // Note: CreateMaterialUsageRequest class not found in codebase - mapping removed

        // Equipment Log mappings
        CreateMap<EquipmentLog, EquipmentLogDto>();

        // Note: CreateEquipmentLogRequest class not found in codebase - mapping removed

        // Work Request mappings
        CreateMap<WorkRequest, WorkRequestDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedBy != null ? src.RequestedBy.FullName : string.Empty))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : string.Empty))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
            // Note: TaskCount and CommentCount properties not found in WorkRequestDto - mappings removed

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
            // Note: UploadedByName and ProjectName properties not found in ImageMetadataDto - mappings removed
            // ImageMetadataDto has UploadedBy (UserDto) property instead

        // Role mappings
        // Note: RoleDto class not found in codebase - mapping commented out
        // CreateMap<Role, RoleDto>();
    }
}
