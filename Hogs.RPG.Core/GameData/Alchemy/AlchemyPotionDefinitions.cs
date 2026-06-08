using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Alchemy
{
    public static class AlchemyPotionDefinitions
    {
        // =========================
        // LEVEL 1
        // =========================

        public static readonly AlchemyPotionDefinition WeakStaminaVial = new()
        {
            Id = "weak_stamina_vial",
            Name = "Weak Stamina Vial",
            Icon = "🧪",
            RequiredAlchemistLevel = 1,
            IngredientRequirements = new() { ["moonpetal"] = 3 },
            AlchemyXpReward = 15,
            PotionType = "instant",
            DurationMinutes = 0,
            EffectId = "restore_stamina",
            EffectValue = 20,
            Description = "Instantly restores 20 Hunter Stamina."
        };

        public static readonly AlchemyPotionDefinition ApprenticesBrew = new()
        {
            Id = "apprentices_brew",
            Name = "Apprentice's Brew",
            Icon = "🧪",
            RequiredAlchemistLevel = 1,
            IngredientRequirements = new() { ["moonpetal"] = 3, ["glowshroom"] = 2 },
            AlchemyXpReward = 15,
            PotionType = "utility",
            DurationMinutes = 60,
            EffectId = "xp_boost",
            EffectValue = 10,
            Description = "+10% XP gain for 1 hour."
        };

        // =========================
        // LEVEL 10
        // =========================

        public static readonly AlchemyPotionDefinition WeakHuntersDraft = new()
        {
            Id = "weak_hunters_draft",
            Name = "Weak Hunter's Draft",
            Icon = "🧪",
            RequiredAlchemistLevel = 10,
            IngredientRequirements = new() { ["moonpetal"] = 4, ["swamp_root"] = 2 },
            AlchemyXpReward = 25,
            PotionType = "utility",
            DurationMinutes = 60,
            EffectId = "loot_boost",
            EffectValue = 10,
            Description = "+10% loot drop chance for 1 hour."
        };

        // =========================
        // LEVEL 15
        // =========================

        public static readonly AlchemyPotionDefinition StaminaVial = new()
        {
            Id = "stamina_vial",
            Name = "Stamina Vial",
            Icon = "🧪",
            RequiredAlchemistLevel = 15,
            IngredientRequirements = new() { ["moonpetal"] = 5, ["glowshroom"] = 3 },
            AlchemyXpReward = 30,
            PotionType = "instant",
            DurationMinutes = 0,
            EffectId = "restore_stamina",
            EffectValue = 40,
            Description = "Instantly restores 40 Hunter Stamina."
        };

        public static readonly AlchemyPotionDefinition TrailTonic = new()
        {
            Id = "trail_tonic",
            Name = "Trail Tonic",
            Icon = "🧪",
            RequiredAlchemistLevel = 15,
            IngredientRequirements = new() { ["moonpetal"] = 4, ["swamp_root"] = 3 },
            AlchemyXpReward = 30,
            PotionType = "instant",
            DurationMinutes = 0,
            EffectId = "trail_reset",
            EffectValue = 1,
            Description = "Grants +1 extra trail run today."
        };

        // =========================
        // LEVEL 20
        // =========================

        public static readonly AlchemyPotionDefinition HuntersDraft = new()
        {
            Id = "hunters_draft",
            Name = "Hunter's Draft",
            Icon = "🧪",
            RequiredAlchemistLevel = 20,
            IngredientRequirements = new() { ["moonpetal"] = 5, ["swamp_root"] = 3, ["glowshroom"] = 2 },
            AlchemyXpReward = 35,
            PotionType = "utility",
            DurationMinutes = 120,
            EffectId = "loot_boost",
            EffectValue = 15,
            Description = "+15% loot drop chance for 2 hours."
        };

        // =========================
        // LEVEL 25
        // =========================

        public static readonly AlchemyPotionDefinition XpSerum = new()
        {
            Id = "xp_serum",
            Name = "XP Serum",
            Icon = "🧪",
            RequiredAlchemistLevel = 25,
            IngredientRequirements = new() { ["glowshroom"] = 4, ["swamp_root"] = 3 },
            AlchemyXpReward = 40,
            PotionType = "utility",
            DurationMinutes = 120,
            EffectId = "xp_boost",
            EffectValue = 20,
            Description = "+20% XP gain for 2 hours."
        };

        // =========================
        // LEVEL 30
        // =========================

        public static readonly AlchemyPotionDefinition WeakBerserkerBrew = new()
        {
            Id = "weak_berserker_brew",
            Name = "Weak Berserker Brew",
            Icon = "🧪",
            RequiredAlchemistLevel = 30,
            IngredientRequirements = new() { ["glowshroom"] = 3, ["venom_gland"] = 2, ["serpent_venom"] = 1 },
            AlchemyXpReward = 55,
            PotionType = "stat",
            DurationMinutes = 60,
            EffectId = "atk_boost",
            EffectValue = 15,
            SecondaryEffectId = "def_penalty",
            SecondaryEffectValue = 10,
            Description = "+15% ATK, -10% DEF for 1 hour."
        };

        public static readonly AlchemyPotionDefinition WeakIronbloodTonic = new()
        {
            Id = "weak_ironblood_tonic",
            Name = "Weak Ironblood Tonic",
            Icon = "🧪",
            RequiredAlchemistLevel = 30,
            IngredientRequirements = new() { ["glowshroom"] = 3, ["swamp_root"] = 2, ["serpent_venom"] = 1 },
            AlchemyXpReward = 55,
            PotionType = "stat",
            DurationMinutes = 60,
            EffectId = "def_boost",
            EffectValue = 15,
            SecondaryEffectId = "atk_penalty",
            SecondaryEffectValue = 10,
            Description = "+15% DEF, -10% ATK for 1 hour."
        };

        // =========================
        // LEVEL 40
        // =========================

        public static readonly AlchemyPotionDefinition GoldRushFlask = new()
        {
            Id = "gold_rush_flask",
            Name = "Gold Rush Flask",
            Icon = "🧪",
            RequiredAlchemistLevel = 40,
            IngredientRequirements = new() { ["venom_gland"] = 4, ["glowshroom"] = 3, ["serpent_venom"] = 2 },
            AlchemyXpReward = 75,
            PotionType = "utility",
            DurationMinutes = 120,
            EffectId = "gold_boost",
            EffectValue = 20,
            Description = "+20% gold from dungeons for 2 hours."
        };

        // =========================
        // LEVEL 50
        // =========================

        public static readonly AlchemyPotionDefinition BerserkerBrew = new()
        {
            Id = "berserker_brew",
            Name = "Berserker Brew",
            Icon = "🧪",
            RequiredAlchemistLevel = 50,
            IngredientRequirements = new()
            {
                ["venom_gland"] = 4,
                ["dreamleaf"] = 3,
                ["serpent_venom"] = 2,
                ["alchemical_core"] = 1
            },
            AlchemyXpReward = 110,
            PotionType = "stat",
            DurationMinutes = 120,
            EffectId = "atk_boost",
            EffectValue = 25,
            SecondaryEffectId = "def_penalty",
            SecondaryEffectValue = 15,
            Description = "+25% ATK, -15% DEF for 2 hours."
        };

        public static readonly AlchemyPotionDefinition IronbloodTonic = new()
        {
            Id = "ironblood_tonic",
            Name = "Ironblood Tonic",
            Icon = "🧪",
            RequiredAlchemistLevel = 50,
            IngredientRequirements = new()
            {
                ["venom_gland"] = 4,
                ["swamp_root"] = 3,
                ["serpent_venom"] = 2,
                ["alchemical_core"] = 1
            },
            AlchemyXpReward = 110,
            PotionType = "stat",
            DurationMinutes = 120,
            EffectId = "def_boost",
            EffectValue = 25,
            SecondaryEffectId = "atk_penalty",
            SecondaryEffectValue = 15,
            Description = "+25% DEF, -15% ATK for 2 hours."
        };

        // =========================
        // LEVEL 60
        // =========================

        public static readonly AlchemyPotionDefinition Antivenom = new()
        {
            Id = "antivenom",
            Name = "Antivenom",
            Icon = "🧪",
            RequiredAlchemistLevel = 60,
            IngredientRequirements = new()
            {
                ["venom_gland"] = 5,
                ["dreamleaf"] = 3,
                ["alchemical_core"] = 2
            },
            AlchemyXpReward = 145,
            PotionType = "utility",
            DurationMinutes = 0,
            EffectId = "dungeon_dmg_reduction",
            EffectValue = 20,
            Description = "-20% dungeon damage taken for the next dungeon run."
        };

        public static readonly AlchemyPotionDefinition BlacksmithsElixir = new()
        {
            Id = "blacksmiths_elixir",
            Name = "Blacksmith's Elixir",
            Icon = "⚒️",
            RequiredAlchemistLevel = 60,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 4,
                ["swamp_root"] = 3,
                ["alchemical_core"] = 2
            },
            AlchemyXpReward = 145,
            PotionType = "utility",
            DurationMinutes = 0,
            EffectId = "npc_max_demand",
            EffectValue = 1,
            Description = "NPCs buy the maximum amount of every item in your shop at the next 12 UTC reset."
        };

        // =========================
        // LEVEL 70
        // =========================

        public static readonly AlchemyPotionDefinition SwiftfootBrew = new()
        {
            Id = "swiftfoot_brew",
            Name = "Swiftfoot Brew",
            Icon = "🧪",
            RequiredAlchemistLevel = 70,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 5,
                ["venom_gland"] = 3,
                ["alchemical_core"] = 2
            },
            AlchemyXpReward = 185,
            PotionType = "utility",
            DurationMinutes = 0,
            EffectId = "dodge_boost",
            EffectValue = 15,
            Description = "+15% dodge chance for the next dungeon run."
        };

        public static readonly AlchemyPotionDefinition RaidElixir = new()
        {
            Id = "raid_elixir",
            Name = "Raid Elixir",
            Icon = "🧪",
            RequiredAlchemistLevel = 70,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 5,
                ["venom_gland"] = 4,
                ["alchemical_core"] = 2,
                ["ethereal_dust"] = 1
            },
            AlchemyXpReward = 185,
            PotionType = "stat",
            DurationMinutes = 180,
            EffectId = "atk_boost",
            EffectValue = 15,
            SecondaryEffectId = "def_boost",
            SecondaryEffectValue = 15,
            Description = "+15% ATK and +15% DEF for 3 hours."
        };

        // =========================
        // LEVEL 80
        // =========================

        public static readonly AlchemyPotionDefinition BloodPactPotion = new()
        {
            Id = "blood_pact_potion",
            Name = "Blood Pact Potion",
            Icon = "🧪",
            RequiredAlchemistLevel = 80,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 5,
                ["phoenix_ash"] = 3,
                ["ethereal_dust"] = 2
            },
            AlchemyXpReward = 230,
            PotionType = "stat",
            DurationMinutes = 120,
            EffectId = "atk_boost",
            EffectValue = 50,
            SecondaryEffectId = "hp_penalty",
            SecondaryEffectValue = 20,
            Description = "+50% ATK, -20% HP for 2 hours."
        };

        public static readonly AlchemyPotionDefinition GreaterStaminaVial = new()
        {
            Id = "greater_stamina_vial",
            Name = "Greater Stamina Vial",
            Icon = "🧪",
            RequiredAlchemistLevel = 80,
            IngredientRequirements = new()
            {
                ["glowshroom"] = 5,
                ["moonpetal"] = 4,
                ["alchemical_core"] = 2
            },
            AlchemyXpReward = 230,
            PotionType = "instant",
            DurationMinutes = 0,
            EffectId = "restore_stamina_full",
            EffectValue = 100,
            Description = "Fully restores all Hunter Stamina."
        };

        // =========================
        // LEVEL 85
        // =========================

        public static readonly AlchemyPotionDefinition RevivalDraught = new()
        {
            Id = "revival_draught",
            Name = "Revival Draught",
            Icon = "🧪",
            RequiredAlchemistLevel = 85,
            IngredientRequirements = new()
            {
                ["phoenix_ash"] = 4,
                ["dreamleaf"] = 3,
                ["ethereal_dust"] = 2
            },
            AlchemyXpReward = 275,
            PotionType = "instant",
            DurationMinutes = 0,
            EffectId = "revival",
            EffectValue = 1,
            Description = "Survive one killing blow in your next dungeon run."
        };

        // =========================
        // LEVEL 90
        // =========================

        public static readonly AlchemyPotionDefinition GreaterHuntersDraft = new()
        {
            Id = "greater_hunters_draft",
            Name = "Greater Hunter's Draft",
            Icon = "🧪",
            RequiredAlchemistLevel = 90,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 5,
                ["phoenix_ash"] = 4,
                ["ethereal_dust"] = 2,
                ["philosophers_stone"] = 1
            },
            AlchemyXpReward = 320,
            PotionType = "utility",
            DurationMinutes = 180,
            EffectId = "loot_boost",
            EffectValue = 25,
            Description = "+25% loot drop chance for 3 hours."
        };

        public static readonly AlchemyPotionDefinition GreaterXpSerum = new()
        {
            Id = "greater_xp_serum",
            Name = "Greater XP Serum",
            Icon = "🧪",
            RequiredAlchemistLevel = 90,
            IngredientRequirements = new()
            {
                ["dreamleaf"] = 5,
                ["phoenix_ash"] = 4,
                ["ethereal_dust"] = 2,
                ["philosophers_stone"] = 1
            },
            AlchemyXpReward = 320,
            PotionType = "utility",
            DurationMinutes = 180,
            EffectId = "xp_boost",
            EffectValue = 35,
            Description = "+35% XP gain for 3 hours."
        };

        // =========================
        // LEVEL 95
        // =========================

        public static readonly AlchemyPotionDefinition ShadowSalve = new()
        {
            Id = "shadow_salve",
            Name = "Shadow Salve",
            Icon = "🧪",
            RequiredAlchemistLevel = 95,
            IngredientRequirements = new()
            {
                ["phoenix_ash"] = 5,
                ["dreamleaf"] = 3,
                ["ethereal_dust"] = 2,
                ["philosophers_stone"] = 2
            },
            AlchemyXpReward = 380,
            PotionType = "utility",
            DurationMinutes = 0,
            EffectId = "first_strike_reduction",
            EffectValue = 30,
            Description = "-30% enemy first strike damage in the next dungeon run."
        };

        // =========================
        // LEVEL 99
        // =========================

        public static readonly AlchemyPotionDefinition VoidTincture = new()
        {
            Id = "void_tincture",
            Name = "Void Tincture",
            Icon = "🔮",
            RequiredAlchemistLevel = 99,
            IngredientRequirements = new()
            {
                ["phoenix_ash"] = 6,
                ["dreamleaf"] = 4,
                ["ethereal_dust"] = 3,
                ["philosophers_stone"] = 2
            },
            AlchemyXpReward = 500,
            PotionType = "utility",
            DurationMinutes = 240,
            EffectId = "xp_boost",
            EffectValue = 50,
            SecondaryEffectId = "loot_boost",
            SecondaryEffectValue = 25,
            Description = "+50% XP gain and +25% loot drop chance for 4 hours."
        };

        public static readonly AlchemyPotionDefinition DragonBlood = new()
        {
            Id = "dragons_blood",
            Name = "Dragon's Blood",
            Icon = "🐉",
            RequiredAlchemistLevel = 99,
            IngredientRequirements = new()
            {
                ["phoenix_ash"] = 5,
                ["dreamleaf"] = 4,
                ["ethereal_dust"] = 3,
                ["philosophers_stone"] = 3
            },
            AlchemyXpReward = 500,
            PotionType = "stat",
            DurationMinutes = 120,
            EffectId = "atk_boost",
            EffectValue = 40,
            SecondaryEffectId = "def_boost",
            SecondaryEffectValue = 40,
            Description = "+40% ATK and +40% DEF for 2 hours."
        };
    }
}