using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddWbsSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WbsTasks",
                columns: table => new
                {
                    WbsId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentWbsId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TaskNameEN = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaskNameTH = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    WeightPercent = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    InstallationArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsTasks", x => x.WbsId);
                    table.ForeignKey(
                        name: "FK_WbsTasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WbsTasks_Users_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WbsTasks_WbsTasks_ParentWbsId",
                        column: x => x.ParentWbsId,
                        principalTable: "WbsTasks",
                        principalColumn: "WbsId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WbsTaskDependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DependentTaskId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrerequisiteTaskId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DependencyType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsTaskDependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WbsTaskDependencies_WbsTasks_DependentTaskId",
                        column: x => x.DependentTaskId,
                        principalTable: "WbsTasks",
                        principalColumn: "WbsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WbsTaskDependencies_WbsTasks_PrerequisiteTaskId",
                        column: x => x.PrerequisiteTaskId,
                        principalTable: "WbsTasks",
                        principalColumn: "WbsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WbsTaskEvidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WbsTaskId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EvidenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WbsTaskEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WbsTaskEvidence_WbsTasks_WbsTaskId",
                        column: x => x.WbsTaskId,
                        principalTable: "WbsTasks",
                        principalColumn: "WbsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WbsTaskDependencies_DependentTaskId_PrerequisiteTaskId",
                table: "WbsTaskDependencies",
                columns: new[] { "DependentTaskId", "PrerequisiteTaskId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WbsTaskDependencies_PrerequisiteTaskId",
                table: "WbsTaskDependencies",
                column: "PrerequisiteTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTaskEvidence_CreatedAt",
                table: "WbsTaskEvidence",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTaskEvidence_EvidenceType",
                table: "WbsTaskEvidence",
                column: "EvidenceType");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTaskEvidence_WbsTaskId",
                table: "WbsTaskEvidence",
                column: "WbsTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTasks_AssignedUserId",
                table: "WbsTasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTasks_InstallationArea",
                table: "WbsTasks",
                column: "InstallationArea");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTasks_ParentWbsId",
                table: "WbsTasks",
                column: "ParentWbsId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTasks_ProjectId",
                table: "WbsTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WbsTasks_Status",
                table: "WbsTasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WbsTaskDependencies");

            migrationBuilder.DropTable(
                name: "WbsTaskEvidence");

            migrationBuilder.DropTable(
                name: "WbsTasks");
        }
    }
}
