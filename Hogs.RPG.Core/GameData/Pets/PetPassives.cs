using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Pets
{
    public static class PetPassives
    {
        public static readonly PetPassiveDefinition Lifesteal = new()
        {
            Id = PetPassive.Lifesteal,
            Name = "Lifesteal",
            Description = "Heal 5% of damage dealt"
        };

        public static readonly PetPassiveDefinition DoubleStrike = new()
        {
            Id = PetPassive.DoubleStrike,
            Name = "DoubleStrike",
            Description = "20% chance to deal double damage"
        };

        public static readonly PetPassiveDefinition GuardianShield = new()
        {
            Id = PetPassive.GuardianShield,
            Name = "GuardianShield",
            Description = "Grants +10% defense"
        };

        public static readonly PetPassiveDefinition Thorns = new()
        {
            Id = PetPassive.Thorns,
            Name = "Thorns",
            Description = "Reflect 15% damage"
        };

        public static readonly PetPassiveDefinition Executioner = new()
        {
            Id = PetPassive.Executioner,
            Name = "Executioner",
            Description = "Bonus damage vs low HP enemies"
        };
    }
}