using Hogs.RPG.Core.Entities.RaidObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Relics
{
    public static class AllRelics
    {
        // =========================
        // 🛡️ TANK RELICS
        // =========================

        public static readonly RelicDefinition IronVow = new()
        {
            Id = "iron_vow",
            Name = "Iron Vow",
            Description = "A warrior's oath carved in iron — your defence becomes your shield.",
            Affinity = RelicAffinity.Tank,
            BonusType = RelicBonusType.DefensePercent,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f }
        };

        public static readonly RelicDefinition FortressHeart = new()
        {
            Id = "fortress_heart",
            Name = "Fortress Heart",
            Description = "A relic pulsing with ancient endurance — your body becomes the wall.",
            Affinity = RelicAffinity.Tank,
            BonusType = RelicBonusType.MaxHpPercent,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f }
        };

        public static readonly RelicDefinition WardenResolve = new()
        {
            Id = "warden_resolve",
            Name = "Warden's Resolve",
            Description = "Your Shatter lingers longer, leaving the boss exposed for an extra round.",
            Affinity = RelicAffinity.Tank,
            BonusType = RelicBonusType.ShattersLastLonger,
            BonusPerRank = new float[] { 1, 1, 2, 2, 3 },
            SpecialEffectDescription = "Shatter debuff lasts additional rounds based on relic rank."
        };

        public static readonly RelicDefinition Unyielding = new()
        {
            Id = "unyielding",
            Name = "Unyielding",
            Description = "Your taunt comes faster — no enemy holds its gaze elsewhere for long.",
            Affinity = RelicAffinity.Tank,
            BonusType = RelicBonusType.TauntCooldownReduction,
            BonusPerRank = new float[] { 1, 1, 1, 2, 2 },
            SpecialEffectDescription = "Taunt becomes available rounds sooner based on relic rank."
        };

        // =========================
        // ⚔️ DPS RELICS
        // =========================

        public static readonly RelicDefinition BerserkersBrand = new()
        {
            Id = "berserkers_brand",
            Name = "Berserker's Brand",
            Description = "Fury made physical — every strike hits harder.",
            Affinity = RelicAffinity.Dps,
            BonusType = RelicBonusType.AttackPercent,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f }
        };

        public static readonly RelicDefinition ExecutionersMark = new()
        {
            Id = "executioners_mark",
            Name = "Executioner's Mark",
            Description = "When the boss is weakened your blade finds its mark with brutal precision.",
            Affinity = RelicAffinity.Dps,
            BonusType = RelicBonusType.ExecutionerBonus,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f },
            SpecialEffectDescription = "Bonus damage when boss is below 50% HP."
        };

        public static readonly RelicDefinition Bloodlust = new()
        {
            Id = "bloodlust",
            Name = "Bloodlust",
            Description = "Each wound you deal feeds you — your attacks drain the life of your enemy.",
            Affinity = RelicAffinity.Dps,
            BonusType = RelicBonusType.LifeSteal,
            BonusPerRank = new float[] { 0.02f, 0.04f, 0.06f, 0.09f, 0.12f },
            SpecialEffectDescription = "Heal for a percentage of damage dealt each attack."
        };

        public static readonly RelicDefinition Relentless = new()
        {
            Id = "relentless",
            Name = "Relentless",
            Description = "Each consecutive blow builds momentum — the longer you fight the harder you hit.",
            Affinity = RelicAffinity.Dps,
            BonusType = RelicBonusType.ConsecutiveHitBonus,
            BonusPerRank = new float[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f },
            SpecialEffectDescription = "Stacking attack bonus per consecutive round. Resets if a round is missed."
        };

        // =========================
        // 💚 HEALER RELICS
        // =========================

        public static readonly RelicDefinition MendersGrace = new()
        {
            Id = "menders_grace",
            Name = "Mender's Grace",
            Description = "Your potions carry more life — each heal reaches deeper.",
            Affinity = RelicAffinity.Healer,
            BonusType = RelicBonusType.IncreasedHealPercent,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f },
            SpecialEffectDescription = "Potions heal an additional % of max HP on top of the base 25%."
        };

        public static readonly RelicDefinition VialMastery = new()
        {
            Id = "vial_mastery",
            Name = "Vial Mastery",
            Description = "Your hands are blessed — sometimes a potion gives without taking.",
            Affinity = RelicAffinity.Healer,
            BonusType = RelicBonusType.ChanceToSavePotion,
            BonusPerRank = new float[] { 0.05f, 0.08f, 0.12f, 0.16f, 0.20f },
            SpecialEffectDescription = "Chance each heal does not consume a potion."
        };

        public static readonly RelicDefinition SpiritWard = new()
        {
            Id = "spirit_ward",
            Name = "Spirit Ward",
            Description = "The power you grant lingers in the air — your empowerment outlasts the moment.",
            Affinity = RelicAffinity.Healer,
            BonusType = RelicBonusType.EmpowerLastsLonger,
            BonusPerRank = new float[] { 1, 1, 2, 2, 3 },
            SpecialEffectDescription = "Empower buff lasts additional rounds based on relic rank."
        };

        public static readonly RelicDefinition BoundlessCare = new()
        {
            Id = "boundless_care",
            Name = "Boundless Care",
            Description = "Your spirit reaches further — your empower can uplift more than one at a time.",
            Affinity = RelicAffinity.Healer,
            BonusType = RelicBonusType.WiderEmpower,
            BonusPerRank = new float[] { 1, 1, 2, 2, 3 },
            SpecialEffectDescription = "Empower affects an additional party member once every set number of rounds."
        };

        // =========================
        // ✨ UNIVERSAL RELICS
        // =========================

        public static readonly RelicDefinition WanderersCoin = new()
        {
            Id = "wanderers_coin",
            Name = "Wanderer's Coin",
            Description = "Fortune follows those who wander — gold finds its way to your hands.",
            Affinity = RelicAffinity.Universal,
            BonusType = RelicBonusType.BonusGold,
            BonusPerRank = new float[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f },
        };

        public static readonly RelicDefinition ScholarsTome = new()
        {
            Id = "scholars_tome",
            Name = "Scholar's Tome",
            Description = "Knowledge flows from every battle — your experience compounds with each victory.",
            Affinity = RelicAffinity.Universal,
            BonusType = RelicBonusType.BonusPlayerXp,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f }
        };

        public static readonly RelicDefinition BeastBond = new()
        {
            Id = "beast_bond",
            Name = "Beast Bond",
            Description = "Your companion grows stronger beside you — victories feed you both.",
            Affinity = RelicAffinity.Universal,
            BonusType = RelicBonusType.BonusPetXp,
            BonusPerRank = new float[] { 0.03f, 0.06f, 0.10f, 0.14f, 0.18f }
        };

        public static readonly RelicDefinition Plunderer = new()
        {
            Id = "plunderer",
            Name = "Plunderer",
            Description = "You never leave empty handed — every clear hides a bonus prize.",
            Affinity = RelicAffinity.Universal,
            BonusType = RelicBonusType.BonusLootRoll,
            BonusPerRank = new float[] { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f },
            SpecialEffectDescription = "Bonus loot roll chance on dungeon, boss and raid clears."
        };
    }
}