using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddSignalRNotificationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestNotification",
                table: "WorkRequestNotification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestApproval",
                table: "WorkRequestApproval");

            migrationBuilder.RenameTable(
                name: "WorkRequestNotification",
                newName: "WorkRequestNotifications");

            migrationBuilder.RenameTable(
                name: "WorkRequestApproval",
                newName: "WorkRequestApprovals");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestNotifications",
                table: "WorkRequestNotifications",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestApprovals",
                table: "WorkRequestApprovals",
                column: "ApprovalId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestNotifications",
                table: "WorkRequestNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkRequestApprovals",
                table: "WorkRequestApprovals");

            migrationBuilder.RenameTable(
                name: "WorkRequestNotifications",
                newName: "WorkRequestNotification");

            migrationBuilder.RenameTable(
                name: "WorkRequestApprovals",
                newName: "WorkRequestApproval");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestNotification",
                table: "WorkRequestNotification",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkRequestApproval",
                table: "WorkRequestApproval",
                column: "ApprovalId");

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
    }
}
