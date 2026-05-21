using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Pets
{
    public class Tier2Pets
    {
        // =========================
        // ⚔️ ATTACK PET
        // Drops from: Blazewing's Gorge (Pet Dungeon)
        // =========================
        public static readonly PetDefinition ArmoredCapybara = new ()
        {
            Id = "armored_capybara",
            Name = "Armored capybara",
            Icon = "<:Capy:1500620423614955571>",
            BaseAttack = 10,
            BaseDefense = 2,
            BaseHealth = 6,
            Scaling = 0.8f,
            Tier = 2
        };

        // =========================
        // 🛡️ DEFENSE PET
        // Drops from: Stonehall Depths (Pet Dungeon)
        // =========================
        public static readonly PetDefinition ElTataDeFrog = new()
        {
            Id = "el_tata_de_frog",
            Name = "El Tata de Frog",
            Icon = "<:ElTataDeFrog:1500620538484232416>",
            BaseAttack = 2,
            BaseDefense = 10,
            BaseHealth = 12,
            Scaling = 0.8f,
            Tier = 2
        };

        // =========================
        // ❤️ HEALTH PET
        // Drops from: Drowned Archives (Pet Dungeon)
        // =========================
        public static readonly PetDefinition IceWolf = new()
        {
            Id = "ice_wolf",
            Name = "Ice Wolf",
            Icon = "<:IceWolf:1500620499217023026>",
            BaseAttack = 3,
            BaseDefense = 4,
            BaseHealth = 22,
            Scaling = 0.8f,
            Tier = 2
        };
    }
}
