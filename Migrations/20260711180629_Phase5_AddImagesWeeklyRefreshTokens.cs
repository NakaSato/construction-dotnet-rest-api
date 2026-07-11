using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class Phase5_AddImagesWeeklyRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_ImageMetadata_DailyReports_DailyReportId",
                table: "ImageMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_ProjectTasks_TaskId",
                table: "ImageMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_Projects_ProjectId",
                table: "ImageMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_Users_UploadedByUserId",
                table: "ImageMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_WorkRequests_WorkRequestId",
                table: "ImageMetadata");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyWorkRequest",
                table: "WeeklyWorkRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyReport",
                table: "WeeklyReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImageMetadata",
                table: "ImageMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent");

            migrationBuilder.RenameTable(
                name: "WeeklyWorkRequest",
                newName: "WeeklyWorkRequests");

            migrationBuilder.RenameTable(
                name: "WeeklyReport",
                newName: "WeeklyReports");

            migrationBuilder.RenameTable(
                name: "ImageMetadata",
                newName: "Images");

            migrationBuilder.RenameTable(
                name: "CalendarEvent",
                newName: "CalendarEvents");

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
                name: "IX_ImageMetadata_WorkRequestId",
                table: "Images",
                newName: "IX_Images_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageMetadata_UploadedByUserId",
                table: "Images",
                newName: "IX_Images_UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageMetadata_TaskId",
                table: "Images",
                newName: "IX_Images_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageMetadata_ProjectId",
                table: "Images",
                newName: "IX_Images_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageMetadata_DailyReportId",
                table: "Images",
                newName: "IX_Images_DailyReportId");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyWorkRequests",
                table: "WeeklyWorkRequests",
                column: "WeeklyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyReports",
                table: "WeeklyReports",
                column: "WeeklyReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                table: "Images",
                column: "ImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents",
                column: "EventId");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Email", "PasswordHash" },
                values: new object[] { "admin@example.com", "$2a$11$OMrjsU5wl02tYlgxX4g9Eu7RuRDcjM2H4hZvDKOdi8YMDNSbMpuey" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "RoleId", "Username" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "manager@solarprojects.com", "Project Manager", true, "$2a$11$OMrjsU5wl02tYlgxX4g9Eu7RuRDcjM2H4hZvDKOdi8YMDNSbMpuey", 2, "manager" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "engineer@solarprojects.com", "Solar Engineer", true, "$2a$11$OMrjsU5wl02tYlgxX4g9Eu7RuRDcjM2H4hZvDKOdi8YMDNSbMpuey", 3, "engineer" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "viewer@solarprojects.com", "Stakeholder Viewer", true, "$2a$11$OMrjsU5wl02tYlgxX4g9Eu7RuRDcjM2H4hZvDKOdi8YMDNSbMpuey", 4, "viewer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

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
                name: "FK_Images_DailyReports_DailyReportId",
                table: "Images",
                column: "DailyReportId",
                principalTable: "DailyReports",
                principalColumn: "DailyReportId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ProjectTasks_TaskId",
                table: "Images",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Projects_ProjectId",
                table: "Images",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Users_UploadedByUserId",
                table: "Images",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_WorkRequests_WorkRequestId",
                table: "Images",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.SetNull);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_Images_DailyReports_DailyReportId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_ProjectTasks_TaskId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Projects_ProjectId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Users_UploadedByUserId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_WorkRequests_WorkRequestId",
                table: "Images");

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

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyWorkRequests",
                table: "WeeklyWorkRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WeeklyReports",
                table: "WeeklyReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                table: "Images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CalendarEvents",
                table: "CalendarEvents");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.RenameTable(
                name: "WeeklyWorkRequests",
                newName: "WeeklyWorkRequest");

            migrationBuilder.RenameTable(
                name: "WeeklyReports",
                newName: "WeeklyReport");

            migrationBuilder.RenameTable(
                name: "Images",
                newName: "ImageMetadata");

            migrationBuilder.RenameTable(
                name: "CalendarEvents",
                newName: "CalendarEvent");

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
                name: "IX_Images_WorkRequestId",
                table: "ImageMetadata",
                newName: "IX_ImageMetadata_WorkRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_UploadedByUserId",
                table: "ImageMetadata",
                newName: "IX_ImageMetadata_UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_TaskId",
                table: "ImageMetadata",
                newName: "IX_ImageMetadata_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_ProjectId",
                table: "ImageMetadata",
                newName: "IX_ImageMetadata_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Images_DailyReportId",
                table: "ImageMetadata",
                newName: "IX_ImageMetadata_DailyReportId");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyWorkRequest",
                table: "WeeklyWorkRequest",
                column: "WeeklyRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WeeklyReport",
                table: "WeeklyReport",
                column: "WeeklyReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImageMetadata",
                table: "ImageMetadata",
                column: "ImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CalendarEvent",
                table: "CalendarEvent",
                column: "EventId");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "Email", "PasswordHash" },
                values: new object[] { "admin@solarprojects.com", "$2a$11$rqiU3ov8V4yGqQpzYpKqY.Y5p3YmXFKJZk8GvOqHqOqh4v7/7gzMu" });

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
                name: "FK_ImageMetadata_DailyReports_DailyReportId",
                table: "ImageMetadata",
                column: "DailyReportId",
                principalTable: "DailyReports",
                principalColumn: "DailyReportId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_ProjectTasks_TaskId",
                table: "ImageMetadata",
                column: "TaskId",
                principalTable: "ProjectTasks",
                principalColumn: "TaskId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_Projects_ProjectId",
                table: "ImageMetadata",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_Users_UploadedByUserId",
                table: "ImageMetadata",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_WorkRequests_WorkRequestId",
                table: "ImageMetadata",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.SetNull);

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
        }
    }
}
