using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class Tier3Pet
    {
        // =========================
        // 🐉 PRIMAL CHIMERA
        // Evolved from: BlazefangRaptor + IronshellBoar + TidecallSerpent
        // =========================
        public static readonly PetDefinition MythicalArmoredIceTurtle = new ()
        {
            Id = "mythical_armored_ice_turtle",
            Name = "Mythical Armored Ice Turtle",
            Icon = "<:T3TURTO:1500620463125041202>",
            BaseAttack = 15,
            BaseDefense = 15,
            BaseHealth = 35,
            Scaling = 1.5f,
        };
    }
}
