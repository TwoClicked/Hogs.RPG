using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class Tier3Pet
    {
        // =========================
        // 🐉 PRIMAL CHIMERA
        // Evolved from: BlazefangRaptor + IronshellBoar + TidecallSerpent
        // =========================
        public static readonly PetDefinition PrimalChimera = new()
        {
            Id = "primal_chimera",
            Name = "Primal Chimera",
            Icon = "🐉",
            BaseAttack = 15,
            BaseDefense = 15,
            BaseHealth = 35,
            Scaling = 1.5f,
        };
    }
}
