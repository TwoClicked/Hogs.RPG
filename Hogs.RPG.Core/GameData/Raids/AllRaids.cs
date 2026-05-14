using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Raids
{
    public static class AllRaids
    {
        public static readonly RaidDefinition Lair = new()
        {
            Id = "raid_t1",
            Name = "Wafflera, Queen of the Scorched Lair ",
            Description = "A fearsome Wafflera lurks in the darkness.",
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
            Name = "The Stronghold",
            Description = "The enemy has fortified its domain. Breach its defences or fall trying.",
            Tier = 2,
            RequiredLevel = 15,
            ImageUrl = "https://placeholder.com/raid_t2.png",

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
            Name = "The Fortress",
            Description = "An ancient fortress hiding something far worse than stone and iron.",
            Tier = 3,
            RequiredLevel = 20,
            ImageUrl = "https://placeholder.com/raid_t3.png",

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
            Name = "The Citadel",
            Description = "A citadel of terror. Only the strongest parties survive what awaits within.",
            Tier = 4,
            RequiredLevel = 25,
            ImageUrl = "https://placeholder.com/raid_t4.png",

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
            Name = "The World Boss",
            Description = "The apex of destruction. This is the endgame — come prepared or do not come at all.",
            Tier = 5,
            RequiredLevel = 30,
            ImageUrl = "https://placeholder.com/raid_t5.png",

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