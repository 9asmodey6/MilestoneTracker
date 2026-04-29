using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilestoneTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManyToManyChildParentConn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_children_parents_parent_id",
                table: "children");

            migrationBuilder.DropIndex(
                name: "ix_children_parent_id",
                table: "children");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "children");

            migrationBuilder.CreateTable(
                name: "access_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    child_id = table.Column<int>(type: "integer", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false),
                    token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    used_by_parent_id = table.Column<int>(type: "integer", nullable: true),
                    max_uses = table.Column<int>(type: "integer", nullable: false),
                    current_uses = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_access_tokens_children_child_id",
                        column: x => x.child_id,
                        principalTable: "children",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_access_tokens_parents_creator_id",
                        column: x => x.creator_id,
                        principalTable: "parents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_access_tokens_parents_used_by_parent_id",
                        column: x => x.used_by_parent_id,
                        principalTable: "parents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "child_parent",
                columns: table => new
                {
                    children_id = table.Column<int>(type: "integer", nullable: false),
                    parents_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_child_parent", x => new { x.children_id, x.parents_id });
                    table.ForeignKey(
                        name: "fk_child_parent_children_children_id",
                        column: x => x.children_id,
                        principalTable: "children",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_child_parent_parents_parents_id",
                        column: x => x.parents_id,
                        principalTable: "parents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_access_tokens_child_id",
                table: "access_tokens",
                column: "child_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_tokens_creator_id",
                table: "access_tokens",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "ix_access_tokens_is_used",
                table: "access_tokens",
                column: "is_used");

            migrationBuilder.CreateIndex(
                name: "ix_access_tokens_token",
                table: "access_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_access_tokens_used_by_parent_id",
                table: "access_tokens",
                column: "used_by_parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_child_parent_parents_id",
                table: "child_parent",
                column: "parents_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_tokens");

            migrationBuilder.DropTable(
                name: "child_parent");

            migrationBuilder.AddColumn<int>(
                name: "parent_id",
                table: "children",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_children_parent_id",
                table: "children",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_children_parents_parent_id",
                table: "children",
                column: "parent_id",
                principalTable: "parents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
