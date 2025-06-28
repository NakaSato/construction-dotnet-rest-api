using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetColumnToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTasks_PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "PhaseId1",
                table: "ProjectTasks");

            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "Projects",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "Projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedById",
                table: "ProjectMilestones",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "ProjectMilestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ProjectMilestones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDate",
                table: "ProjectMilestones",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ChallengesFaced",
                table: "ProgressReports",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPercentage",
                table: "ProgressReports",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NextSteps",
                table: "ProgressReports",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportContent",
                table: "ProgressReports",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportTitle",
                table: "ProgressReports",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProgressReports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Budget",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompletedById",
                table: "ProjectMilestones");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ProjectMilestones");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProjectMilestones");

            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "ProjectMilestones");

            migrationBuilder.DropColumn(
                name: "ChallengesFaced",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "NextSteps",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "ReportContent",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "ReportTitle",
                table: "ProgressReports");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProgressReports");

            migrationBuilder.AddColumn<Guid>(
                name: "PhaseId1",
                table: "ProjectTasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTasks_PhaseId1",
                table: "ProjectTasks",
                column: "PhaseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectTasks_ProjectPhases_PhaseId1",
                table: "ProjectTasks",
                column: "PhaseId1",
                principalTable: "ProjectPhases",
                principalColumn: "PhaseId");
        }
    }
}
