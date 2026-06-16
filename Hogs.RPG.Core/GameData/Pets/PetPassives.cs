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
            Description = "Heal 10% of damage dealt"
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
            Description = "15% chance to reduce incoming damage by 30%"
        };
        public static readonly PetPassiveDefinition Thorns = new()
        {
            Id = PetPassive.Thorns,
            Name = "Thorns",
            Description = "Reflect 15% of damage taken + 5 flat"
        };
        public static readonly PetPassiveDefinition Executioner = new()
        {
            Id = PetPassive.Executioner,
            Name = "Executioner",
            Description = "+25% damage vs enemies below 30% HP"
        };
    }
}