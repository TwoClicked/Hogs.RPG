using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Pets
{
    public static class Tier1Pets
    {
        public static readonly PetDefinition VerdantWisp = new()
        {
            Id = "verdant_cat",
            Name = "Verdant Cat",
            Icon = "a:Cat_stare:1383492923999649924",
            BaseAttack = 0,
            BaseDefense = 1,
            BaseHealth = 5,
            Scaling = 0.2f,
        };
    }
}
