using Hogs.RPG.Core.Entities;
using System.Runtime.InteropServices;

namespace Hogs.RPG.Core.GameData.DungeonBosses
{
    public static class DungeonBosses
    {

        // =========================
        // LEVEL 5 BOSS
        // =========================
        public static readonly DungeonBossDefinition RotFatherMalchor = new()
        {
            Id = "rot_father_malchor",
            Name = "Rot Father Malchor",
            Description = "Malchor, the Blighted Archdruid — A fallen guardian of nature, now a living plague who spreads endless rot wherever he treads.",

            MaxHealth = 750,
            Attack = 70,
            Defense = 16,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1495333078564995282/image.png?ex=69e5dcf3&is=69e48b73&hm=c9e7ce730e8205d86356212863cbdadea2611ef40435974d265815508a8bfcf6",

            BehaviorId = "Intoxicate",
            AbilitiesText = "At 50% health Malchor releases a cloud of toxic spores, dealing damage over time to all nearby enemies.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "malchor_grips", ChancePercent = 3 }
            }
        };


        // =========================
        // LEVEL 10 BOSS
        // =========================
        public static readonly DungeonBossDefinition Fanculo = new()
        {
            Id = "fanculo",
            Name = "Fanculo the Wandering Viking",
            Description = "A chaotic Viking riding a divine scooter, radiating raw strength. screaming FANCULO",

            MaxHealth = 950,
            Attack = 85,
            Defense = 18,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482049411365470400/B2BA700C-F9CE-4B4B-8847-44D4EE18825C.png?ex=69bd7292&is=69bc2112&hm=d461b2ff15efb092afd84048e83339051c4b96b2ca5c1ee0138cbdca3932401c",

            BehaviorId = "rage",
            AbilitiesText = "Enters rage at low HP, doubling damage temporarily.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "fanculo_helm", ChancePercent = 3 }
            }
        };



        // =========================
        // LEVEL 15 BOSS
        // =========================
        public static readonly DungeonBossDefinition Hrothgar = new()
        {
            Id = "hrothgar",
            Name = "Hrothgar",
            Description = "Hrothgar the Frost-Bound Jarl, The ancient Viking warlord cursed by the gods",

            MaxHealth = 1400,
            Attack = 165,
            Defense = 28,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1482039038960468029/image0.jpg?ex=69c4a929&is=69c357a9&hm=da453d03b2b992740ca594ab6a22c24a9f11bcfbfd6da396666fc5fa01af4b47",

            BehaviorId = "lifesteal_smash",
            AbilitiesText = "Unleashes a heavy blow, restoring health equal to damage dealt.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "hrothgar_ring", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 17 BOSS
        // =========================
        public static readonly DungeonBossDefinition AurelionTheOathbreakerPaladin = new()
        {
            Id = "aurelion_the_oathbreaker_paladin",
            Name = "Aurelion, the Oathbreaker Paladin",
            Description = "A fallen holy knight who was betrayed by his order and left to die beneath their cathedral. Once a symbol of divine justice.",

            MaxHealth = 1600,
            Attack = 185,
            Defense = 33,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486586479223246909/486435901_604223982616622_6484063493615273434_n.jpg?ex=69e5af0c&is=69e45d8c&hm=2af31ca721626edc9b191d67f2d539fd31b088afb0c30cfe64d8286aa04ca815",

            BehaviorId = "chain_snare",
            AbilitiesText = "Briefly roots a player in place skipping their attack and attacking back instead",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "oathcrush_legguards", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 18 BOSS
        // =========================
        public static readonly DungeonBossDefinition AmphivosTaterous = new ()
        {
            Id = "amphivos_taterous",
            Name = "Amphivos Taterous",
            Description = "Amphivos Taterous, Queen of the Starchy Wastes — A battle-hardened tuber born of enchanted earth, as unyielding as the soil she rose from.",

            MaxHealth = 1800,
            Attack = 200,
            Defense = 36,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1495336257016954921/image.png?ex=69e5dfe9&is=69e48e69&hm=48bcad96cd87e979bf3f31e13ba3e9bf60a60a45c280903ddbf2be926ba0637f",

            BehaviorId = "tongue_of_the_abyss",
            AbilitiesText = "Strikes with her elongated tongue, pulling enemies closer and dealing damage.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "taterous_battleaxe", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 20 BOSS
        // =========================
        public static readonly DungeonBossDefinition Luminara = new()
        {
            Id = "luminara",
            Name = "Luminara",
            Description = "Luminara, the Moonlit Deceiver — A Vittra who lures the living into the forest's grasp",

            MaxHealth = 2100,
            Attack = 215,
            Defense = 40,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486259283753701426/Boss.JPG?ex=69c4da52&is=69c388d2&hm=95b7737c96729bec3481238d7b7545422c0a731dc835432f8ab06d51bb55a0ea",

            BehaviorId = "defensive_cloud",
            AbilitiesText = "Surrounds herself with a toxic mist, reducing incoming damage.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "luminara_amulet", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 22 BOSS (NEW)
        // =========================
        public static readonly DungeonBossDefinition SkarrTheClownOfCarnage = new ()
        {
            Id = "skarr_the_clown_of_carnage",
            Name = "Skarr, The Clown of Carnage",
            Description = "The performer who failed to bring joy and instead brought chaos.",

            MaxHealth = 2400,
            Attack = 240,
            Defense = 45,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1487032134526042172/content.png?ex=69e553d8&is=69e40258&hm=b96eba2e3b2bc04082ca66f27e57216cea73490de15cb791eefbac49ba4062f2",

            BehaviorId = "maniacal_encore",
            AbilitiesText = "Crushing blows that leave enemies in a state of panic",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "skarr_sawbladeshield", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 23 BOSS
        // =========================
        public static readonly DungeonBossDefinition GrimblexTheGildedTyrant = new ()
        {
            Id = "grimblex_the_gilded_tyrant",
            Name = "Grimblex, The Gilded Tyrant",
            Description = "The corrupted greed goblin.",

            MaxHealth = 2700,
            Attack = 260,
            Defense = 52,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1491145087600562308/E29EC022-8F76-4E1A-B767-E76A42F84581.png?ex=69e5c9d5&is=69e47855&hm=28778a25b914969badb12c82501e306cbafca5d942ad2b9b459bfff6a94cfe18",

            BehaviorId = "gold_shine",
            AbilitiesText = "Lowering players' defenses with a dazzling display of wealth.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "shadowsaphire_signet", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 25 BOSS
        // =========================
        public static readonly DungeonBossDefinition ThorkellSonOfTyr = new()
        {
            Id = "thorkell_son_of_tyr",
            Name = "Thorkell, Son of Tyr",
            Description = "The blood-born warlord torn between divine justice and primal fury",

            MaxHealth = 3200,
            Attack = 285,
            Defense = 60,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486083730123526285/image.png?ex=69c436d3&is=69c2e553&hm=7671b9af89c6a275aa2fb1b90df80281ceb75a5e50e157d777200ada94a88c08",

            BehaviorId = "crushing_blow",
            AbilitiesText = "Occasionally unleashes a devastating blow ignoring part of the player's defense.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "thorkell_boots", ChancePercent = 3 }
            }
        };

        // =========================
        // LEVEL 27 BOSS (NEW)
        // =========================
        public static readonly DungeonBossDefinition Gritch = new()
        {
            Id = "gritch",
            Name = "Gritch, The Gilded Prowler",
            Description = "Gritch, the Sparkle-Mad Wretch — A broken mine-crawler twisted by Star-Iron madness, crawling out of the dark to steal everything that shines.",

            MaxHealth = 4500,
            Attack = 380,
            Defense = 80,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1495340104477048963/image.png?ex=69e5e37e&is=69e491fe&hm=3c7df25a6deee4505e16f2e8eb3e057d4281540e7b1f29b5bbc5ca7abd9ae649",

            BehaviorId = "star_iron_madness",
            AbilitiesText = "When 50% Health is reached, Gritch enters a frenzied state, doubling his defense",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop { ItemId = "gritch_warplate", ChancePercent = 3 }
            }
        };
    }
}
