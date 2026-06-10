using Hogs.RPG.Core.Entities.DungeonObjects;
using Hogs.RPG.Core.Entities.PetObjects;

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

        // =========================
        // 🧪 ALCHEMIST COMPANION BOSS
        // The Abandoned Academy — Lv 27
        // =========================
        public static readonly DungeonBossDefinition Bandit = new()
        {
            Id = "bandit",
            Name = "Bandit, the Workshop Assistant",
            Description = "A mischievous lab creature that survived the academy's collapse by drinking everything it could find.",
            MaxHealth = 4000,
            Attack = 330,
            Defense = 65,
            ImageUrl = "https://media.discordapp.net/attachments/1490322685446717543/1513819678303195306/CF6A821D-9879-49EF-83E5-5F2D4D0321CB.png?ex=6a2a6f70&is=6a291df0&hm=14c1b7b9ed7e12dd97a4f493737ab715237ba708f4bd365bbcc25bb61d475510&=&format=webp&quality=lossless&width=968&height=968",
            BehaviorId = "toxic_concoction",
            AbilitiesText = "At 50% HP, hurls a volatile brew — poisoning you for 3 turns and reducing your attack by 20% for 2 turns.",
            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "bandit", ChancePercent = 3 }
            }
        };

        // =========================
        // 🌿 GATHER COMPANION BOSS
        // The Ashen Hollow — Lv 29
        // =========================
        public static readonly DungeonBossDefinition RavensOfOdin = new()
        {
            Id = "ravens_of_odin",
            Name = "The Ravens of Odin",
            Description = "Ancient ravens bound to the land, guarding the hollow against those deemed unworthy.",
            MaxHealth = 4700,
            Attack = 365,
            Defense = 72,
            ImageUrl = "https://media.discordapp.net/attachments/1490322685446717543/1514301258352300123/image.png?ex=6a2ade72&is=6a298cf2&hm=d77886d0a9aef9455562489b169f85531655fa56e8d170e710a771cf372e3294&=&format=webp&quality=lossless",
            BehaviorId = "raven_swarm",
            AbilitiesText = "25% chance each turn to split into a swarm, striking 3 times with reduced damage — hard to defend against.",
            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "ravens_of_odin", ChancePercent = 3 }
            }
        };

        // =========================
        // 🔨 BLACKSMITH COMPANION BOSS
        // Ember ClankaVille — Lv 31
        // =========================
        public static readonly DungeonBossDefinition FurnyDaClanka = new()
        {
            Id = "furny_da_clanka",
            Name = "Furny da Clanka",
            Description = "A rogue forge golem that built itself a village and doesn't appreciate visitors.",
            MaxHealth = 5500,
            Attack = 400,
            Defense = 80,
            ImageUrl = "https://media.discordapp.net/attachments/1499080557995360267/1514179175232311406/AA4D0FD6-4777-4CF7-9C4F-9394A1302F9B.png?ex=6a2a6cbf&is=6a291b3f&hm=b1cbf0f3ef694260fda8a236d62a4141c1c5b55d37bec74fff81a106f9017396&=&format=webp&quality=lossless&width=968&height=968",
            BehaviorId = "forge_slam",
            AbilitiesText = "Below 40% HP, enters forge fury — all attacks deal +30% damage and have a 15% chance to stun.",
            PetDrops = new List<PetDrop>
            {
                new PetDrop { PetId = "furny_da_clanka", ChancePercent = 3 }
            }
        };
    }
}