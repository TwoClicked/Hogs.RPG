using Hogs.RPG.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public class DungeonBossDrops
    {
        //Fanculo dungeon drops (Level 25)
        public static readonly ItemDefinition FanculoHelmItem = new()
        {
            Id = "fanculo_helm",
            Name = "Fanculo Horned Helm",
            Icon = "<:fanculo_helm:1485380941416497294>",
            Type = "Equipment",
            Description = "A battle-worn helm infused with chaotic Viking energy."
        };
    }
}
