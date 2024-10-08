using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelemarketingControlSystem.Migrations
{
    /// <inheritdoc />
    public partial class DomainModelsInheritFromBaseModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "ProjectTypeMistakeDictionaries",
                newName: "LastUpdatedDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "Projects",
                newName: "LastUpdatedDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "ProjectMistakeDictionaries",
                newName: "LastUpdatedDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "ProjectDetails",
                newName: "LastUpdatedDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdateDate",
                table: "Employees",
                newName: "LastUpdatedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedOn",
                table: "ProjectTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProjectTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProjectTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "ProjectTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "ProjectTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AddedOn",
                table: "CallStatuses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CallStatuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CallStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "CallStatuses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "CallStatuses",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedOn",
                table: "ProjectTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProjectTypes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProjectTypes");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "ProjectTypes");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "ProjectTypes");

            migrationBuilder.DropColumn(
                name: "AddedOn",
                table: "CallStatuses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CallStatuses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CallStatuses");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "CallStatuses");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "CallStatuses");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "ProjectTypeMistakeDictionaries",
                newName: "LastUpdateDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "Projects",
                newName: "LastUpdateDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "ProjectMistakeDictionaries",
                newName: "LastUpdateDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "ProjectDetails",
                newName: "LastUpdateDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedDate",
                table: "Employees",
                newName: "LastUpdateDate");
        }
    }
}
