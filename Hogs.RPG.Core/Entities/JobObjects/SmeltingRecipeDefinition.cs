using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.JobObjects
{
    public class SmeltingRecipeDefinition
    {
        public string BarId { get; set; }
        public string BarName { get; set; }

        // Smithing level required to smelt this bar
        public int RequiredSmithingLevel { get; set; }

        // Ores consumed per smelt (e.g. "iron_ore" x2, "coal" x1)
        public Dictionary<string, int> OreRequirements { get; set; } = new();
    }
}