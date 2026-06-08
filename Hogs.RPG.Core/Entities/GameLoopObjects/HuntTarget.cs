using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.GameLoopObjects
{
    public class HuntTarget
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string DropItem { get; set; }

        public int MinXP { get; set; }

        public int MaxXP { get; set; }

        public int MinDrop { get; set; }
        public int MaxDrop { get; set; }

        public int RequiredLevel { get; set; }


        // Seperating the material gathering for Alchemy and Gear, Discord limits the autocomp at 25
        public HuntCategory Category { get; set; } = HuntCategory.Normal;

        public string? RareDropItem { get; set; }

        // Alchemy XP granted when this target is hunted (0 for non-alchemy targets)
        public int AlchemyXpReward { get; set; } = 0;
    }
}
