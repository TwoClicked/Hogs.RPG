using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class Tier3Pet
    {
        // =========================
        // 🐉 PRIMAL CHIMERA
        // Evolved from: BlazefangRaptor + IronshellBoar + TidecallSerpent
        // =========================
        public static readonly PetDefinition Capytara = new ()
        {
            Id = "capytara",
            Name = "Capytara",
            Icon = "<:CapyTara:1500638781110354113>",
            BaseAttack = 15,
            BaseDefense = 15,
            BaseHealth = 35,
            Scaling = 1.5f,
            Tier = 3
        };
    }
}
