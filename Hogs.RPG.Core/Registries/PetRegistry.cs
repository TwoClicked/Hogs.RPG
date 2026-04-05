using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Pets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Registries
{
    public static class PetRegistry
    {
        public static readonly Dictionary<string, PetDefinition> All = new()
        {

            //Start pet
            { StarterPets.VerdantWisp.Id, StarterPets.VerdantWisp },

            //Dungeon pets
            { DungeonPets.IronhideBoar.Id, DungeonPets.IronhideBoar },
            { DungeonPets.EmberfangLynx.Id, DungeonPets.EmberfangLynx },
            { DungeonPets.StoneheartTortoise.Id, DungeonPets.StoneheartTortoise },

            //Raid pets
            { RaidPets.AetherionDrake.Id, RaidPets.AetherionDrake }
        };
    }
}
