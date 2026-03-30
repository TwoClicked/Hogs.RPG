using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogs.RPG.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastHuntProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastHunt",
                table: "Players");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastHunt",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
