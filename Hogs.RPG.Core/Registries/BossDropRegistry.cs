using Hogs.RPG.Core.Entities.DungeonObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Registries
{
    public static class BossDropRegistry
    {
        public static readonly Dictionary<string, List<BossLoot>> Drops = new()
        {
            { "gravelmaw_02", BossDropTables.Gravelmaw },
            { "primordial_serpent_03", BossDropTables.PrimordialSerpent },
            { "xerathul_04", BossDropTables.Xerathul },
            { "aurelius_01", BossDropTables.Aurelius },
            { "click_punisher_07", BossDropTables.Punisher},
            { "two_tier_tyr_05", BossDropTables.Tyr },
            { "king_thorlak_06", BossDropTables.Thorlak },
            { "gullveig_huld_08", BossDropTables.Gullveig }
        };
    }
}
