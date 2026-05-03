using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.DungeonBosses
{
    public static class PetDungeonBosses
    {
        // =========================
        // ⚔️ ATTACK PET BOSS
        // Blazewing's Gorge — Lv 15
        // =========================
        public static readonly DungeonBossDefinition ArmoredCapybara = new()
        {
            Id = "armored_capybara",
            Name = "Armored capybara",
            Description = "A colossal Capybara",

            MaxHealth = 1500,
            Attack = 175,
            Defense = 25,

            ImageUrl = "https://cdn.discordapp.com/attachments/1490322685446717543/1490414348538220564/34beS7k6SoqZvyicpbKWyg.webp?ex=69f8e207&is=69f79087&hm=ec142f6086768b0670a2c68204903e503682b90c9c2ba9fe4899a258322651b9",

            BehaviorId = "rage",
            AbilitiesText = "Enters rage at low HP, doubling damage temporarily.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "armored_capybara", ChancePercent = 3 }
            }
        };

        // =========================
        // 🛡️ DEFENSE PET BOSS
        // Stonehall Depths — Lv 20
        // =========================
        public static readonly DungeonBossDefinition ElTataDeFrog = new ()
        {
            Id = "el_tata_de_frog",
            Name = "El Tata de frog",
            Description = "Rare Australian mythical creature",

            MaxHealth = 2300,
            Attack = 220,
            Defense = 55,

            ImageUrl = "https://cdn.discordapp.com/attachments/1490322685446717543/1490496011917987982/IMG_4313.jpg?ex=69f88555&is=69f733d5&hm=c5dc80a02af1bff60219c86dae00bfd09fd84e6b30ef2566a06d762b67d10533",

            BehaviorId = "crushing_blow",
            AbilitiesText = "Occasionally unleashes a devastating blow ignoring part of the player's defense.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "el_tata_de_frog", ChancePercent = 3 }
            }
        };

        // =========================
        // ❤️ HEALTH PET BOSS
        // Drowned Archives — Lv 25
        // =========================
        public static readonly DungeonBossDefinition IceWolf = new()
        {
            Id = "ice_wolf",
            Name = "Ice Wolf",
            Description = "An Ice Wolf that guards the flooded ruins of an ancient library. Feeds on the drowned.",

            MaxHealth = 3400,
            Attack = 295,
            Defense = 58,

            ImageUrl = "https://cdn.discordapp.com/attachments/1490322685446717543/1490429711480914103/5B83C465-A896-4784-B9AE-491067756886.png?ex=69f8f056&is=69f79ed6&hm=f1d13eb323aeb520b448ad4f2fe068e8ee82bcc0864add851e678444b050eb6c",

            BehaviorId = "lifesteal_smash",
            AbilitiesText = "Unleashes a heavy blow, restoring health equal to damage dealt.",

            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "ice_wolf", ChancePercent = 3 }
            }
        };
    }
}