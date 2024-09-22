using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeWorkingHoursTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeWorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkingHours = table.Column<double>(type: "float", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SegmentName = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkingHours_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkingHours_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkingHours_Segments_SegmentName",
                        column: x => x.SegmentName,
                        principalTable: "Segments",
                        principalColumn: "Name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_EmployeeId",
                table: "EmployeeWorkingHours",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_ProjectId",
                table: "EmployeeWorkingHours",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_SegmentName",
                table: "EmployeeWorkingHours",
                column: "SegmentName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeWorkingHours");
        }
    }
}
