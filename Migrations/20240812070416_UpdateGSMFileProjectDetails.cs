using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGSMFileProjectDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "GenerationId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "LineTypeId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Generation",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineType",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "Generation",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "LineType",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GenerationId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineTypeId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);
        }
    }
}
