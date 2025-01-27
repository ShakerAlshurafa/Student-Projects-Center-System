using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditWorkgroupMessageRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages",
                column: "WorkgroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_WorkgroupId",
                table: "Messages",
                column: "WorkgroupId",
                unique: true,
                filter: "[WorkgroupId] IS NOT NULL");
        }
    }
}
