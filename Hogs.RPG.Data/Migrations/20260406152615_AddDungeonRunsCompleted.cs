using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogs.RPG.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDungeonRunsCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DungeonRunsCompleted",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Players_DiscordId",
                table: "Players",
                column: "DiscordId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPets_DiscordId",
                table: "PlayerPets",
                column: "DiscordId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerPets_Players_DiscordId",
                table: "PlayerPets",
                column: "DiscordId",
                principalTable: "Players",
                principalColumn: "DiscordId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerPets_Players_DiscordId",
                table: "PlayerPets");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Players_DiscordId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_PlayerPets_DiscordId",
                table: "PlayerPets");

            migrationBuilder.DropColumn(
                name: "DungeonRunsCompleted",
                table: "Players");
        }
    }
}
