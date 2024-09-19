using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectsTableAddRemDaysAndIsClosedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ExpectedRemainingDays",
                table: "Projects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedRemainingDays",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Projects");
        }
    }
}
