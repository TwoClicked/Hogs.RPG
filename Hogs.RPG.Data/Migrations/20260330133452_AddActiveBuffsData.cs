using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogs.RPG.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveBuffsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveBuffsData",
                table: "Players",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveBuffsData",
                table: "Players");
        }
    }
}
