using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Pets;
using System.Collections.Generic;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class PetPassiveRegistry
    {
        public static readonly Dictionary<PetPassive, PetPassiveDefinition> All = new()
        {
            { PetPassives.Lifesteal.Id, PetPassives.Lifesteal },
            { PetPassives.DoubleStrike.Id, PetPassives.DoubleStrike },
            { PetPassives.GuardianShield.Id, PetPassives.GuardianShield },
            { PetPassives.Thorns.Id, PetPassives.Thorns },
            { PetPassives.Executioner.Id, PetPassives.Executioner }
        };
    }
}