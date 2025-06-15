using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterPlanSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualHours",
                table: "ProjectTasks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPercentage",
                table: "ProjectTasks",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "ProjectTasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "ProjectTasks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PhaseId",
                table: "ProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhaseId1",
                table: "ProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ProjectTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightInPhase",
                table: "ProjectTasks",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MasterPlans",
                columns: table => new
                {
                    MasterPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPlannedDays = table.Column<int>(type: "integer", nullable: false),
                    TotalEstimatedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPlans", x => x.MasterPlanId);
                    table.ForeignKey(
                        name: "FK_MasterPlans_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MasterPlans_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MasterPlans_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyReports",
                columns: table => new
                {
                    WeeklyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SummaryOfProgress = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TotalManHours = table.Column<int>(type: "integer", nullable: false),
                    PanelsInstalled = table.Column<int>(type: "integer", nullable: false),
                    SafetyIncidents = table.Column<int>(type: "integer", nullable: false),
                    DelaysReported = table.Column<int>(type: "integer", nullable: false),
                    MajorAccomplishments = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    MajorIssues = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    Lookahead = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletionPercentage = table.Column<int>(type: "integer", nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReports", x => x.WeeklyReportId);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WeeklyReports_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyWorkRequests",
                columns: table => new
                {
                    WeeklyRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OverallGoals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    KeyTasks = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    PersonnelForecast = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MajorEquipment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriticalMaterials = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedHours = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyWorkRequests", x => x.WeeklyRequestId);
                    table.ForeignKey(
                        name: "FK_WeeklyWorkRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyWorkRequests_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WeeklyWorkRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProgressReports",
                columns: table => new
                {
                    ProgressReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverallCompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SchedulePerformanceIndex = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    CostPerformanceIndex = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    ActualCostToDate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstimatedCostAtCompletion = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    BudgetVariance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ScheduleVarianceDays = table.Column<int>(type: "integer", nullable: false),
                    ProjectedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActiveIssuesCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedMilestonesCount = table.Column<int>(type: "integer", nullable: false),
                    TotalMilestonesCount = table.Column<int>(type: "integer", nullable: false),
                    HealthStatus = table.Column<int>(type: "integer", nullable: false),
                    KeyAccomplishments = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentChallenges = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    UpcomingActivities = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RiskSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    QualityNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WeatherImpact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResourceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExecutiveSummary = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressReports", x => x.ProgressReportId);
                    table.ForeignKey(
                        name: "FK_ProgressReports_MasterPlans_MasterPlanId",
                        column: x => x.MasterPlanId,
                        principalTable: "MasterPlans",
                        principalColumn: "MasterPlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressReports_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPhases",
                columns: table => new
                {
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PhaseOrder = table.Column<int>(type: "integer", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedDurationDays = table.Column<int>(type: "integer", nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Prerequisites = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPhases", x => x.PhaseId);
                    table.ForeignKey(
                        name: "FK_ProjectPhases_MasterPlans_MasterPlanId",
                        column: x => x.MasterPlanId,
                        principalTable: "MasterPlans",
                        principalColumn: "MasterPlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseProgresses",
                columns: table => new
                {
                    PhaseProgressId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    PlannedCompletionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ProgressVariance = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ActivitiesCompleted = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Issues = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseProgresses", x => x.PhaseProgressId);
                    table.ForeignKey(
                        name: "FK_PhaseProgresses_ProgressReports_ProgressReportId",
                        column: x => x.ProgressReportId,
                        principalTable: "ProgressReports",
                        principalColumn: "ProgressReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhaseProgresses_ProjectPhases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "PhaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhaseResources",
                columns: table => new
                {
                    PhaseResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<int>(type: "integer", nullable: false),
                    ResourceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QuantityRequired = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualQuantityUsed = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    AllocationStatus = table.Column<int>(type: "integer", nullable: false),
                    Supplier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseResources", x => x.PhaseResourceId);
                    table.ForeignKey(
                        name: "FK_PhaseResources_ProjectPhases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "PhaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestones",
                columns: table => new
                {
                    MilestoneId = table.Column<Guid>(type: "uuid", nullable: false),
                    MasterPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    MilestoneName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PlannedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Importance = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WeightPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CompletionCriteria = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletionEvidence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestones", x => x.MilestoneId);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_MasterPlans_MasterPlanId",
                        column: x => x.MasterPlanId,
                        principalTable: "MasterPlans",
                        principalColumn: "MasterPlanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_ProjectPhases_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "ProjectPhases",
                        principalColumn: "PhaseId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_CompletionPercentage",
                table: "ProjectTasks",
                column: "CompletionPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_PhaseId",
                table: "ProjectTasks",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_PhaseId1",
                table: "ProjectTasks",
                column: "PhaseId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_Priority",
                table: "ProjectTasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlans_ApprovedById",
                table: "MasterPlans",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlans_CreatedAt",
                table: "MasterPlans",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlans_CreatedById",
                table: "MasterPlans",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlans_ProjectId",
                table: "MasterPlans",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasterPlans_Status",
                table: "MasterPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseProgresses_PhaseId",
                table: "PhaseProgresses",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseProgresses_ProgressReportId",
                table: "PhaseProgresses",
                column: "ProgressReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseResources_AllocationStatus",
                table: "PhaseResources",
                column: "AllocationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseResources_PhaseId",
                table: "PhaseResources",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PhaseResources_ResourceType",
                table: "PhaseResources",
                column: "ResourceType");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_CreatedById",
                table: "ProgressReports",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_HealthStatus",
                table: "ProgressReports",
                column: "HealthStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_MasterPlanId",
                table: "ProgressReports",
                column: "MasterPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_ProjectId",
                table: "ProgressReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressReports_ReportDate",
                table: "ProgressReports",
                column: "ReportDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_MasterPlanId",
                table: "ProjectMilestones",
                column: "MasterPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_PhaseId",
                table: "ProjectMilestones",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_PlannedDate",
                table: "ProjectMilestones",
                column: "PlannedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_Status",
                table: "ProjectMilestones",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_VerifiedById",
                table: "ProjectMilestones",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPhases_MasterPlanId",
                table: "ProjectPhases",
                column: "MasterPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPhases_PhaseOrder",
                table: "ProjectPhases",
                column: "PhaseOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPhases_Status",
                table: "ProjectPhases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_ApprovedById",
                table: "WeeklyReports",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_CreatedAt",
                table: "WeeklyReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_ProjectId",
                table: "WeeklyReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_Status",
                table: "WeeklyReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_SubmittedById",
                table: "WeeklyReports",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReports_WeekStartDate",
                table: "WeeklyReports",
                column: "WeekStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_ApprovedById",
                table: "WeeklyWorkRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_CreatedAt",
                table: "WeeklyWorkRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_ProjectId",
                table: "WeeklyWorkRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_RequestedById",
                table: "WeeklyWorkRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_Status",
                table: "WeeklyWorkRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWorkRequests_WeekStartDate",
                table: "WeeklyWorkRequests",
                column: "WeekStartDate");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId",
                table: "ProjectTasks",
                column: "PhaseId",
                principalTable: "ProjectPhases",
                principalColumn: "PhaseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId1",
                table: "ProjectTasks",
                column: "PhaseId1",
                principalTable: "ProjectPhases",
                principalColumn: "PhaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId",
                table: "ProjectTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.DropTable(
                name: "PhaseProgresses");

            migrationBuilder.DropTable(
                name: "PhaseResources");

            migrationBuilder.DropTable(
                name: "ProjectMilestones");

            migrationBuilder.DropTable(
                name: "WeeklyReports");

            migrationBuilder.DropTable(
                name: "WeeklyWorkRequests");

            migrationBuilder.DropTable(
                name: "ProgressReports");

            migrationBuilder.DropTable(
                name: "ProjectPhases");

            migrationBuilder.DropTable(
                name: "MasterPlans");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_CompletionPercentage",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_PhaseId",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_Priority",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "ActualHours",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "PhaseId",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "WeightInPhase",
                table: "ProjectTasks");
        }
    }
}
