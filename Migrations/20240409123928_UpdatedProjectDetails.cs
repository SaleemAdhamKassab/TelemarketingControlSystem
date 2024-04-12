using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedProjectDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallStatus",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "Generation",
                table: "ProjectDetails");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Projects",
                newName: "TypeId");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "ProjectDetails",
                newName: "LineTypeId");

            migrationBuilder.RenameColumn(
                name: "LineType",
                table: "ProjectDetails",
                newName: "GenerationId");

            migrationBuilder.AddColumn<int>(
                name: "CallStatusId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallStatusId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "ProjectDetails");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Projects",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "LineTypeId",
                table: "ProjectDetails",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "GenerationId",
                table: "ProjectDetails",
                newName: "LineType");

            migrationBuilder.AddColumn<int>(
                name: "CallStatus",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "City",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Generation",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
