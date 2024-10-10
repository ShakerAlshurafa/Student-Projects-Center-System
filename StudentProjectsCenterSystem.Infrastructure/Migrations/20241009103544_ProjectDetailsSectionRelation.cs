using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProjectDetailsSectionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectDetailsSectionId",
                table: "ProjectDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails",
                column: "ProjectDetailsSectionId",
                principalTable: "ProjectDetailsSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectDetailsSectionId",
                table: "ProjectDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails",
                column: "ProjectDetailsSectionId",
                principalTable: "ProjectDetailsSections",
                principalColumn: "Id");
        }
    }
}
