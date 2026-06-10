using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Enums
{
    public enum RelicBonusType
    {
        // Tank
        DefensePercent,
        MaxHpPercent,

        // DPS
        AttackPercent,
        ExecutionerBonus,
        LifeSteal,
        ConsecutiveHitBonus,

        // Healer
        IncreasedHealPercent,
        ChanceToSavePotion,

        // Universal
        BonusGold,
        BonusPlayerXp,
        BonusPetXp,
        BonusLootRoll
    }
}
