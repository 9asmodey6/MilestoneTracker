using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilestoneTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChildSoftDeleteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "deleted_by",
                table: "milestones",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "children",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "deleted_by",
                table: "children",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "children",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "children");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "children");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "children");

            migrationBuilder.AlterColumn<int>(
                name: "deleted_by",
                table: "milestones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
