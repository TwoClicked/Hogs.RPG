using Hogs.RPG.Core.Entities;
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
                DropChance = 5,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> PrimordialSerpent = new()
        {
            new BossLoot
            {
                ItemId = "serpent_amulet",
                DropChance = 5,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Xerathul = new()
        {
            new BossLoot
            {
                ItemId = "xerathul_armor",
                DropChance = 5,
                MinAmount = 1,
                MaxAmount = 1
            }
        };

        public static List<BossLoot> Aurelius = new()
        {
            new BossLoot
            {
                ItemId = "aurelius_sword",
                DropChance = 5,
                MinAmount = 1,
                MaxAmount = 1
            }
        };
    }
}