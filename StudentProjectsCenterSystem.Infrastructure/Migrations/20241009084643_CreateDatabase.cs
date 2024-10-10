using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectId",
                table: "ProjectDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDetails_ProjectId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ProjectDetailsSections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetailsSections_ProjectId",
                table: "ProjectDetailsSections",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetailsSections_Projects_ProjectId",
                table: "ProjectDetailsSections",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetailsSections_Projects_ProjectId",
                table: "ProjectDetailsSections");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDetailsSections_ProjectId",
                table: "ProjectDetailsSections");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectDetailsSections");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetails_ProjectId",
                table: "ProjectDetails",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_Projects_ProjectId",
                table: "ProjectDetails",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
