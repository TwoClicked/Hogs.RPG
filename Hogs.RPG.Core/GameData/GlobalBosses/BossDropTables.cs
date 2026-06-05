using Hogs.RPG.Core.Entities.DungeonObjects;
using System.Collections.Generic;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class BossDropTables
    {
        public static List<BossLoot> Gravelmaw = new()
        {
            new BossLoot
            {
                ItemId = "gravelmaw_shield",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> PrimordialSerpent = new()
        {
            new BossLoot
            {
                ItemId = "serpent_gloves",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Xerathul = new()
        {
            new BossLoot
            {
                ItemId = "xerathul_armor",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Aurelius = new()
        {
            new BossLoot
            {
                ItemId = "aurelius_sword",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Punisher = new()
        {
            new BossLoot
            {
                ItemId = "punisher_ring",
                DropChance = 1, // slightly higher (daily boss but stronger)
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Tyr = new()
        {
            new BossLoot
            {
                ItemId = "tyr_helm",
                DropChance = 1, // weekly boss
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Thorlak = new()
        {
            new BossLoot
            {
                ItemId = "thorlak_leggings",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Gullveig = new()
        {
            new BossLoot
            {
                ItemId = "gullveig_amulet",
                DropChance = 1,
                MinAmount = 1,
                MaxAmount = 1
            }
        };
    }
}