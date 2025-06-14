using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AdminApprovalDate",
                table: "WorkRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdminApproverId",
                table: "WorkRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminComments",
                table: "WorkRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AutoApprovalThreshold",
                table: "WorkRequests",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoApproved",
                table: "WorkRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerApprovalDate",
                table: "WorkRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerApproverId",
                table: "WorkRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerComments",
                table: "WorkRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "WorkRequests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAdminApproval",
                table: "WorkRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresManagerApproval",
                table: "WorkRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedForApprovalDate",
                table: "WorkRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkRequestApprovals",
                columns: table => new
                {
                    ApprovalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: false),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EscalatedFromId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequestApprovals", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_WorkRequestApprovals_Users_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkRequestApprovals_WorkRequestApprovals_EscalatedFromId",
                        column: x => x.EscalatedFromId,
                        principalTable: "WorkRequestApprovals",
                        principalColumn: "ApprovalId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkRequestApprovals_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "WorkRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequestNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EmailTo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmailCc = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequestNotifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_WorkRequestNotifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkRequestNotifications_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkRequestNotifications_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "WorkRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_AdminApproverId",
                table: "WorkRequests",
                column: "AdminApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_ManagerApproverId",
                table: "WorkRequests",
                column: "ManagerApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_Action",
                table: "WorkRequestApprovals",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_ApproverId",
                table: "WorkRequestApprovals",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_CreatedAt",
                table: "WorkRequestApprovals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_EscalatedFromId",
                table: "WorkRequestApprovals",
                column: "EscalatedFromId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_IsActive",
                table: "WorkRequestApprovals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_Level",
                table: "WorkRequestApprovals",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestApprovals_WorkRequestId",
                table: "WorkRequestApprovals",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_CreatedAt",
                table: "WorkRequestNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_ReadAt",
                table: "WorkRequestNotifications",
                column: "ReadAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_RecipientId",
                table: "WorkRequestNotifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_SenderId",
                table: "WorkRequestNotifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_SentAt",
                table: "WorkRequestNotifications",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_Status",
                table: "WorkRequestNotifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_Type",
                table: "WorkRequestNotifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequestNotifications_WorkRequestId",
                table: "WorkRequestNotifications",
                column: "WorkRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_Users_AdminApproverId",
                table: "WorkRequests",
                column: "AdminApproverId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_Users_ManagerApproverId",
                table: "WorkRequests",
                column: "ManagerApproverId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_Users_AdminApproverId",
                table: "WorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_Users_ManagerApproverId",
                table: "WorkRequests");

            migrationBuilder.DropTable(
                name: "WorkRequestApprovals");

            migrationBuilder.DropTable(
                name: "WorkRequestNotifications");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_AdminApproverId",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_ManagerApproverId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "AdminApprovalDate",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "AdminApproverId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "AdminComments",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "AutoApprovalThreshold",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "IsAutoApproved",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "ManagerApprovalDate",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "ManagerApproverId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "ManagerComments",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "RequiresAdminApproval",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "RequiresManagerApproval",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "SubmittedForApprovalDate",
                table: "WorkRequests");
        }
    }
}
