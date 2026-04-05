using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hogs.RPG.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BossSpawnStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossSpawnStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    PetId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    XP = table.Column<int>(type: "int", nullable: false),
                    IsEquipped = table.Column<bool>(type: "bit", nullable: false),
                    Passive1 = table.Column<int>(type: "int", nullable: true),
                    Passive2 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscordId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    XP = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    Attack = table.Column<int>(type: "int", nullable: false),
                    Defense = table.Column<int>(type: "int", nullable: false),
                    Health = table.Column<int>(type: "int", nullable: false),
                    MaxHealth = table.Column<int>(type: "int", nullable: false),
                    HunterStamina = table.Column<int>(type: "int", nullable: false),
                    LastHunterStaminaUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MainHand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OffHand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Helmet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Legs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gloves = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Boots = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ring = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amulet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActiveBuffsData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutoUseXpPotions = table.Column<bool>(type: "bit", nullable: false),
                    Energy = table.Column<int>(type: "int", nullable: false),
                    LastEnergyUpdate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastBossAttack = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_DiscordId_ItemId",
                table: "InventoryItems",
                columns: new[] { "DiscordId", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BossSpawnStates");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "PlayerPets");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
