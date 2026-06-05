using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.JobObjects
{
    public class SmithingItemDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        // Smithing level required to forge this item
        public int RequiredSmithingLevel { get; set; }

        // Bars consumed per craft (e.g. "bronze_bar" x2)
        public Dictionary<string, int> BarRequirements { get; set; } = new();

        // XP granted when forged
        public int SmithingXpReward { get; set; }

        // Gold the NPC pays per unit
        public int NpcGoldPrice { get; set; }

        // Max units NPC will buy per daily reset
        public int MaxNpcBuysPerDay { get; set; }
    }
}