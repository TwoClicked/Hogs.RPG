using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.DungeonBosses
{
    public static class DungeonBosses
    {
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
                new DungeonDrop
                {
                    ItemId = "fanculo_helm",
                    ChancePercent = 3
                }
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
                new DungeonDrop
                {
                    ItemId = "hrothgar_ring",
                    ChancePercent = 3
                }
            }
        };

        // =========================
        // LEVEL 20 BOSS
        // =========================
        public static readonly DungeonBossDefinition Luminara = new()
        {
            Id = "luminara",
            Name = "Luminara",
            Description = "Luminara, the Moonlit Deceiver — A Vittra who lures the living into the forest’s grasp",

            MaxHealth = 2100,
            Attack = 215,
            Defense = 40,

            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1486259283753701426/Boss.JPG?ex=69c4da52&is=69c388d2&hm=95b7737c96729bec3481238d7b7545422c0a731dc835432f8ab06d51bb55a0ea",

            BehaviorId = "defensive_cloud",
            AbilitiesText = "Surrounds herself with a toxic mist, reducing incoming damage.",

            Drops = new List<DungeonDrop>
            {
                new DungeonDrop
                {
                    ItemId = "luminara_amulet",
                    ChancePercent = 3
                }
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
                new DungeonDrop
                {
                    ItemId = "thorkell_boots",
                    ChancePercent = 3
                }
            }
        };
    }
}