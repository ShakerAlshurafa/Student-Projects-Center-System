using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkgroupTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Tasks",
                newName: "SubmittedFilePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Tasks",
                newName: "QuestionFilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubmittedFilePath",
                table: "Tasks",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "QuestionFilePath",
                table: "Tasks",
                newName: "FileName");
        }
    }
}
