using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class Tier2Pets
    {
        // =========================
        // ⚔️ ATTACK PET
        // Drops from: Blazewing's Gorge (Pet Dungeon)
        // =========================
        public static readonly PetDefinition BlazefangRaptor = new()
        {
            Id = "blazefang_raptor",
            Name = "Blazefang Raptor",
            Icon = "🦅",
            BaseAttack = 10,
            BaseDefense = 2,
            BaseHealth = 6,
            Scaling = 0.8f,
        };

        // =========================
        // 🛡️ DEFENSE PET
        // Drops from: Stonehall Depths (Pet Dungeon)
        // =========================
        public static readonly PetDefinition IronshellBoar = new()
        {
            Id = "ironshell_boar",
            Name = "Ironshell Boar",
            Icon = "🦏",
            BaseAttack = 2,
            BaseDefense = 10,
            BaseHealth = 12,
            Scaling = 0.8f,
        };

        // =========================
        // ❤️ HEALTH PET
        // Drops from: Drowned Archives (Pet Dungeon)
        // =========================
        public static readonly PetDefinition TidecallSerpent = new()
        {
            Id = "tidecall_serpent",
            Name = "Tidecall Serpent",
            Icon = "🐍",
            BaseAttack = 3,
            BaseDefense = 4,
            BaseHealth = 22,
            Scaling = 0.8f,
        };
    }
}
