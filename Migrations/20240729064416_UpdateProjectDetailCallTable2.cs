using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectDetailCallTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetailCalls_Employees_EmployeeId",
                table: "ProjectDetailCalls");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDetailCalls_EmployeeId",
                table: "ProjectDetailCalls");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "ProjectDetailCalls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "ProjectDetailCalls",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetailCalls_EmployeeId",
                table: "ProjectDetailCalls",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetailCalls_Employees_EmployeeId",
                table: "ProjectDetailCalls",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
