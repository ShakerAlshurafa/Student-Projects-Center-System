using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTermGroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Terms_TermGroups_TermGroupId",
                table: "Terms");

            migrationBuilder.DropTable(
                name: "TermGroups");

            migrationBuilder.DropIndex(
                name: "IX_Terms_TermGroupId",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "TermGroupId",
                table: "Terms");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Terms",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "Terms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Terms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Terms");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Terms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TermGroupId",
                table: "Terms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TermGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Terms_TermGroupId",
                table: "Terms",
                column: "TermGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Terms_TermGroups_TermGroupId",
                table: "Terms",
                column: "TermGroupId",
                principalTable: "TermGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
