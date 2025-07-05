-- Solar Projects API Database Schema
-- Generated from .NET Entity Framework Models
-- PostgreSQL Database Schema

-- ============================================
-- CORE TABLES
-- ============================================

-- Roles table
CREATE TABLE "Roles" (
    "RoleId" SERIAL PRIMARY KEY,
    "RoleName" VARCHAR(50) NOT NULL
);

-- Users table
CREATE TABLE "Users" (
    "UserId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Username" VARCHAR(100) NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "RoleId" INTEGER NOT NULL,
    "FullName" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Users_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("RoleId") ON DELETE RESTRICT
);

-- Projects table (Enhanced for Real-Time Updates)
CREATE TABLE "Projects" (
    "ProjectId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectName" VARCHAR(255) NOT NULL,
    "Address" VARCHAR(500) NOT NULL,
    "ClientInfo" VARCHAR(1000) NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Planning, 1=InProgress, 2=Completed, 3=OnHold, 4=Cancelled
    "StartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EstimatedEndDate" TIMESTAMP WITH TIME ZONE,
    "ActualEndDate" TIMESTAMP WITH TIME ZONE,
    "ProjectManagerId" UUID,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Solar Project Specific Fields
    "Team" VARCHAR(50),
    "ConnectionType" VARCHAR(20),
    "ConnectionNotes" VARCHAR(1000),
    "TotalCapacityKw" DECIMAL(10,2),
    "PvModuleCount" INTEGER,
    
    -- Equipment Details
    "Inverter125kw" INTEGER DEFAULT 0,
    "Inverter80kw" INTEGER DEFAULT 0,
    "Inverter60kw" INTEGER DEFAULT 0,
    "Inverter40kw" INTEGER DEFAULT 0,
    
    -- Business Values
    "FtsValue" INTEGER,
    "RevenueValue" INTEGER,
    "PqmValue" INTEGER,
    
    -- Enhanced Location Coordinates (Real-Time Support)
    "Latitude" DECIMAL(10,7),
    "Longitude" DECIMAL(10,7),
    "LocationAccuracy" DECIMAL(8,2), -- GPS accuracy in meters
    "LocationUpdatedAt" TIMESTAMP WITH TIME ZONE,
    "LocationUpdatedBy" VARCHAR(255),
    
    -- Real-Time Tracking Fields
    "CompletionPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0 CHECK ("CompletionPercentage" BETWEEN 0 AND 100),
    "LastStatusChange" TIMESTAMP WITH TIME ZONE,
    "LastLocationUpdate" TIMESTAMP WITH TIME ZONE,
    "LastActivityAt" TIMESTAMP WITH TIME ZONE,
    "RealTimeEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Enhanced Metadata
    "Description" TEXT,
    "Priority" VARCHAR(50),
    "Budget" DECIMAL(18,2),
    "CreatedById" UUID,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "Version" INTEGER NOT NULL DEFAULT 1, -- For optimistic concurrency
    
    -- Geographic Intelligence
    "Province" VARCHAR(100),
    "Region" VARCHAR(50), -- northern, western, central, unknown
    "FacilityType" VARCHAR(100), -- water_treatment, pumping_station, etc.
    "WaterAuthority" VARCHAR(255), -- Provincial/Metropolitan Waterworks Authority
    
    -- Manager relationship made nullable for development flexibility
    CONSTRAINT "FK_Projects_Users_ProjectManagerId" FOREIGN KEY ("ProjectManagerId") REFERENCES "Users" ("UserId") ON DELETE SET NULL,
    CONSTRAINT "FK_Projects_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- MASTER PLANNING TABLES
-- ============================================

-- Master Plans table
CREATE TABLE "MasterPlans" (
    "MasterPlanId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "PlanName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000),
    "PlannedStartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "PlannedEndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "TotalPlannedDays" INTEGER NOT NULL,
    "TotalEstimatedBudget" DECIMAL(18,2) NOT NULL,
    "Version" INTEGER NOT NULL DEFAULT 1,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Draft, 1=UnderReview, 2=Approved, 3=Active, 4=Completed, 5=Cancelled
    "ApprovedAt" TIMESTAMP WITH TIME ZONE,
    "ApprovedById" UUID,
    "ApprovalNotes" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "CreatedById" UUID NOT NULL,
    
    CONSTRAINT "FK_MasterPlans_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_MasterPlans_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_MasterPlans_Users_ApprovedById" FOREIGN KEY ("ApprovedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

-- Project Phases table
CREATE TABLE "ProjectPhases" (
    "PhaseId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MasterPlanId" UUID NOT NULL,
    "PhaseName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000),
    "PhaseOrder" INTEGER NOT NULL,
    "PlannedStartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "PlannedEndDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ActualStartDate" TIMESTAMP WITH TIME ZONE,
    "ActualEndDate" TIMESTAMP WITH TIME ZONE,
    "PlannedDurationDays" INTEGER NOT NULL,
    "EstimatedBudget" DECIMAL(18,2) NOT NULL,
    "ActualCost" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "WeightPercentage" DECIMAL(5,2) NOT NULL,
    "CompletionPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=NotStarted, 1=InProgress, 2=Completed, 3=OnHold, 4=Delayed, 5=Cancelled
    "Prerequisites" VARCHAR(500),
    "RiskLevel" INTEGER NOT NULL DEFAULT 0, -- 0=Low, 1=Medium, 2=High, 3=Critical
    "Notes" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_ProjectPhases_MasterPlans_MasterPlanId" FOREIGN KEY ("MasterPlanId") REFERENCES "MasterPlans" ("MasterPlanId") ON DELETE CASCADE
);

-- Project Milestones table
CREATE TABLE "ProjectMilestones" (
    "MilestoneId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MasterPlanId" UUID NOT NULL,
    "PhaseId" UUID,
    "MilestoneName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000),
    "PlannedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ActualDate" TIMESTAMP WITH TIME ZONE,
    "Type" INTEGER NOT NULL, -- 1=PhaseCompletion, 2=PermitApproval, etc.
    "Importance" INTEGER NOT NULL DEFAULT 1, -- 0=Low, 1=Medium, 2=High, 3=Critical
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Pending, 1=InProgress, 2=Completed, 3=Delayed, 4=AtRisk, 5=Cancelled
    "WeightPercentage" DECIMAL(5,2) NOT NULL,
    "CompletionCriteria" VARCHAR(2000),
    "CompletionEvidence" VARCHAR(1000),
    "VerifiedById" UUID,
    "VerifiedAt" TIMESTAMP WITH TIME ZONE,
    "Notes" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "TargetDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "CompletedDate" TIMESTAMP WITH TIME ZONE,
    "CompletedById" UUID,
    "Priority" INTEGER NOT NULL DEFAULT 1,
    
    CONSTRAINT "FK_ProjectMilestones_MasterPlans_MasterPlanId" FOREIGN KEY ("MasterPlanId") REFERENCES "MasterPlans" ("MasterPlanId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectMilestones_ProjectPhases_PhaseId" FOREIGN KEY ("PhaseId") REFERENCES "ProjectPhases" ("PhaseId") ON DELETE SET NULL,
    CONSTRAINT "FK_ProjectMilestones_Users_VerifiedById" FOREIGN KEY ("VerifiedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Phase Resources table
CREATE TABLE "PhaseResources" (
    "PhaseResourceId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PhaseId" UUID NOT NULL,
    "ResourceType" INTEGER NOT NULL, -- 1=Personnel, 2=Equipment, 3=Material, 4=Service, 5=Permit, 99=Other
    "ResourceName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(500),
    "QuantityRequired" DECIMAL(18,2) NOT NULL,
    "Unit" VARCHAR(50) NOT NULL,
    "UnitCost" DECIMAL(18,2) NOT NULL,
    "TotalEstimatedCost" DECIMAL(18,2) NOT NULL,
    "ActualQuantityUsed" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ActualCost" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "RequiredDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "DurationDays" INTEGER NOT NULL,
    "AllocationStatus" INTEGER NOT NULL DEFAULT 0, -- 0=Planned, 1=Requested, etc.
    "Supplier" VARCHAR(255),
    "Notes" VARCHAR(1000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_PhaseResources_ProjectPhases_PhaseId" FOREIGN KEY ("PhaseId") REFERENCES "ProjectPhases" ("PhaseId") ON DELETE CASCADE
);

-- ============================================
-- TASK MANAGEMENT TABLES
-- ============================================

-- Project Tasks table
CREATE TABLE "ProjectTasks" (
    "TaskId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "PhaseId" UUID,
    "Title" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(2000) NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=NotStarted, 1=InProgress, 2=Completed, 3=OnHold, 4=Cancelled
    "Priority" INTEGER NOT NULL DEFAULT 1, -- 0=Low, 1=Medium, 2=High, 3=Critical
    "DueDate" TIMESTAMP WITH TIME ZONE,
    "AssignedTechnicianId" UUID,
    "CompletionDate" TIMESTAMP WITH TIME ZONE,
    "EstimatedHours" DECIMAL(18,2) NOT NULL,
    "ActualHours" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CompletionPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "WeightInPhase" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "Dependencies" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_ProjectTasks_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectTasks_ProjectPhases_PhaseId" FOREIGN KEY ("PhaseId") REFERENCES "ProjectPhases" ("PhaseId") ON DELETE SET NULL,
    CONSTRAINT "FK_ProjectTasks_Users_AssignedTechnicianId" FOREIGN KEY ("AssignedTechnicianId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Tasks table (additional task model)
CREATE TABLE "Tasks" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "Priority" INTEGER NOT NULL DEFAULT 1,
    "StartDate" TIMESTAMP WITH TIME ZONE,
    "DueDate" TIMESTAMP WITH TIME ZONE,
    "CompletedDate" TIMESTAMP WITH TIME ZONE,
    "EstimatedHours" DECIMAL(18,2) NOT NULL,
    "ActualHours" DECIMAL(18,2) NOT NULL,
    "Progress" INTEGER NOT NULL DEFAULT 0, -- 0-100
    "ProjectId" UUID NOT NULL,
    "AssignedToId" UUID,
    "PhaseId" UUID,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById" UUID NOT NULL,
    "UpdatedById" UUID,
    
    CONSTRAINT "FK_Tasks_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_Tasks_Users_AssignedToId" FOREIGN KEY ("AssignedToId") REFERENCES "Users" ("UserId") ON DELETE SET NULL,
    CONSTRAINT "FK_Tasks_MasterPlans_PhaseId" FOREIGN KEY ("PhaseId") REFERENCES "MasterPlans" ("MasterPlanId") ON DELETE SET NULL,
    CONSTRAINT "FK_Tasks_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_Tasks_Users_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Task Progress Reports table
CREATE TABLE "TaskProgressReports" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT NOT NULL,
    "Progress" INTEGER NOT NULL DEFAULT 0, -- 0-100
    "HoursWorked" DECIMAL(18,2) NOT NULL,
    "Issues" TEXT NOT NULL,
    "NextSteps" TEXT NOT NULL,
    "TaskId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById" UUID NOT NULL,
    "UpdatedById" UUID,
    
    CONSTRAINT "FK_TaskProgressReports_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TaskProgressReports_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_TaskProgressReports_Users_UpdatedById" FOREIGN KEY ("UpdatedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- REPORTING TABLES
-- ============================================

-- Daily Reports table
CREATE TABLE "DailyReports" (
    "DailyReportId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "ReportDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Draft, 1=Submitted, 2=Approved, 3=Rejected, 4=RevisionRequested
    "ReporterId" UUID NOT NULL,
    "SubmittedByUserId" UUID,
    "SubmittedAt" TIMESTAMP WITH TIME ZONE,
    "ApprovedAt" TIMESTAMP WITH TIME ZONE,
    "RejectionReason" VARCHAR(2000),
    "GeneralNotes" VARCHAR(2000),
    "WeatherCondition" INTEGER, -- 0=Sunny, 1=PartlyCloudy, etc.
    "TemperatureHigh" DOUBLE PRECISION CHECK ("TemperatureHigh" BETWEEN -40 AND 50),
    "TemperatureLow" DOUBLE PRECISION CHECK ("TemperatureLow" BETWEEN -40 AND 50),
    "Humidity" INTEGER CHECK ("Humidity" BETWEEN 0 AND 100),
    "WindSpeed" DOUBLE PRECISION CHECK ("WindSpeed" BETWEEN 0 AND 200),
    "TotalWorkHours" INTEGER NOT NULL,
    "PersonnelOnSite" INTEGER NOT NULL,
    "SafetyIncidents" VARCHAR(1000),
    "QualityIssues" VARCHAR(1000),
    "Summary" VARCHAR(2000),
    "WorkAccomplished" VARCHAR(2000),
    "WorkPlanned" VARCHAR(2000),
    "Issues" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_DailyReports_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_DailyReports_Users_ReporterId" FOREIGN KEY ("ReporterId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_DailyReports_Users_SubmittedByUserId" FOREIGN KEY ("SubmittedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Daily Report Attachments table
CREATE TABLE "DailyReportAttachments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "FileName" VARCHAR(255) NOT NULL,
    "FilePath" VARCHAR(500) NOT NULL,
    "FileType" VARCHAR(100) NOT NULL,
    "FileSize" BIGINT NOT NULL,
    "DailyReportId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById" UUID NOT NULL,
    
    CONSTRAINT "FK_DailyReportAttachments_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE CASCADE,
    CONSTRAINT "FK_DailyReportAttachments_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

-- Progress Reports table
CREATE TABLE "ProgressReports" (
    "ProgressReportId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "MasterPlanId" UUID NOT NULL,
    "ProjectId" UUID NOT NULL,
    "ReportDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "OverallCompletionPercentage" DECIMAL(5,2) NOT NULL,
    "SchedulePerformanceIndex" DECIMAL(5,4) NOT NULL,
    "CostPerformanceIndex" DECIMAL(5,4) NOT NULL,
    "ActualCostToDate" DECIMAL(18,2) NOT NULL,
    "EstimatedCostAtCompletion" DECIMAL(18,2) NOT NULL,
    "BudgetVariance" DECIMAL(18,2) NOT NULL,
    "ScheduleVarianceDays" INTEGER NOT NULL,
    "ProjectedCompletionDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "ActiveIssuesCount" INTEGER NOT NULL,
    "CompletedMilestonesCount" INTEGER NOT NULL,
    "TotalMilestonesCount" INTEGER NOT NULL,
    "HealthStatus" INTEGER NOT NULL, -- Project health status enum
    "KeyAccomplishments" VARCHAR(4000),
    "CurrentChallenges" VARCHAR(4000),
    "UpcomingActivities" VARCHAR(4000),
    "RiskSummary" VARCHAR(2000),
    "QualityNotes" VARCHAR(2000),
    "WeatherImpact" VARCHAR(1000),
    "ResourceNotes" VARCHAR(2000),
    "ExecutiveSummary" VARCHAR(3000),
    "ReportTitle" VARCHAR(255),
    "ReportContent" VARCHAR(8000),
    "CompletionPercentage" DECIMAL(5,2) NOT NULL,
    "ChallengesFaced" VARCHAR(4000),
    "NextSteps" VARCHAR(4000),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CreatedById" UUID NOT NULL,
    
    CONSTRAINT "FK_ProgressReports_MasterPlans_MasterPlanId" FOREIGN KEY ("MasterPlanId") REFERENCES "MasterPlans" ("MasterPlanId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProgressReports_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProgressReports_Users_CreatedById" FOREIGN KEY ("CreatedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

-- Phase Progress table
CREATE TABLE "PhaseProgresses" (
    "PhaseProgressId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProgressReportId" UUID NOT NULL,
    "PhaseId" UUID NOT NULL,
    "CompletionPercentage" DECIMAL(5,2) NOT NULL,
    "ActualStartDate" TIMESTAMP WITH TIME ZONE,
    "ActualEndDate" TIMESTAMP WITH TIME ZONE,
    "ActualCostToDate" DECIMAL(18,2) NOT NULL,
    "EstimatedCostAtCompletion" DECIMAL(18,2) NOT NULL,
    "ScheduleVarianceDays" INTEGER NOT NULL,
    "BudgetVariance" DECIMAL(18,2) NOT NULL,
    "Status" INTEGER NOT NULL,
    "KeyActivities" VARCHAR(2000),
    "Issues" VARCHAR(2000),
    "RiskFactors" VARCHAR(1000),
    "Notes" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_PhaseProgresses_ProgressReports_ProgressReportId" FOREIGN KEY ("ProgressReportId") REFERENCES "ProgressReports" ("ProgressReportId") ON DELETE CASCADE,
    CONSTRAINT "FK_PhaseProgresses_ProjectPhases_PhaseId" FOREIGN KEY ("PhaseId") REFERENCES "ProjectPhases" ("PhaseId") ON DELETE CASCADE
);

-- Weekly Reports table
CREATE TABLE "WeeklyReports" (
    "WeeklyReportId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "WeekStartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Draft, 1=Submitted, 2=Approved
    "SummaryOfProgress" VARCHAR(2000) NOT NULL,
    "TotalManHours" INTEGER NOT NULL,
    "PanelsInstalled" INTEGER NOT NULL,
    "SafetyIncidents" INTEGER NOT NULL,
    "DelaysReported" INTEGER NOT NULL,
    "MajorAccomplishments" JSONB NOT NULL DEFAULT '[]',
    "MajorIssues" JSONB NOT NULL DEFAULT '[]',
    "Lookahead" VARCHAR(2000),
    "CompletionPercentage" INTEGER NOT NULL DEFAULT 0 CHECK ("CompletionPercentage" BETWEEN 0 AND 100),
    "SubmittedById" UUID NOT NULL,
    "ApprovedById" UUID,
    "ApprovedAt" TIMESTAMP WITH TIME ZONE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_WeeklyReports_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_WeeklyReports_Users_SubmittedById" FOREIGN KEY ("SubmittedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_WeeklyReports_Users_ApprovedById" FOREIGN KEY ("ApprovedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- WORK REQUEST TABLES
-- ============================================

-- Work Requests table
CREATE TABLE "WorkRequests" (
    "WorkRequestId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(2000) NOT NULL,
    "RequestedById" UUID NOT NULL,
    "AssignedToId" UUID,
    "Status" INTEGER NOT NULL DEFAULT 0, -- WorkRequestStatus enum
    "Priority" INTEGER NOT NULL DEFAULT 1, -- 0=Low, 1=Medium, 2=High, 3=Critical
    "Type" INTEGER NOT NULL DEFAULT 5, -- WorkRequestType enum
    "EstimatedHours" DECIMAL(8,2),
    "ActualHours" DECIMAL(8,2),
    "EstimatedCost" DECIMAL(18,2),
    "ActualCost" DECIMAL(18,2),
    "RequestedDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "RequiredByDate" TIMESTAMP WITH TIME ZONE,
    "StartedAt" TIMESTAMP WITH TIME ZONE,
    "CompletedAt" TIMESTAMP WITH TIME ZONE,
    "ApprovedAt" TIMESTAMP WITH TIME ZONE,
    "ApprovedById" UUID,
    "RejectedAt" TIMESTAMP WITH TIME ZONE,
    "RejectedById" UUID,
    "RejectionReason" VARCHAR(1000),
    "CompletionNotes" VARCHAR(2000),
    "InternalNotes" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_WorkRequests_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkRequests_Users_RequestedById" FOREIGN KEY ("RequestedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_WorkRequests_Users_AssignedToId" FOREIGN KEY ("AssignedToId") REFERENCES "Users" ("UserId") ON DELETE SET NULL,
    CONSTRAINT "FK_WorkRequests_Users_ApprovedById" FOREIGN KEY ("ApprovedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL,
    CONSTRAINT "FK_WorkRequests_Users_RejectedById" FOREIGN KEY ("RejectedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Weekly Work Requests table
CREATE TABLE "WeeklyWorkRequests" (
    "WeeklyRequestId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "WeekStartDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Draft, 1=Submitted, 2=Approved, 3=InProgress, 4=Completed
    "OverallGoals" VARCHAR(2000) NOT NULL,
    "KeyTasks" JSONB NOT NULL DEFAULT '[]',
    "PersonnelForecast" VARCHAR(1000),
    "MajorEquipment" VARCHAR(1000),
    "CriticalMaterials" VARCHAR(1000),
    "EstimatedHours" INTEGER NOT NULL DEFAULT 0,
    "Priority" VARCHAR(50),
    "Type" VARCHAR(100),
    "RequestedById" UUID NOT NULL,
    "ApprovedById" UUID,
    "ApprovedAt" TIMESTAMP WITH TIME ZONE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_WeeklyWorkRequests_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_WeeklyWorkRequests_Users_RequestedById" FOREIGN KEY ("RequestedById") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_WeeklyWorkRequests_Users_ApprovedById" FOREIGN KEY ("ApprovedById") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Work Request Tasks table
CREATE TABLE "WorkRequestTasks" (
    "WorkRequestTaskId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WorkRequestId" UUID NOT NULL,
    "TaskName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000),
    "EstimatedHours" DECIMAL(8,2),
    "ActualHours" DECIMAL(8,2),
    "Status" INTEGER NOT NULL DEFAULT 0,
    "AssignedToId" UUID,
    "StartedAt" TIMESTAMP WITH TIME ZONE,
    "CompletedAt" TIMESTAMP WITH TIME ZONE,
    "Notes" VARCHAR(1000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_WorkRequestTasks_WorkRequests_WorkRequestId" FOREIGN KEY ("WorkRequestId") REFERENCES "WorkRequests" ("WorkRequestId") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkRequestTasks_Users_AssignedToId" FOREIGN KEY ("AssignedToId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Work Request Comments table
CREATE TABLE "WorkRequestComments" (
    "CommentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WorkRequestId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "CommentText" VARCHAR(2000) NOT NULL,
    "IsInternal" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_WorkRequestComments_WorkRequests_WorkRequestId" FOREIGN KEY ("WorkRequestId") REFERENCES "WorkRequests" ("WorkRequestId") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkRequestComments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

-- Work Request Approvals table
CREATE TABLE "WorkRequestApprovals" (
    "ApprovalId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WorkRequestId" UUID NOT NULL,
    "ApproverId" UUID NOT NULL,
    "Action" INTEGER NOT NULL, -- ApprovalAction enum
    "Level" INTEGER NOT NULL, -- 0=None, 1=Manager, 2=Admin
    "PreviousStatus" INTEGER NOT NULL,
    "NewStatus" INTEGER NOT NULL,
    "Comments" VARCHAR(1000),
    "RejectionReason" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ProcessedAt" TIMESTAMP WITH TIME ZONE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "EscalatedFromId" UUID,
    "EscalationDate" TIMESTAMP WITH TIME ZONE,
    "EscalationReason" VARCHAR(500),
    
    CONSTRAINT "FK_WorkRequestApprovals_WorkRequests_WorkRequestId" FOREIGN KEY ("WorkRequestId") REFERENCES "WorkRequests" ("WorkRequestId") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkRequestApprovals_Users_ApproverId" FOREIGN KEY ("ApproverId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_WorkRequestApprovals_WorkRequestApprovals_EscalatedFromId" FOREIGN KEY ("EscalatedFromId") REFERENCES "WorkRequestApprovals" ("ApprovalId") ON DELETE SET NULL
);

-- Work Request Notifications table
CREATE TABLE "WorkRequestNotifications" (
    "NotificationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "WorkRequestId" UUID NOT NULL,
    "RecipientId" UUID NOT NULL,
    "SenderId" UUID,
    "Type" INTEGER NOT NULL, -- NotificationType enum
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Pending, 1=Sent, 2=Failed, 3=Delivered, 4=Read
    "Subject" VARCHAR(200) NOT NULL,
    "Message" VARCHAR(2000) NOT NULL,
    "EmailTo" VARCHAR(500),
    "EmailCc" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "SentAt" TIMESTAMP WITH TIME ZONE,
    "DeliveredAt" TIMESTAMP WITH TIME ZONE,
    "ReadAt" TIMESTAMP WITH TIME ZONE,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "NextRetryAt" TIMESTAMP WITH TIME ZONE,
    "ErrorMessage" VARCHAR(1000),
    "Metadata" VARCHAR(2000),
    
    CONSTRAINT "FK_WorkRequestNotifications_WorkRequests_WorkRequestId" FOREIGN KEY ("WorkRequestId") REFERENCES "WorkRequests" ("WorkRequestId") ON DELETE CASCADE,
    CONSTRAINT "FK_WorkRequestNotifications_Users_RecipientId" FOREIGN KEY ("RecipientId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_WorkRequestNotifications_Users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- DETAILED TRACKING TABLES
-- ============================================

-- Work Progress Items table
CREATE TABLE "WorkProgressItems" (
    "WorkProgressId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DailyReportId" UUID NOT NULL,
    "ActivityDescription" VARCHAR(500) NOT NULL,
    "Location" VARCHAR(255),
    "StartTime" TIME,
    "EndTime" TIME,
    "HoursWorked" DECIMAL(4,2) NOT NULL,
    "PersonnelCount" INTEGER NOT NULL,
    "CompletionPercentage" INTEGER NOT NULL DEFAULT 0 CHECK ("CompletionPercentage" BETWEEN 0 AND 100),
    "QualityRating" INTEGER CHECK ("QualityRating" BETWEEN 1 AND 5),
    "Notes" VARCHAR(1000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_WorkProgressItems_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE CASCADE
);

-- Personnel Logs table
CREATE TABLE "PersonnelLogs" (
    "PersonnelLogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DailyReportId" UUID NOT NULL,
    "PersonnelName" VARCHAR(255) NOT NULL,
    "Role" VARCHAR(100),
    "HoursWorked" DECIMAL(4,2) NOT NULL,
    "ActivityDescription" VARCHAR(500),
    "Notes" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_PersonnelLogs_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE CASCADE
);

-- Material Usage table
CREATE TABLE "MaterialUsages" (
    "MaterialUsageId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DailyReportId" UUID NOT NULL,
    "MaterialName" VARCHAR(255) NOT NULL,
    "QuantityUsed" DECIMAL(10,2) NOT NULL,
    "Unit" VARCHAR(50) NOT NULL,
    "WastageQuantity" DECIMAL(10,2),
    "WastageReason" VARCHAR(500),
    "Supplier" VARCHAR(255),
    "Notes" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_MaterialUsages_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE CASCADE
);

-- Equipment Logs table
CREATE TABLE "EquipmentLogs" (
    "EquipmentLogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "DailyReportId" UUID NOT NULL,
    "EquipmentName" VARCHAR(255) NOT NULL,
    "EquipmentType" VARCHAR(100),
    "HoursUsed" DECIMAL(4,2) NOT NULL,
    "Condition" VARCHAR(100),
    "Issues" VARCHAR(500),
    "MaintenanceRequired" BOOLEAN NOT NULL DEFAULT FALSE,
    "Notes" VARCHAR(500),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_EquipmentLogs_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE CASCADE
);

-- ============================================
-- CALENDAR AND EVENTS
-- ============================================

-- Calendar Events table
CREATE TABLE "CalendarEvents" (
    "EventId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Title" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "StartDateTime" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EndDateTime" TIMESTAMP WITH TIME ZONE NOT NULL,
    "EventType" INTEGER NOT NULL, -- 1=Meeting, 2=ProjectDeadline, etc.
    "Status" INTEGER NOT NULL DEFAULT 1, -- 1=Scheduled, 2=InProgress, etc.
    "Priority" INTEGER NOT NULL DEFAULT 2, -- 1=Low, 2=Medium, 3=High, 4=Critical
    "Location" VARCHAR(500),
    "IsAllDay" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsRecurring" BOOLEAN NOT NULL DEFAULT FALSE,
    "RecurrencePattern" VARCHAR(100),
    "RecurrenceEndDate" TIMESTAMP WITH TIME ZONE,
    "Notes" VARCHAR(1000),
    "ReminderMinutes" INTEGER,
    "ProjectId" UUID,
    "TaskId" UUID,
    "CreatedByUserId" UUID NOT NULL,
    "AssignedToUserId" UUID,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE,
    "Color" VARCHAR(50) DEFAULT '#3788d8',
    "IsPrivate" BOOLEAN NOT NULL DEFAULT FALSE,
    "MeetingUrl" VARCHAR(255),
    "Attendees" VARCHAR(2000),
    
    CONSTRAINT "FK_CalendarEvents_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE SET NULL,
    CONSTRAINT "FK_CalendarEvents_ProjectTasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "ProjectTasks" ("TaskId") ON DELETE SET NULL,
    CONSTRAINT "FK_CalendarEvents_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT,
    CONSTRAINT "FK_CalendarEvents_Users_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- IMAGE AND FILE MANAGEMENT
-- ============================================

-- Image Metadata table
CREATE TABLE "ImageMetadata" (
    "ImageId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "TaskId" UUID,
    "DailyReportId" UUID,
    "WorkRequestId" UUID,
    "UploadedByUserId" UUID NOT NULL,
    "CloudStorageKey" VARCHAR(500) NOT NULL,
    "OriginalFileName" VARCHAR(255) NOT NULL,
    "ContentType" VARCHAR(100) NOT NULL,
    "FileSizeInBytes" BIGINT NOT NULL,
    "UploadTimestamp" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "CaptureTimestamp" TIMESTAMP WITH TIME ZONE,
    "GPSLatitude" DOUBLE PRECISION,
    "GPSLongitude" DOUBLE PRECISION,
    "DeviceModel" VARCHAR(255),
    "Orientation" SMALLINT,
    "EXIFData" JSONB,
    
    CONSTRAINT "FK_ImageMetadata_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ImageMetadata_ProjectTasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "ProjectTasks" ("TaskId") ON DELETE SET NULL,
    CONSTRAINT "FK_ImageMetadata_DailyReports_DailyReportId" FOREIGN KEY ("DailyReportId") REFERENCES "DailyReports" ("DailyReportId") ON DELETE SET NULL,
    CONSTRAINT "FK_ImageMetadata_WorkRequests_WorkRequestId" FOREIGN KEY ("WorkRequestId") REFERENCES "WorkRequests" ("WorkRequestId") ON DELETE SET NULL,
    CONSTRAINT "FK_ImageMetadata_Users_UploadedByUserId" FOREIGN KEY ("UploadedByUserId") REFERENCES "Users" ("UserId") ON DELETE RESTRICT
);

-- ============================================
-- ENHANCED INDEXES FOR REAL-TIME PERFORMANCE
-- ============================================

-- User indexes
CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");
CREATE INDEX "IX_Users_Email" ON "Users" ("Email");
CREATE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");

-- Enhanced Project indexes for real-time queries
CREATE INDEX "IX_Projects_ProjectManagerId" ON "Projects" ("ProjectManagerId");
CREATE INDEX "IX_Projects_Status" ON "Projects" ("Status");
CREATE INDEX "IX_Projects_StartDate" ON "Projects" ("StartDate");
CREATE INDEX "IX_Projects_Region" ON "Projects" ("Region");
CREATE INDEX "IX_Projects_Province" ON "Projects" ("Province");
CREATE INDEX "IX_Projects_FacilityType" ON "Projects" ("FacilityType");
CREATE INDEX "IX_Projects_LastActivityAt" ON "Projects" ("LastActivityAt");
CREATE INDEX "IX_Projects_CompletionPercentage" ON "Projects" ("CompletionPercentage");
CREATE INDEX "IX_Projects_Location" ON "Projects" ("Latitude", "Longitude");
CREATE INDEX "IX_Projects_RealTimeEnabled" ON "Projects" ("RealTimeEnabled");

-- Real-time notification indexes
CREATE INDEX "IX_UserConnections_UserId" ON "UserConnections" ("UserId");
CREATE INDEX "IX_UserConnections_SignalRConnectionId" ON "UserConnections" ("SignalRConnectionId");
CREATE INDEX "IX_UserConnections_IsActive" ON "UserConnections" ("IsActive");
CREATE INDEX "IX_UserConnections_LastHeartbeat" ON "UserConnections" ("LastHeartbeat");

CREATE INDEX "IX_RealTimeNotifications_UserId" ON "RealTimeNotifications" ("UserId");
CREATE INDEX "IX_RealTimeNotifications_Status" ON "RealTimeNotifications" ("Status");
CREATE INDEX "IX_RealTimeNotifications_EventType" ON "RealTimeNotifications" ("EventType");
CREATE INDEX "IX_RealTimeNotifications_EntityType_EntityId" ON "RealTimeNotifications" ("EntityType", "EntityId");
CREATE INDEX "IX_RealTimeNotifications_CreatedAt" ON "RealTimeNotifications" ("CreatedAt");
CREATE INDEX "IX_RealTimeNotifications_Priority" ON "RealTimeNotifications" ("Priority");

-- SignalR group indexes
CREATE INDEX "IX_SignalRGroups_GroupName" ON "SignalRGroups" ("GroupName");
CREATE INDEX "IX_SignalRGroups_GroupType" ON "SignalRGroups" ("GroupType");
CREATE INDEX "IX_SignalRGroups_EntityId" ON "SignalRGroups" ("EntityId");
CREATE INDEX "IX_SignalRGroups_IsActive" ON "SignalRGroups" ("IsActive");

CREATE INDEX "IX_SignalRGroupMemberships_GroupId" ON "SignalRGroupMemberships" ("GroupId");
CREATE INDEX "IX_SignalRGroupMemberships_UserId" ON "SignalRGroupMemberships" ("UserId");
CREATE INDEX "IX_SignalRGroupMemberships_IsActive" ON "SignalRGroupMemberships" ("IsActive");

-- Project tracking indexes
CREATE INDEX "IX_ProjectStatusHistory_ProjectId" ON "ProjectStatusHistory" ("ProjectId");
CREATE INDEX "IX_ProjectStatusHistory_ChangedAt" ON "ProjectStatusHistory" ("ChangedAt");
CREATE INDEX "IX_ProjectStatusHistory_NewStatus" ON "ProjectStatusHistory" ("NewStatus");

CREATE INDEX "IX_ProjectLocationHistory_ProjectId" ON "ProjectLocationHistory" ("ProjectId");
CREATE INDEX "IX_ProjectLocationHistory_ChangedAt" ON "ProjectLocationHistory" ("ChangedAt");

CREATE INDEX "IX_ProjectActivityFeed_ProjectId" ON "ProjectActivityFeed" ("ProjectId");
CREATE INDEX "IX_ProjectActivityFeed_ActivityType" ON "ProjectActivityFeed" ("ActivityType");
CREATE INDEX "IX_ProjectActivityFeed_CreatedAt" ON "ProjectActivityFeed" ("CreatedAt");
CREATE INDEX "IX_ProjectActivityFeed_IsVisible" ON "ProjectActivityFeed" ("IsVisible");

-- Geographic indexes
CREATE INDEX "IX_ProjectRegionAssignments_ProjectId" ON "ProjectRegionAssignments" ("ProjectId");
CREATE INDEX "IX_ProjectRegionAssignments_RegionId" ON "ProjectRegionAssignments" ("RegionId");

CREATE INDEX "IX_ProjectFacilityAssignments_ProjectId" ON "ProjectFacilityAssignments" ("ProjectId");
CREATE INDEX "IX_ProjectFacilityAssignments_FacilityTypeId" ON "ProjectFacilityAssignments" ("FacilityTypeId");

-- Dashboard cache indexes
CREATE INDEX "IX_DashboardStatsCache_CacheKey" ON "DashboardStatsCache" ("CacheKey");
CREATE INDEX "IX_DashboardStatsCache_CacheType" ON "DashboardStatsCache" ("CacheType");
CREATE INDEX "IX_DashboardStatsCache_EntityId" ON "DashboardStatsCache" ("EntityId");
CREATE INDEX "IX_DashboardStatsCache_ExpiresAt" ON "DashboardStatsCache" ("ExpiresAt");
CREATE INDEX "IX_DashboardStatsCache_IsValid" ON "DashboardStatsCache" ("IsValid");

-- Real-time events log indexes
CREATE INDEX "IX_RealTimeEventsLog_EventType" ON "RealTimeEventsLog" ("EventType");
CREATE INDEX "IX_RealTimeEventsLog_EntityType_EntityId" ON "RealTimeEventsLog" ("EntityType", "EntityId");
CREATE INDEX "IX_RealTimeEventsLog_CreatedAt" ON "RealTimeEventsLog" ("CreatedAt");
CREATE INDEX "IX_RealTimeEventsLog_DeliveryStatus" ON "RealTimeEventsLog" ("DeliveryStatus");

-- Original indexes (preserved)
CREATE INDEX "IX_ProjectTasks_ProjectId" ON "ProjectTasks" ("ProjectId");
CREATE INDEX "IX_ProjectTasks_PhaseId" ON "ProjectTasks" ("PhaseId");
CREATE INDEX "IX_ProjectTasks_AssignedTechnicianId" ON "ProjectTasks" ("AssignedTechnicianId");
CREATE INDEX "IX_ProjectTasks_Status" ON "ProjectTasks" ("Status");
CREATE INDEX "IX_ProjectTasks_DueDate" ON "ProjectTasks" ("DueDate");

-- Master Plan indexes
CREATE INDEX "IX_MasterPlans_ProjectId" ON "MasterPlans" ("ProjectId");
CREATE INDEX "IX_MasterPlans_Status" ON "MasterPlans" ("Status");
CREATE INDEX "IX_MasterPlans_CreatedById" ON "MasterPlans" ("CreatedById");

-- Phase indexes
CREATE INDEX "IX_ProjectPhases_MasterPlanId" ON "ProjectPhases" ("MasterPlanId");
CREATE INDEX "IX_ProjectPhases_Status" ON "ProjectPhases" ("Status");

-- Daily Report indexes
CREATE INDEX "IX_DailyReports_ProjectId" ON "DailyReports" ("ProjectId");
CREATE INDEX "IX_DailyReports_ReportDate" ON "DailyReports" ("ReportDate");
CREATE INDEX "IX_DailyReports_Status" ON "DailyReports" ("Status");

-- Work Request indexes
CREATE INDEX "IX_WorkRequests_ProjectId" ON "WorkRequests" ("ProjectId");
CREATE INDEX "IX_WorkRequests_RequestedById" ON "WorkRequests" ("RequestedById");
CREATE INDEX "IX_WorkRequests_AssignedToId" ON "WorkRequests" ("AssignedToId");
CREATE INDEX "IX_WorkRequests_Status" ON "WorkRequests" ("Status");

-- Calendar Event indexes
CREATE INDEX "IX_CalendarEvents_ProjectId" ON "CalendarEvents" ("ProjectId");
CREATE INDEX "IX_CalendarEvents_StartDateTime" ON "CalendarEvents" ("StartDateTime");
CREATE INDEX "IX_CalendarEvents_CreatedByUserId" ON "CalendarEvents" ("CreatedByUserId");

-- Image Metadata indexes
CREATE INDEX "IX_ImageMetadata_ProjectId" ON "ImageMetadata" ("ProjectId");
CREATE INDEX "IX_ImageMetadata_TaskId" ON "ImageMetadata" ("TaskId");
CREATE INDEX "IX_ImageMetadata_DailyReportId" ON "ImageMetadata" ("DailyReportId");

-- ============================================
-- ENHANCED SEED DATA FOR REAL-TIME FEATURES
-- ============================================

-- Insert default roles
INSERT INTO "Roles" ("RoleId", "RoleName") VALUES 
(1, 'Admin'),
(2, 'Manager'),
(3, 'User'),
(4, 'Viewer');

-- Insert default admin user (password: Admin123!)
INSERT INTO "Users" (
    "UserId", 
    "Username", 
    "Email", 
    "PasswordHash", 
    "FullName", 
    "RoleId", 
    "IsActive", 
    "CreatedAt"
) VALUES (
    '00000000-0000-0000-0000-000000000001',
    'admin',
    'admin@solarprojects.com',
    '$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu',
    'System Administrator',
    1,
    TRUE,
    '2024-01-01 00:00:00+00'
);

-- Insert Thailand Geographic Regions for Real-Time Regional Updates
INSERT INTO "ProjectRegions" ("RegionId", "RegionName", "RegionDisplayName", "Provinces", "Description", "IsActive") VALUES 
(
    '11111111-1111-1111-1111-111111111111',
    'northern',
    'Northern Thailand',
    '["เชียงใหม่", "เชียงราย", "ลำปาง", "ลำพูน", "พะเยา", "แพร่", "น่าน"]',
    'Northern region of Thailand including major provinces with water authority facilities',
    TRUE
),
(
    '22222222-2222-2222-2222-222222222222',
    'western',
    'Western Thailand',
    '["ตาก"]',
    'Western region of Thailand',
    TRUE
),
(
    '33333333-3333-3333-3333-333333333333',
    'central',
    'Central Thailand',
    '["กรุงเทพมหานคร", "พิจิตร", "พิษณุโลก", "สุโขทัย", "อุตรดิตถ์"]',
    'Central region including Bangkok and surrounding provinces',
    TRUE
),
(
    '44444444-4444-4444-4444-444444444444',
    'unknown',
    'Unknown Region',
    '[]',
    'Default region for projects with unidentified geographic location',
    TRUE
);

-- Insert Water Facility Types for Thailand Water Authority Projects
INSERT INTO "WaterFacilityTypes" ("FacilityTypeId", "FacilityTypeName", "FacilityDisplayName", "Description", "Authority", "SignalRGroupName", "IsActive") VALUES 
(
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'water_treatment',
    'Water Treatment Plant',
    'Water treatment and purification facilities operated by Provincial Waterworks Authority',
    'Provincial Waterworks Authority',
    'facility_water_treatment',
    TRUE
),
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'pumping_station',
    'Water Pumping Station',
    'Water pumping and distribution stations',
    'Provincial Waterworks Authority',
    'facility_pumping_station',
    TRUE
),
(
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'reservoir',
    'Water Reservoir',
    'Water storage and reservoir facilities',
    'Provincial Waterworks Authority',
    'facility_reservoir',
    TRUE
),
(
    'dddddddd-dddd-dddd-dddd-dddddddddddd',
    'distribution_center',
    'Distribution Center',
    'Water distribution and supply centers',
    'Metropolitan Waterworks Authority',
    'facility_distribution',
    TRUE
),
(
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
    'solar_installation',
    'Solar Installation Site',
    'Solar panel installation and maintenance sites',
    'Solar Project Management',
    'facility_solar_installation',
    TRUE
);

-- Insert Default SignalR Groups for Real-Time Updates
INSERT INTO "SignalRGroups" ("GroupId", "GroupName", "GroupType", "Description", "IsActive") VALUES 
(
    'f1111111-1111-1111-1111-111111111111',
    'all_users',
    'global',
    'Global group for system-wide announcements and updates',
    TRUE
),
(
    'f2222222-2222-2222-2222-222222222222',
    'role_admin',
    'role',
    'Administrative users group for high-priority notifications',
    TRUE
),
(
    'f3333333-3333-3333-3333-333333333333',
    'role_manager',
    'role',
    'Project managers group for management-level updates',
    TRUE
),
(
    'f4444444-4444-4444-4444-444444444444',
    'region_northern',
    'region',
    'Northern Thailand regional project updates',
    TRUE
),
(
    'f5555555-5555-5555-5555-555555555555',
    'region_western',
    'region',
    'Western Thailand regional project updates',
    TRUE
),
(
    'f6666666-6666-6666-6666-666666666666',
    'region_central',
    'region',
    'Central Thailand regional project updates',
    TRUE
),
(
    'f7777777-7777-7777-7777-777777777777',
    'facility_water_treatment',
    'facility',
    'Water treatment facility updates',
    TRUE
),
(
    'f8888888-8888-8888-8888-888888888888',
    'facility_solar_installation',
    'facility',
    'Solar installation facility updates',
    TRUE
),
(
    'f9999999-9999-9999-9999-999999999999',
    'map_viewers',
    'special',
    'Users viewing real-time map and location updates',
    TRUE
);

-- ============================================
-- ENHANCED SCHEMA DOCUMENTATION
-- ============================================

/*
This schema represents a comprehensive solar project management system with advanced real-time features:

CORE FEATURES:
1. **User Management**: Role-based access control with Admin, Manager, User, and Viewer roles
2. **Project Management**: Complete project lifecycle with solar-specific fields and real-time tracking
3. **Master Planning**: Hierarchical planning with phases, milestones, and resource allocation
4. **Task Management**: Detailed task tracking with assignment and progress monitoring
5. **Reporting**: Daily, weekly, and progress reports with comprehensive tracking
6. **Work Requests**: Complete workflow with approval processes and notifications
7. **Calendar Integration**: Event management tied to projects and tasks
8. **Image Management**: Metadata tracking for project photos with EXIF data
9. **Audit Trail**: Created/Updated timestamps and user tracking throughout

REAL-TIME FEATURES (July 2025 Enhancement):
10. **SignalR Integration**: Complete real-time notification system with WebSocket support
11. **Geographic Intelligence**: Thailand-specific regional grouping (Northern, Western, Central)
12. **Water Authority Mapping**: Specialized facility tracking for Thai water authority projects
13. **Live Location Tracking**: GPS coordinate and address change tracking with accuracy
14. **Status Change Broadcasting**: Real-time project status transitions with user attribution
15. **Activity Feed**: Comprehensive project activity tracking with real-time notifications
16. **Dashboard Statistics**: Live dashboard metrics with regional breakdowns and caching
17. **Group Management**: SignalR group subscriptions for targeted real-time updates
18. **Connection Tracking**: User connection management with heartbeat monitoring
19. **Event Logging**: Comprehensive real-time event auditing and debugging
20. **Performance Optimization**: Enhanced indexes for real-time query performance

THAILAND WATER AUTHORITY INTEGRATION:
- Regional project grouping by Thai provinces
- Water facility type categorization (treatment plants, pumping stations, reservoirs)
- Real-time updates filtered by geographic regions
- Provincial and Metropolitan Waterworks Authority support
- GPS-based automatic region detection for Thai coordinates

REAL-TIME ARCHITECTURE:
- UserConnections: Active SignalR connection tracking
- RealTimeNotifications: Queued notification system with delivery status
- SignalRGroups: Hierarchical group management (projects, regions, facilities, roles)
- ProjectStatusHistory: Complete status change audit trail
- ProjectLocationHistory: GPS and address change tracking
- ProjectActivityFeed: Real-time activity stream for each project
- DashboardStatsCache: Optimized dashboard performance with caching

The schema supports:
- PostgreSQL-specific features (JSONB, UUID, GEN_RANDOM_UUID())
- Proper foreign key relationships with cascade options
- Performance indexes on frequently queried columns (enhanced for real-time)
- Check constraints for data validation
- Comprehensive seed data for initial system setup including regional data
- Real-time notification delivery tracking and retry mechanisms
- Geographic intelligence for Thailand-specific water authority projects

SIGNALR GROUP TYPES:
- project_{projectId}: Project-specific updates
- region_{regionName}: Geographic regional updates (northern, western, central)
- facility_{facilityType}: Water facility type updates
- role_{roleName}: Role-based notifications
- user_{userId}: Personal user notifications
- map_viewers: Real-time location and map updates
- all_users: System-wide announcements

ENUM VALUES REFERENCE:
- ProjectStatus: 0=Planning, 1=InProgress, 2=Completed, 3=OnHold, 4=Cancelled
- TaskStatus: 0=NotStarted, 1=InProgress, 2=Completed, 3=OnHold, 4=Cancelled
- TaskPriority: 0=Low, 1=Medium, 2=High, 3=Critical
- WorkRequestStatus: Various workflow states from Open to Completed
- MasterPlanStatus: 0=Draft, 1=UnderReview, 2=Approved, 3=Active, 4=Completed, 5=Cancelled
- NotificationStatus: 0=Pending, 1=Sent, 2=Delivered, 3=Read, 4=Failed
- GroupType: project, region, facility, role, user, special, global

REAL-TIME PERFORMANCE FEATURES:
- Optimized indexes for geographic queries (lat/lng)
- Regional project filtering with province-based grouping
- Cached dashboard statistics with TTL expiration
- Connection heartbeat monitoring for reliability
- Event aggregation to reduce notification spam
- Optimistic concurrency control with version fields
- Background processing for notification delivery

This schema is production-ready for the Thai Solar Project Management system with 
comprehensive real-time collaboration features and geographic intelligence.
*/

-- ============================================
-- REAL-TIME NOTIFICATIONS AND SIGNALR SUPPORT
-- ============================================

-- User Connections table (for SignalR connection tracking)
CREATE TABLE "UserConnections" (
    "ConnectionId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "SignalRConnectionId" VARCHAR(255) NOT NULL,
    "UserAgent" VARCHAR(500),
    "IpAddress" VARCHAR(45),
    "ConnectedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LastHeartbeat" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisconnectedAt" TIMESTAMP WITH TIME ZONE,
    "SessionData" JSONB,
    
    CONSTRAINT "FK_UserConnections_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

-- Real-Time Notifications table
CREATE TABLE "RealTimeNotifications" (
    "NotificationId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID,
    "GroupName" VARCHAR(255),
    "EventType" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Message" VARCHAR(2000) NOT NULL,
    "Data" JSONB NOT NULL DEFAULT '{}',
    "Priority" INTEGER NOT NULL DEFAULT 1, -- 0=Low, 1=Medium, 2=High, 3=Critical
    "Status" INTEGER NOT NULL DEFAULT 0, -- 0=Pending, 1=Sent, 2=Delivered, 3=Read, 4=Failed
    "CreatedBy" VARCHAR(255),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "SentAt" TIMESTAMP WITH TIME ZONE,
    "DeliveredAt" TIMESTAMP WITH TIME ZONE,
    "ReadAt" TIMESTAMP WITH TIME ZONE,
    "ErrorMessage" VARCHAR(1000),
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "MaxRetries" INTEGER NOT NULL DEFAULT 3,
    "ExpiresAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_RealTimeNotifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE
);

-- SignalR Groups table (for managing user group subscriptions)
CREATE TABLE "SignalRGroups" (
    "GroupId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "GroupName" VARCHAR(255) NOT NULL,
    "GroupType" VARCHAR(100) NOT NULL, -- project, region, facility, role, user, map_viewers
    "Description" VARCHAR(500),
    "EntityId" UUID, -- References Projects, Users, etc. depending on group type
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

-- SignalR Group Memberships table
CREATE TABLE "SignalRGroupMemberships" (
    "MembershipId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "GroupId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ConnectionId" UUID,
    "JoinedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "LeftAt" TIMESTAMP WITH TIME ZONE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "MembershipData" JSONB,
    
    CONSTRAINT "FK_SignalRGroupMemberships_SignalRGroups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "SignalRGroups" ("GroupId") ON DELETE CASCADE,
    CONSTRAINT "FK_SignalRGroupMemberships_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE CASCADE,
    CONSTRAINT "FK_SignalRGroupMemberships_UserConnections_ConnectionId" FOREIGN KEY ("ConnectionId") REFERENCES "UserConnections" ("ConnectionId") ON DELETE SET NULL
);

-- Real-Time Events Log table (for auditing and debugging)
CREATE TABLE "RealTimeEventsLog" (
    "EventLogId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "EventType" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "UserId" UUID,
    "UserName" VARCHAR(255),
    "EventData" JSONB NOT NULL DEFAULT '{}',
    "Recipients" JSONB NOT NULL DEFAULT '[]', -- Array of user IDs or group names
    "BroadcastGroups" JSONB NOT NULL DEFAULT '[]', -- Array of SignalR group names
    "DeliveryStatus" INTEGER NOT NULL DEFAULT 0, -- 0=Pending, 1=Sent, 2=Failed
    "ProcessingTimeMs" INTEGER,
    "ErrorDetails" VARCHAR(2000),
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ProcessedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_RealTimeEventsLog_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Project Geographic Regions table (for Thailand-specific regional grouping)
CREATE TABLE "ProjectRegions" (
    "RegionId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "RegionName" VARCHAR(100) NOT NULL, -- northern, western, central, unknown
    "RegionDisplayName" VARCHAR(255) NOT NULL, -- "Northern Thailand", etc.
    "Provinces" JSONB NOT NULL DEFAULT '[]', -- Array of province names
    "BoundingBox" JSONB, -- Geographic bounding box coordinates
    "Description" VARCHAR(500),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Project Region Assignments table
CREATE TABLE "ProjectRegionAssignments" (
    "AssignmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "RegionId" UUID NOT NULL,
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "AssignmentMethod" VARCHAR(100) NOT NULL, -- automatic, manual, coordinate_based, address_based
    "Confidence" DECIMAL(3,2), -- 0.00 to 1.00 confidence score for automatic assignments
    "Notes" VARCHAR(500),
    
    CONSTRAINT "FK_ProjectRegionAssignments_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectRegionAssignments_ProjectRegions_RegionId" FOREIGN KEY ("RegionId") REFERENCES "ProjectRegions" ("RegionId") ON DELETE CASCADE
);

-- Dashboard Statistics Cache table (for real-time dashboard performance)
CREATE TABLE "DashboardStatsCache" (
    "CacheId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "CacheKey" VARCHAR(255) NOT NULL,
    "CacheType" VARCHAR(100) NOT NULL, -- global, regional, project, user
    "EntityId" UUID, -- ProjectId, UserId, RegionId, etc.
    "Statistics" JSONB NOT NULL DEFAULT '{}',
    "LastCalculated" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ExpiresAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "Version" INTEGER NOT NULL DEFAULT 1,
    "IsValid" BOOLEAN NOT NULL DEFAULT TRUE
);

-- ============================================
-- ENHANCED PROJECT TRACKING TABLES
-- ============================================

-- Project Status History table (for tracking all status changes with real-time support)
CREATE TABLE "ProjectStatusHistory" (
    "StatusHistoryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "OldStatus" INTEGER NOT NULL,
    "NewStatus" INTEGER NOT NULL,
    "ChangedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ChangedByUserId" UUID,
    "ChangedByUserName" VARCHAR(255),
    "ChangeReason" VARCHAR(1000),
    "CompletionPercentage" DECIMAL(5,2),
    "ActualEndDate" TIMESTAMP WITH TIME ZONE,
    "EstimatedEndDate" TIMESTAMP WITH TIME ZONE,
    "Notes" VARCHAR(2000),
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "NotificationRecipients" JSONB NOT NULL DEFAULT '[]',
    
    CONSTRAINT "FK_ProjectStatusHistory_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectStatusHistory_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Project Location History table (for GPS and address change tracking)
CREATE TABLE "ProjectLocationHistory" (
    "LocationHistoryId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "OldLatitude" DECIMAL(10,7),
    "OldLongitude" DECIMAL(10,7),
    "NewLatitude" DECIMAL(10,7),
    "NewLongitude" DECIMAL(10,7),
    "OldAddress" VARCHAR(500),
    "NewAddress" VARCHAR(500),
    "ChangedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "ChangedByUserId" UUID,
    "ChangedByUserName" VARCHAR(255),
    "ChangeMethod" VARCHAR(100) NOT NULL, -- manual, gps_update, address_geocoding, system_correction
    "AccuracyMeters" DECIMAL(8,2),
    "DataSource" VARCHAR(255), -- mobile_app, web_interface, api, bulk_import
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "GeofenceAlerts" JSONB NOT NULL DEFAULT '[]',
    
    CONSTRAINT "FK_ProjectLocationHistory_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectLocationHistory_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Project Activity Feed table (for comprehensive project activity tracking)
CREATE TABLE "ProjectActivityFeed" (
    "ActivityId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "ActivityType" VARCHAR(100) NOT NULL, -- created, updated, status_changed, location_updated, milestone_reached, etc.
    "EntityType" VARCHAR(100) NOT NULL, -- project, task, daily_report, work_request, etc.
    "EntityId" UUID NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(1000) NOT NULL,
    "PerformedByUserId" UUID,
    "PerformedByUserName" VARCHAR(255),
    "ActivityData" JSONB NOT NULL DEFAULT '{}',
    "Severity" INTEGER NOT NULL DEFAULT 1, -- 0=Info, 1=Low, 2=Medium, 3=High, 4=Critical
    "IsVisible" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "RealTimeNotificationSent" BOOLEAN NOT NULL DEFAULT FALSE,
    "AggregatedCount" INTEGER NOT NULL DEFAULT 1, -- For grouping similar activities
    "LastAggregatedAt" TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT "FK_ProjectActivityFeed_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectActivityFeed_Users_PerformedByUserId" FOREIGN KEY ("PerformedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- Water Facility Types table (for Thailand water authority facility categorization)
CREATE TABLE "WaterFacilityTypes" (
    "FacilityTypeId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "FacilityTypeName" VARCHAR(100) NOT NULL, -- water_treatment, pumping_station, reservoir, distribution_center
    "FacilityDisplayName" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(500),
    "Authority" VARCHAR(255), -- "Provincial Waterworks Authority", "Metropolitan Waterworks Authority"
    "SignalRGroupName" VARCHAR(255), -- facility_water_treatment, facility_pumping_station, etc.
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Project Facility Assignments table
CREATE TABLE "ProjectFacilityAssignments" (
    "AssignmentId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProjectId" UUID NOT NULL,
    "FacilityTypeId" UUID NOT NULL,
    "FacilityName" VARCHAR(255),
    "FacilityCode" VARCHAR(100),
    "Authority" VARCHAR(255),
    "AssignedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "AssignmentMethod" VARCHAR(100) NOT NULL, -- manual, address_matching, coordinate_matching, import
    "Confidence" DECIMAL(3,2),
    "VerifiedAt" TIMESTAMP WITH TIME ZONE,
    "VerifiedByUserId" UUID,
    "Notes" VARCHAR(500),
    
    CONSTRAINT "FK_ProjectFacilityAssignments_Projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("ProjectId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectFacilityAssignments_WaterFacilityTypes_FacilityTypeId" FOREIGN KEY ("FacilityTypeId") REFERENCES "WaterFacilityTypes" ("FacilityTypeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProjectFacilityAssignments_Users_VerifiedByUserId" FOREIGN KEY ("VerifiedByUserId") REFERENCES "Users" ("UserId") ON DELETE SET NULL
);

-- ============================================
-- ENHANCED INDEXES FOR REAL-TIME PERFORMANCE
-- ============================================
