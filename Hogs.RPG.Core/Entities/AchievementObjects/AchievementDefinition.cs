using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.AchievementObjects
{
    public class AchievementDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Category { get; set; }

        // The condition check — evaluated by AchievementService
        public Func<AchievementContext, bool> Condition { get; set; }

        // Gold reward per achievement — always 250g
        public int GoldReward { get; set; } = 250;

        // Can this be retroactively awarded from existing player data
        public bool IsRetroactiveEligible { get; set; } = true;
    }
}
