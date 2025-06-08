using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class LocalDevelopmentUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("a05fb009-b6eb-4fe1-a354-7c1606b035f8"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "RoleId", "Username" },
                values: new object[] { new Guid("5d52b7fa-0b88-4f45-8b72-8d095d9c1fb0"), new DateTime(2025, 6, 8, 7, 12, 53, 52, DateTimeKind.Utc).AddTicks(7150), "admin@example.com", "System Administrator", true, "$2a$11$8hQc4Rf1Z/Ma3gUIL00SWuqcA0Y1NGdlBEhP3kqzMQi1onhvXIlT2", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("5d52b7fa-0b88-4f45-8b72-8d095d9c1fb0"));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "RoleId", "Username" },
                values: new object[] { new Guid("a05fb009-b6eb-4fe1-a354-7c1606b035f8"), new DateTime(2025, 6, 8, 3, 26, 32, 898, DateTimeKind.Utc).AddTicks(1130), "admin@example.com", "System Administrator", true, "$2a$11$eyNQP0dXveyrEDPeSxlI6.FRn0lfVrjNESXPftNTU9Hec09phHv.i", 1, "admin" });
        }
    }
}
