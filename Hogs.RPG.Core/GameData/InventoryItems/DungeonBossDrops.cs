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
        // =========================
        // LEVEL 10 - FANCULO
        // =========================
        public static readonly ItemDefinition FanculoHelmItem = new()
        {
            Id = "fanculo_helm",
            Name = "Fanculo Horned Helm",
            Icon = "<:fanculo_helm:1485380941416497294>",
            Type = "Equipment",
            Description = "A battle-worn helm infused with chaotic Viking energy."
        };

        // =========================
        // LEVEL 15 - HROTHGAR
        // =========================
        public static readonly ItemDefinition HrothgarRingItem = new()
        {
            Id = "hrothgar_ring",
            Name = "Hrothgar's Frostbound Ring",
            Icon = "💍(Sally will make)", // replace with custom emoji later
            Type = "Equipment",
            Description = "A frozen band that pulses with the lifesteal power of the Frost-Bound Jarl."
        };

        // =========================
        // LEVEL 20 - LUMINARA
        // =========================
        public static readonly ItemDefinition LuminaraAmuletItem = new()
        {
            Id = "luminara_amulet",
            Name = "Luminara's Moonlit Amulet",
            Icon = "📿(Sally will make)", // replace later
            Type = "Equipment",
            Description = "A glowing charm radiating deceptive beauty, masking a sinister, protective aura."
        };

        // =========================
        // LEVEL 25 - THORKELL
        // =========================
        public static readonly ItemDefinition ThorkellBootsItem = new()
        {
            Id = "thorkell_boots",
            Name = "Boots of the Warborn",
            Icon = "👢(Sally will make)", // replace later
            Type = "Equipment",
            Description = "Heavy war boots infused with divine fury, allowing the wearer to crush through all resistance."
        };
    }
}