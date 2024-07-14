using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCallStatuesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CallStatusId",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CallStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetails_CallStatusId",
                table: "ProjectDetails",
                column: "CallStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_CallStatuses_CallStatusId",
                table: "ProjectDetails",
                column: "CallStatusId",
                principalTable: "CallStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_CallStatuses_CallStatusId",
                table: "ProjectDetails");

            migrationBuilder.DropTable(
                name: "CallStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDetails_CallStatusId",
                table: "ProjectDetails");

            migrationBuilder.AlterColumn<int>(
                name: "CallStatusId",
                table: "ProjectDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
