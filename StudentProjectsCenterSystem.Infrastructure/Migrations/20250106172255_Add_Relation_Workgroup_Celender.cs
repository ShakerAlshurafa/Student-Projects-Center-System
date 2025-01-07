using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Relation_Workgroup_Celender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkgroupId",
                table: "Celenders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Celenders_WorkgroupId",
                table: "Celenders",
                column: "WorkgroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Celenders_Workgroups_WorkgroupId",
                table: "Celenders",
                column: "WorkgroupId",
                principalTable: "Workgroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Celenders_Workgroups_WorkgroupId",
                table: "Celenders");

            migrationBuilder.DropIndex(
                name: "IX_Celenders_WorkgroupId",
                table: "Celenders");

            migrationBuilder.DropColumn(
                name: "WorkgroupId",
                table: "Celenders");
        }
    }
}
