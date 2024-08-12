using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class ProjectDetailCall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallType",
                table: "EmployeeCalls");

            migrationBuilder.AddColumn<string>(
                name: "CallType",
                table: "ProjectDetailCalls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallType",
                table: "ProjectDetailCalls");

            migrationBuilder.AddColumn<string>(
                name: "CallType",
                table: "EmployeeCalls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
