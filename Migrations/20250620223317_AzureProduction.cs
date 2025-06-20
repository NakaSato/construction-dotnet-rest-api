using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AzureProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_ProjectTasks_TaskId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Projects_ProjectId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Users_AssignedToUserId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Users_CreatedByUserId",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PhaseResources_ProjectPhases_PhaseId",
                table: "PhaseResources");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReports_Projects_ProjectId",
                table: "WeeklyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReports_Users_ApprovedById",
                table: "WeeklyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReports_Users_SubmittedById",
                table: "WeeklyReports");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequests_Projects_ProjectId",
                table: "WeeklyWorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequests_Users_ApprovedById",
                table: "WeeklyWorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequests_Users_RequestedById",
                table: "WeeklyWorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApprovals_Users_ApproverId",
                table: "WorkRequestApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApprovals_WorkRequestApprovals_EscalatedFromId",
                table: "WorkRequestApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApprovals_WorkRequests_WorkRequestId",
                table: "WorkRequestApprovals");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotifications_Users_RecipientId",
                table: "WorkRequestNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotifications_Users_SenderId",
                table: "WorkRequestNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotifications_WorkRequests_WorkRequestId",
                table: "WorkRequestNotifications");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleName",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_ProjectId",
                table: "DailyReports");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_ReporterId",
                table: "DailyReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestNotifications",
                table: "WorkRequestNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestApprovals",
                table: "WorkRequestApprovals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyWorkRequests",
                table: "WeeklyWorkRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyReports",
                table: "WeeklyReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseResources",
                table: "PhaseResources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents");

            migrationBuilder.RenameTable(
                name: "WorkRequestNotifications",
                newName: "WorkRequestNotification");

            migrationBuilder.RenameTable(
                name: "WorkRequestApprovals",
                newName: "WorkRequestApproval");

            migrationBuilder.RenameTable(
                name: "WeeklyWorkRequests",
                newName: "WeeklyWorkRequest");

            migrationBuilder.RenameTable(
                name: "WeeklyReports",
                newName: "WeeklyReport");

            migrationBuilder.RenameTable(
                name: "PhaseResources",
                newName: "PhaseResource");

            migrationBuilder.RenameTable(
                name: "CalendarEvents",
                newName: "CalendarEvent");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_WorkRequestId",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_Type",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_Type");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_Status",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_SentAt",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_SentAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_SenderId",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_RecipientId",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_ReadAt",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_ReadAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotifications_CreatedAt",
                table: "WorkRequestNotification",
                newName: "IX_WorkRequestNotification_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_WorkRequestId",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_Level",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_Level");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_IsActive",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_EscalatedFromId",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_EscalatedFromId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_CreatedAt",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_ApproverId",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApprovals_Action",
                table: "WorkRequestApproval",
                newName: "IX_WorkRequestApproval_Action");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_WeekStartDate",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_WeekStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_Status",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_RequestedById",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_RequestedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_ProjectId",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_CreatedAt",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequests_ApprovedById",
                table: "WeeklyWorkRequest",
                newName: "IX_WeeklyWorkRequest_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_WeekStartDate",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_WeekStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_SubmittedById",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_SubmittedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_Status",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_ProjectId",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_CreatedAt",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReports_ApprovedById",
                table: "WeeklyReport",
                newName: "IX_WeeklyReport_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResources_ResourceType",
                table: "PhaseResource",
                newName: "IX_PhaseResource_ResourceType");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResources_PhaseId",
                table: "PhaseResource",
                newName: "IX_PhaseResource_PhaseId");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResources_AllocationStatus",
                table: "PhaseResource",
                newName: "IX_PhaseResource_AllocationStatus");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_TaskId",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_Status",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_Status");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_StartDateTime",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_StartDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_ProjectId",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_EventType",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_EventType");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_EndDateTime",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_EndDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_CreatedByUserId",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvents_AssignedToUserId",
                table: "CalendarEvent",
                newName: "IX_CalendarEvent_AssignedToUserId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyReportId",
                table: "DailyReports",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestNotification",
                table: "WorkRequestNotification",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestApproval",
                table: "WorkRequestApproval",
                column: "ApprovalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyWorkRequest",
                table: "WeeklyWorkRequest",
                column: "WeeklyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyReport",
                table: "WeeklyReport",
                column: "WeeklyReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseResource",
                table: "PhaseResource",
                column: "PhaseResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent",
                column: "EventId");

            migrationBuilder.CreateTable(
                name: "DailyReportAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReportAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyReportAttachments_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "DailyReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualHours = table.Column<decimal>(type: "numeric", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_MasterPlans_PhaseId",
                        column: x => x.PhaseId,
                        principalTable: "MasterPlans",
                        principalColumn: "MasterPlanId");
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TaskProgressReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "numeric", nullable: false),
                    Issues = table.Column<string>(type: "text", nullable: false),
                    NextSteps = table.Column<string>(type: "text", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskProgressReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskProgressReport_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId_ReportDate",
                table: "DailyReports",
                columns: new[] { "ProjectId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ReporterId_ReportDate",
                table: "DailyReports",
                columns: new[] { "ReporterId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyReportAttachments_DailyReportId",
                table: "DailyReportAttachments",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskProgressReport_TaskId",
                table: "TaskProgressReport",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToId",
                table: "Tasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_PhaseId",
                table: "Tasks",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvent_ProjectTasks_TaskId",
                table: "CalendarEvent",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvent_Projects_ProjectId",
                table: "CalendarEvent",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvent_Users_AssignedToUserId",
                table: "CalendarEvent",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvent_Users_CreatedByUserId",
                table: "CalendarEvent",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhaseResource_ProjectPhases_PhaseId",
                table: "PhaseResource",
                column: "PhaseId",
                principalTable: "ProjectPhases",
                principalColumn: "PhaseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReport_Projects_ProjectId",
                table: "WeeklyReport",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReport_Users_ApprovedById",
                table: "WeeklyReport",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReport_Users_SubmittedById",
                table: "WeeklyReport",
                column: "SubmittedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequest_Projects_ProjectId",
                table: "WeeklyWorkRequest",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequest_Users_ApprovedById",
                table: "WeeklyWorkRequest",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequest_Users_RequestedById",
                table: "WeeklyWorkRequest",
                column: "RequestedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApproval_Users_ApproverId",
                table: "WorkRequestApproval",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApproval_WorkRequestApproval_EscalatedFromId",
                table: "WorkRequestApproval",
                column: "EscalatedFromId",
                principalTable: "WorkRequestApproval",
                principalColumn: "ApprovalId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApproval_WorkRequests_WorkRequestId",
                table: "WorkRequestApproval",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotification_Users_RecipientId",
                table: "WorkRequestNotification",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotification_Users_SenderId",
                table: "WorkRequestNotification",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotification_WorkRequests_WorkRequestId",
                table: "WorkRequestNotification",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvent_ProjectTasks_TaskId",
                table: "CalendarEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvent_Projects_ProjectId",
                table: "CalendarEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvent_Users_AssignedToUserId",
                table: "CalendarEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvent_Users_CreatedByUserId",
                table: "CalendarEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_PhaseResource_ProjectPhases_PhaseId",
                table: "PhaseResource");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReport_Projects_ProjectId",
                table: "WeeklyReport");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReport_Users_ApprovedById",
                table: "WeeklyReport");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyReport_Users_SubmittedById",
                table: "WeeklyReport");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequest_Projects_ProjectId",
                table: "WeeklyWorkRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequest_Users_ApprovedById",
                table: "WeeklyWorkRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyWorkRequest_Users_RequestedById",
                table: "WeeklyWorkRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApproval_Users_ApproverId",
                table: "WorkRequestApproval");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApproval_WorkRequestApproval_EscalatedFromId",
                table: "WorkRequestApproval");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestApproval_WorkRequests_WorkRequestId",
                table: "WorkRequestApproval");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotification_Users_RecipientId",
                table: "WorkRequestNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotification_Users_SenderId",
                table: "WorkRequestNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequestNotification_WorkRequests_WorkRequestId",
                table: "WorkRequestNotification");

            migrationBuilder.DropTable(
                name: "DailyReportAttachments");

            migrationBuilder.DropTable(
                name: "TaskProgressReport");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_ProjectId_ReportDate",
                table: "DailyReports");

            migrationBuilder.DropIndex(
                name: "IX_DailyReports_ReporterId_ReportDate",
                table: "DailyReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestNotification",
                table: "WorkRequestNotification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestApproval",
                table: "WorkRequestApproval");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyWorkRequest",
                table: "WeeklyWorkRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyReport",
                table: "WeeklyReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseResource",
                table: "PhaseResource");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent");

            migrationBuilder.RenameTable(
                name: "WorkRequestNotification",
                newName: "WorkRequestNotifications");

            migrationBuilder.RenameTable(
                name: "WorkRequestApproval",
                newName: "WorkRequestApprovals");

            migrationBuilder.RenameTable(
                name: "WeeklyWorkRequest",
                newName: "WeeklyWorkRequests");

            migrationBuilder.RenameTable(
                name: "WeeklyReport",
                newName: "WeeklyReports");

            migrationBuilder.RenameTable(
                name: "PhaseResource",
                newName: "PhaseResources");

            migrationBuilder.RenameTable(
                name: "CalendarEvent",
                newName: "CalendarEvents");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_WorkRequestId",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_Type",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_Type");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_Status",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_SentAt",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_SentAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_SenderId",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_RecipientId",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_RecipientId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_ReadAt",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_ReadAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestNotification_CreatedAt",
                table: "WorkRequestNotifications",
                newName: "IX_WorkRequestNotifications_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_WorkRequestId",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_Level",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_Level");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_IsActive",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_EscalatedFromId",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_EscalatedFromId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_CreatedAt",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_ApproverId",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_ApproverId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequestApproval_Action",
                table: "WorkRequestApprovals",
                newName: "IX_WorkRequestApprovals_Action");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_WeekStartDate",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_WeekStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_Status",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_RequestedById",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_RequestedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_ProjectId",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_CreatedAt",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyWorkRequest_ApprovedById",
                table: "WeeklyWorkRequests",
                newName: "IX_WeeklyWorkRequests_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_WeekStartDate",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_WeekStartDate");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_SubmittedById",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_SubmittedById");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_Status",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_Status");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_ProjectId",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_CreatedAt",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_WeeklyReport_ApprovedById",
                table: "WeeklyReports",
                newName: "IX_WeeklyReports_ApprovedById");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResource_ResourceType",
                table: "PhaseResources",
                newName: "IX_PhaseResources_ResourceType");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResource_PhaseId",
                table: "PhaseResources",
                newName: "IX_PhaseResources_PhaseId");

            migrationBuilder.RenameIndex(
                name: "IX_PhaseResource_AllocationStatus",
                table: "PhaseResources",
                newName: "IX_PhaseResources_AllocationStatus");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_TaskId",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_Status",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_Status");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_StartDateTime",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_StartDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_ProjectId",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_EventType",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_EventType");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_EndDateTime",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_EndDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_CreatedByUserId",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CalendarEvent_AssignedToUserId",
                table: "CalendarEvents",
                newName: "IX_CalendarEvents_AssignedToUserId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "DailyReportId",
                table: "DailyReports",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestNotifications",
                table: "WorkRequestNotifications",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestApprovals",
                table: "WorkRequestApprovals",
                column: "ApprovalId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyWorkRequests",
                table: "WeeklyWorkRequests",
                column: "WeeklyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyReports",
                table: "WeeklyReports",
                column: "WeeklyReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseResources",
                table: "PhaseResources",
                column: "PhaseResourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId",
                table: "DailyReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ReporterId",
                table: "DailyReports",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_ProjectTasks_TaskId",
                table: "CalendarEvents",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Projects_ProjectId",
                table: "CalendarEvents",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Users_AssignedToUserId",
                table: "CalendarEvents",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Users_CreatedByUserId",
                table: "CalendarEvents",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PhaseResources_ProjectPhases_PhaseId",
                table: "PhaseResources",
                column: "PhaseId",
                principalTable: "ProjectPhases",
                principalColumn: "PhaseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReports_Projects_ProjectId",
                table: "WeeklyReports",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReports_Users_ApprovedById",
                table: "WeeklyReports",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyReports_Users_SubmittedById",
                table: "WeeklyReports",
                column: "SubmittedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequests_Projects_ProjectId",
                table: "WeeklyWorkRequests",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequests_Users_ApprovedById",
                table: "WeeklyWorkRequests",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyWorkRequests_Users_RequestedById",
                table: "WeeklyWorkRequests",
                column: "RequestedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApprovals_Users_ApproverId",
                table: "WorkRequestApprovals",
                column: "ApproverId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApprovals_WorkRequestApprovals_EscalatedFromId",
                table: "WorkRequestApprovals",
                column: "EscalatedFromId",
                principalTable: "WorkRequestApprovals",
                principalColumn: "ApprovalId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestApprovals_WorkRequests_WorkRequestId",
                table: "WorkRequestApprovals",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotifications_Users_RecipientId",
                table: "WorkRequestNotifications",
                column: "RecipientId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotifications_Users_SenderId",
                table: "WorkRequestNotifications",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequestNotifications_WorkRequests_WorkRequestId",
                table: "WorkRequestNotifications",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
