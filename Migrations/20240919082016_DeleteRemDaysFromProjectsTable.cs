using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class DeleteRemDaysFromProjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedRemainingDays",
                table: "Projects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ExpectedRemainingDays",
                table: "Projects",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
