using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateWorkgroupFilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubmittedFilePath",
                table: "Tasks",
                newName: "SubmittedBy");

            migrationBuilder.RenameColumn(
                name: "QuestionFilePath",
                table: "Tasks",
                newName: "LastUpdateBy");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "WorkgroupFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    WorkgroupTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkgroupFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkgroupFiles_Tasks_WorkgroupTaskId",
                        column: x => x.WorkgroupTaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkgroupFiles_WorkgroupTaskId",
                table: "WorkgroupFiles",
                column: "WorkgroupTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkgroupFiles");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "SubmittedBy",
                table: "Tasks",
                newName: "SubmittedFilePath");

            migrationBuilder.RenameColumn(
                name: "LastUpdateBy",
                table: "Tasks",
                newName: "QuestionFilePath");
        }
    }
}
