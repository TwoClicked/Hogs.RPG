using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class DungeonPets
    {
        public static readonly PetDefinition IronhideBoar = new()
        {
            Id = "ironhide_boar",
            Name = "Ironhide Boar",
            Icon = "🐗",
            BaseAttack = 1,
            BaseDefense = 4,
            BaseHealth = 8,
            Scaling = 0.5f,
        };

        public static readonly PetDefinition EmberfangLynx = new()
        {
            Id = "emberfang_lynx",
            Name = "Emberfang Lynx",
            Icon = "🔥",
            BaseAttack = 5,
            BaseDefense = 1,
            BaseHealth = 5,
            Scaling = 0.5f,
        };

        public static readonly PetDefinition StoneheartTortoise = new()
        {
            Id = "stoneheart_tortoise",
            Name = "Stoneheart Tortoise",
            Icon = "🐢",
            BaseAttack = 1,
            BaseDefense = 2,
            BaseHealth = 15,
            Scaling = 0.5f,
        };
    }
}
