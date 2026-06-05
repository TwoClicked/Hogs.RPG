using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.RaidObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Raids
{
    public static class AllRaids
    {
        public static readonly RaidDefinition Lair = new()
        {
            Id = "raid_t1",
            Name = "Wafflera, Queen of the Scorched Lair ",
            Description = "A fearsome Wafflera that lurks in the darkness.",
            Tier = 1,
            RequiredLevel = 10,
            ImageUrl = "https://cdn.discordapp.com/attachments/1499080557995360267/1504493466045124700/gemini-2.5-flash-image_Looks_male_make_abit_more_female-0.png?ex=6a07303a&is=6a05deba&hm=f4be570b42e28d32f9f876df6e445de9caa8e70cdac368ad43bf2b901e204c9d",

            HpMultiplier = 14f,
            AttackMultiplier = 0.6f,
            DefenseMultiplier = 0.4f,
            AggroSwapChance = 0.15f,

            AbilityPool = new List<BossAbilityType>
            {
                BossAbilityType.SavageCleave,
                BossAbilityType.TargetSwap
            },

            KeyIngredients = new List<RaidKeyIngredient>()
        };

        public static readonly RaidDefinition Stronghold = new()
        {
            Id = "raid_t2",
            Name = "Unc Click, Fastest clicker in the west",
            Description = "Unc Click, Fastest clicker in the west",
            Tier = 2,
            RequiredLevel = 15,
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1488229076186435734/09966EAA-9F64-47EB-9327-EBD9B283A37D.png?ex=6a0bf595&is=6a0aa415&hm=4a4cbe38fa3eca44c0222f65d0f0453e33de428558db3cb74052f79494a078b7",

            HpMultiplier = 16f,
            AttackMultiplier = 0.75f,
            DefenseMultiplier = 0.5f,
            AggroSwapChance = 0.20f,

            AbilityPool = new List<BossAbilityType>
            {
                BossAbilityType.SavageCleave,
                BossAbilityType.TargetSwap,
                BossAbilityType.CrushingBlow
            },

            KeyIngredients = new List<RaidKeyIngredient>()
        };

        public static readonly RaidDefinition Fortress = new()
        {
            Id = "raid_t3",
            Name = "Buldiablo, Male Harvester",
            Description = "Buldiablo, Male Harvester",
            Tier = 3,
            RequiredLevel = 20,
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504507051509022822/IMG_4518.jpg?ex=6a0bda21&is=6a0a88a1&hm=05f56c81d7cad7c05b3b141a57d74fa75c76cd2163b2c369aeebba4b30e9ebfd",

            HpMultiplier = 18f,
            AttackMultiplier = 0.9f,
            DefenseMultiplier = 0.6f,
            AggroSwapChance = 0.25f,

            AbilityPool = new List<BossAbilityType>
            {
                BossAbilityType.SavageCleave,
                BossAbilityType.TargetSwap,
                BossAbilityType.CrushingBlow,
                BossAbilityType.Enrage,
                BossAbilityType.Frenzy
            },

            KeyIngredients = new List<RaidKeyIngredient>()
        };

        public static readonly RaidDefinition Citadel = new()
        {
            Id = "raid_t4",
            Name = "Lysandra, The Frozen Divinity",
            Description = "Lysandra, The Frozen Divinity",
            Tier = 4,
            RequiredLevel = 25,
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504501125145956544/image0.png?ex=6a0bd49c&is=6a0a831c&hm=68c552edbd4e541fa29185618e9583400e9f2f8acd36d6a17128c36d6f24eeb1",

            HpMultiplier = 20f,
            AttackMultiplier = 1.1f,
            DefenseMultiplier = 0.7f,
            AggroSwapChance = 0.30f,

            AbilityPool = new List<BossAbilityType>
            {
                BossAbilityType.SavageCleave,
                BossAbilityType.TargetSwap,
                BossAbilityType.CrushingBlow,
                BossAbilityType.Enrage,
                BossAbilityType.Frenzy,
                BossAbilityType.Venom
            },

            KeyIngredients = new List<RaidKeyIngredient>()
        };

        public static readonly RaidDefinition WorldBoss = new()
        {
            Id = "raid_t5",
            Name = "Zarokh, the Worldburn Tyrant",
            Description = "The apex of destruction. This is the endgame — come prepared or do not come at all.",
            Tier = 5,
            RequiredLevel = 30,
            ImageUrl = "https://cdn.discordapp.com/attachments/1482007805513699358/1504504954927317092/image0.png?ex=6a0bd82d&is=6a0a86ad&hm=e58816d89387e0f2035ae60e4bb6d086fb67aeaa00d7d053b382c45724d4c95c",

            HpMultiplier = 22f,
            AttackMultiplier = 1.3f,
            DefenseMultiplier = 0.8f,
            AggroSwapChance = 0.40f,

            AbilityPool = new List<BossAbilityType>
            {
                BossAbilityType.SavageCleave,
                BossAbilityType.TargetSwap,
                BossAbilityType.CrushingBlow,
                BossAbilityType.Enrage,
                BossAbilityType.Frenzy,
                BossAbilityType.Venom,
                BossAbilityType.Execute
            },

            KeyIngredients = new List<RaidKeyIngredient>()
        };
    }
}