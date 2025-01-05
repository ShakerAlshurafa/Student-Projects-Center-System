using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelationWorkgroupAndMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkgroupName",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "WorkgroupId",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages",
                column: "WorkgroupId",
                unique: true,
                filter: "[WorkgroupId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Workgroups_WorkgroupId",
                table: "Messages",
                column: "WorkgroupId",
                principalTable: "Workgroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Workgroups_WorkgroupId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "WorkgroupId",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "WorkgroupName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
