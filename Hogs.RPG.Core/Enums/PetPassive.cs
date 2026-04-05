using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.Enums
{
    public enum PetPassive
    {
        DoubleStrike,        // Chance to hit twice
        Lifesteal,           // Heal % of damage dealt
        Thorns,              // Reflect damage when hit
        Executioner,         // Bonus damage vs low HP enemies
        GuardianShield       // Chance to reduce incoming damage
    }
}
