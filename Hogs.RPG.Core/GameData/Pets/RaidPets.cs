using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Pets
{
    public static class RaidPets
    {
        public static readonly PetDefinition AetherionDrake = new()
        {
            Id = "aetherion_drake",
            Name = "Aetherion, the Worldbound Drake",
            Icon = "🐉",
            BaseAttack = 6,
            BaseDefense = 5,
            BaseHealth = 12,
            Scaling = 1.0f,
        };
    }
}
