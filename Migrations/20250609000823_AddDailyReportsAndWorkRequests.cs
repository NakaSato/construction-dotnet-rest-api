using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyReportsAndWorkRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DailyReportId",
                table: "ImageMetadata",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkRequestId",
                table: "ImageMetadata",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DailyReports",
                columns: table => new
                {
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GeneralNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WeatherCondition = table.Column<string>(type: "text", nullable: true),
                    TemperatureHigh = table.Column<double>(type: "double precision", nullable: true),
                    TemperatureLow = table.Column<double>(type: "double precision", nullable: true),
                    Humidity = table.Column<int>(type: "integer", nullable: true),
                    WindSpeed = table.Column<double>(type: "double precision", nullable: true),
                    TotalWorkHours = table.Column<int>(type: "integer", nullable: false),
                    PersonnelOnSite = table.Column<int>(type: "integer", nullable: false),
                    SafetyIncidents = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    QualityIssues = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WorkAccomplished = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WorkPlanned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Issues = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyReports", x => x.DailyReportId);
                    table.ForeignKey(
                        name: "FK_DailyReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyReports_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequests",
                columns: table => new
                {
                    WorkRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric", nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric", nullable: true),
                    EstimatedHours = table.Column<double>(type: "double precision", nullable: true),
                    ActualHours = table.Column<double>(type: "double precision", nullable: true),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequests", x => x.WorkRequestId);
                    table.ForeignKey(
                        name: "FK_WorkRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkRequests_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentLogs",
                columns: table => new
                {
                    EquipmentLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HoursUsed = table.Column<double>(type: "double precision", nullable: false),
                    OperatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaintenanceRequired = table.Column<bool>(type: "boolean", nullable: false),
                    MaintenanceNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Issues = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentLogs", x => x.EquipmentLogId);
                    table.ForeignKey(
                        name: "FK_EquipmentLogs_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "DailyReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialUsages",
                columns: table => new
                {
                    MaterialUsageId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuantityUsed = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cost = table.Column<decimal>(type: "numeric", nullable: true),
                    Supplier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialUsages", x => x.MaterialUsageId);
                    table.ForeignKey(
                        name: "FK_MaterialUsages_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "DailyReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelLogs",
                columns: table => new
                {
                    PersonnelLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HoursWorked = table.Column<double>(type: "double precision", nullable: false),
                    Position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelLogs", x => x.PersonnelLogId);
                    table.ForeignKey(
                        name: "FK_PersonnelLogs_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "DailyReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnelLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkProgressItems",
                columns: table => new
                {
                    WorkProgressItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    HoursWorked = table.Column<double>(type: "double precision", nullable: false),
                    PercentageComplete = table.Column<int>(type: "integer", nullable: false),
                    WorkersAssigned = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Issues = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NextSteps = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkProgressItems", x => x.WorkProgressItemId);
                    table.ForeignKey(
                        name: "FK_WorkProgressItems_DailyReports_DailyReportId",
                        column: x => x.DailyReportId,
                        principalTable: "DailyReports",
                        principalColumn: "DailyReportId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkProgressItems_ProjectTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "ProjectTasks",
                        principalColumn: "TaskId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequestComments",
                columns: table => new
                {
                    WorkRequestCommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequestComments", x => x.WorkRequestCommentId);
                    table.ForeignKey(
                        name: "FK_WorkRequestComments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkRequestComments_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "WorkRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequestTasks",
                columns: table => new
                {
                    WorkRequestTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedHours = table.Column<double>(type: "double precision", nullable: true),
                    ActualHours = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequestTasks", x => x.WorkRequestTaskId);
                    table.ForeignKey(
                        name: "FK_WorkRequestTasks_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkRequestTasks_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "WorkRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_DailyReportId",
                table: "ImageMetadata",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageMetadata_WorkRequestId",
                table: "ImageMetadata",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ProjectId",
                table: "DailyReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_ReporterId",
                table: "DailyReports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyReports_SubmittedByUserId",
                table: "DailyReports",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentLogs_DailyReportId",
                table: "EquipmentLogs",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialUsages_DailyReportId",
                table: "MaterialUsages",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelLogs_DailyReportId",
                table: "PersonnelLogs",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelLogs_UserId",
                table: "PersonnelLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProgressItems_DailyReportId",
                table: "WorkProgressItems",
                column: "DailyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkProgressItems_TaskId",
                table: "WorkProgressItems",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestComments_AuthorId",
                table: "WorkRequestComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestComments_WorkRequestId",
                table: "WorkRequestComments",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_AssignedToId",
                table: "WorkRequests",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_ProjectId",
                table: "WorkRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_RequestedById",
                table: "WorkRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestTasks_AssignedToId",
                table: "WorkRequestTasks",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestTasks_WorkRequestId",
                table: "WorkRequestTasks",
                column: "WorkRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_DailyReports_DailyReportId",
                table: "ImageMetadata",
                column: "DailyReportId",
                principalTable: "DailyReports",
                principalColumn: "DailyReportId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageMetadata_WorkRequests_WorkRequestId",
                table: "ImageMetadata",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "WorkRequestId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_DailyReports_DailyReportId",
                table: "ImageMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageMetadata_WorkRequests_WorkRequestId",
                table: "ImageMetadata");

            migrationBuilder.DropTable(
                name: "EquipmentLogs");

            migrationBuilder.DropTable(
                name: "MaterialUsages");

            migrationBuilder.DropTable(
                name: "PersonnelLogs");

            migrationBuilder.DropTable(
                name: "WorkProgressItems");

            migrationBuilder.DropTable(
                name: "WorkRequestComments");

            migrationBuilder.DropTable(
                name: "WorkRequestTasks");

            migrationBuilder.DropTable(
                name: "DailyReports");

            migrationBuilder.DropTable(
                name: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_ImageMetadata_DailyReportId",
                table: "ImageMetadata");

            migrationBuilder.DropIndex(
                name: "IX_ImageMetadata_WorkRequestId",
                table: "ImageMetadata");

            migrationBuilder.DropColumn(
                name: "DailyReportId",
                table: "ImageMetadata");

            migrationBuilder.DropColumn(
                name: "WorkRequestId",
                table: "ImageMetadata");
        }
    }
}
