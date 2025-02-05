using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentProjectsCenterSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIconInDetailsImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconData",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ProjectDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ProjectDetails");

            migrationBuilder.AddColumn<byte[]>(
                name: "IconData",
                table: "ProjectDetails",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
