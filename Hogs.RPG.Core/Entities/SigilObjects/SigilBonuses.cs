using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Entities.SigilObjects
{
    public class SigilBonuses
    {
        // Stat bonuses (applied everywhere, same as RelicBonuses)
        public float AttackPercent { get; set; } = 0f;
        public float DefensePercent { get; set; } = 0f;
        public float MaxHpPercent { get; set; } = 0f;

        // Reward bonuses
        public float BonusGoldPercent { get; set; } = 0f;
        public float BonusPlayerXpPercent { get; set; } = 0f;

        // Combat bonus
        public float LifeStealPercent { get; set; } = 0f;
    }
}
