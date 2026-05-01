using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilestoneTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeletionFieldsForMilestone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "milestones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deleted_by",
                table: "milestones",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "milestones",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "milestones");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "milestones");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "milestones");
        }
    }
}
