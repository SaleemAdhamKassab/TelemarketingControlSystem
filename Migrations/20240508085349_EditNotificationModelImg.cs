using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class EditNotificationModelImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_Employees_EmployeeID",
                table: "ProjectDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectID",
                table: "ProjectDetails");

            migrationBuilder.RenameColumn(
                name: "ProjectID",
                table: "ProjectDetails",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "EmployeeID",
                table: "ProjectDetails",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDetails_ProjectID",
                table: "ProjectDetails",
                newName: "IX_ProjectDetails_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDetails_EmployeeID",
                table: "ProjectDetails",
                newName: "IX_ProjectDetails_EmployeeId");

            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_Employees_EmployeeId",
                table: "ProjectDetails",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectId",
                table: "ProjectDetails",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_Employees_EmployeeId",
                table: "ProjectDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "Img",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "ProjectDetails",
                newName: "ProjectID");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "ProjectDetails",
                newName: "EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDetails_ProjectId",
                table: "ProjectDetails",
                newName: "IX_ProjectDetails_ProjectID");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectDetails_EmployeeId",
                table: "ProjectDetails",
                newName: "IX_ProjectDetails_EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_Employees_EmployeeID",
                table: "ProjectDetails",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectID",
                table: "ProjectDetails",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
