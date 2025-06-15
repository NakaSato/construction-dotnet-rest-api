using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddSolarProjectFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionNotes",
                table: "Projects",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionType",
                table: "Projects",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FtsValue",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Inverter125kw",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Inverter40kw",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Inverter60kw",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Inverter80kw",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Projects",
                type: "numeric(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Projects",
                type: "numeric(10,7)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PqmValue",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PvModuleCount",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevenueValue",
                table: "Projects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Team",
                table: "Projects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCapacityKw",
                table: "Projects",
                type: "numeric(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionNotes",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ConnectionType",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "FtsValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Inverter125kw",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Inverter40kw",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Inverter60kw",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Inverter80kw",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PqmValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PvModuleCount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RevenueValue",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Team",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TotalCapacityKw",
                table: "Projects");
        }
    }
}
