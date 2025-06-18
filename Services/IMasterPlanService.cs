using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services;

public interface IMasterPlanService
{
    // Basic CRUD operations
    Task<IEnumerable<MasterPlan>> GetAllMasterPlansAsync();
    Task<MasterPlan?> GetMasterPlanByIdAsync(Guid id);
    Task<IEnumerable<MasterPlan>> GetMasterPlansByProjectIdAsync(Guid projectId);
    Task<MasterPlan> CreateMasterPlanAsync(MasterPlan masterPlan);
    Task<MasterPlan?> UpdateMasterPlanAsync(Guid id, MasterPlan masterPlan);
    Task<bool> DeleteMasterPlanAsync(Guid id);
    
    // Advanced Master Plan operations
    Task<Result<MasterPlanDto>> CreateMasterPlanAsync(CreateMasterPlanRequest request, Guid createdById);
    Task<Result<MasterPlanDto>> GetMasterPlanDtoByIdAsync(Guid masterPlanId);
    Task<Result<MasterPlanDto>> GetMasterPlanByProjectIdAsync(Guid projectId);
    Task<Result<MasterPlanDto>> UpdateMasterPlanAsync(Guid masterPlanId, UpdateMasterPlanRequest request);
    Task<Result<bool>> DeleteMasterPlanDtoAsync(Guid masterPlanId);
    Task<Result<bool>> ApproveMasterPlanAsync(Guid masterPlanId, Guid approvedById, string? notes);
    Task<Result<bool>> ActivateMasterPlanAsync(Guid masterPlanId);
    
    // Phase Management
    Task<Result<ProjectPhaseDto>> AddPhaseToMasterPlanAsync(Guid masterPlanId, CreateProjectPhaseRequest request);
    Task<Result<ProjectPhaseDto>> GetPhaseByIdAsync(Guid phaseId);
    Task<Result<ProjectPhaseDto>> UpdatePhaseAsync(Guid phaseId, UpdateProjectPhaseRequest request);
    Task<Result<bool>> DeletePhaseAsync(Guid phaseId);
    Task<Result<List<ProjectPhaseDto>>> GetPhasesByMasterPlanAsync(Guid masterPlanId);
    Task<Result<bool>> UpdatePhaseProgressAsync(Guid phaseId, decimal completionPercentage, Models.PhaseStatus status);
    
    // Milestone Management
    Task<Result<ProjectMilestoneDto>> AddMilestoneToMasterPlanAsync(Guid masterPlanId, CreateProjectMilestoneRequest request);
    Task<Result<ProjectMilestoneDto>> UpdateMilestoneAsync(Guid milestoneId, UpdateProjectMilestoneRequest request);
    Task<Result<bool>> CompleteMilestoneAsync(Guid milestoneId, Guid verifiedById, string? evidence);
    Task<Result<bool>> DeleteMilestoneAsync(Guid milestoneId);
    Task<Result<List<ProjectMilestoneDto>>> GetMilestonesByMasterPlanAsync(Guid masterPlanId);
    
    // Progress Tracking
    Task<Result<ProgressReportDto>> CreateProgressReportAsync(Guid masterPlanId, CreateProgressReportRequest request, Guid createdById);
    Task<Result<List<ProgressReportDto>>> GetProgressReportsAsync(Guid masterPlanId, int pageNumber = 1, int pageSize = 10);
    Task<Result<ProgressReportDto>> GetLatestProgressReportAsync(Guid masterPlanId);
    Task<Result<ProgressSummaryDto>> GetProgressSummaryAsync(Guid masterPlanId);
    
    // Analytics and Reporting
    Task<Result<decimal>> CalculateOverallProgressAsync(Guid masterPlanId);
    Task<Result<Models.ProjectHealthStatus>> CalculateProjectHealthAsync(Guid masterPlanId);
    Task<Result<List<ProjectPhaseDto>>> GetCriticalPathAsync(Guid masterPlanId);
    Task<Result<Dictionary<string, object>>> GetProjectMetricsAsync(Guid masterPlanId);
    Task<Result<List<ProjectMilestoneDto>>> GetUpcomingMilestonesAsync(Guid masterPlanId, int days = 30);
    Task<Result<List<ProjectPhaseDto>>> GetDelayedPhasesAsync(Guid masterPlanId);
    
    // Template Management
    Task<Result<MasterPlanDto>> CreateFromTemplateAsync(Guid projectId, string templateName, Guid createdById);
    Task<Result<List<string>>> GetAvailableTemplatesAsync();
    
    // Validation and Business Rules
    Task<Result<bool>> ValidateMasterPlanAsync(Guid masterPlanId);
    Task<Result<List<string>>> GetValidationErrorsAsync(Guid masterPlanId);
}
