using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateWorkgroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<int>(
                name: "WorkgroupId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectDetailsSectionId",
                table: "ProjectDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectDetailsSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDetailsSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProjects",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjects", x => new { x.UserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_UserProjects_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workgroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workgroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_WorkgroupId",
                table: "Projects",
                column: "WorkgroupId",
                unique: true,
                filter: "[WorkgroupId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDetails_ProjectDetailsSectionId",
                table: "ProjectDetails",
                column: "ProjectDetailsSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_ProjectId",
                table: "UserProjects",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails",
                column: "ProjectDetailsSectionId",
                principalTable: "ProjectDetailsSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Workgroups_WorkgroupId",
                table: "Projects",
                column: "WorkgroupId",
                principalTable: "Workgroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDetails_ProjectDetailsSections_ProjectDetailsSectionId",
                table: "ProjectDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Workgroups_WorkgroupId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "ProjectDetailsSections");

            migrationBuilder.DropTable(
                name: "UserProjects");

            migrationBuilder.DropTable(
                name: "Workgroups");

            migrationBuilder.DropIndex(
                name: "IX_Projects_WorkgroupId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDetails_ProjectDetailsSectionId",
                table: "ProjectDetails");

            migrationBuilder.DropColumn(
                name: "WorkgroupId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectDetailsSectionId",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
