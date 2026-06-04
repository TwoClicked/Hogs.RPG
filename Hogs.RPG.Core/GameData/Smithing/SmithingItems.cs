using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Smithing
{
    public static class SmithingItems
    {
        // ===== Bronze (Level 1) =====
        public static readonly SmithingItemDefinition BronzeSword = new()
        {
            Id = "bronze_sword",
            Name = "Bronze Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 1,
            BarRequirements = new() { ["bronze_bar"] = 1 },
            SmithingXpReward = 15,
            NpcGoldPrice = 8,
            MaxNpcBuysPerDay = 20
        };

        public static readonly SmithingItemDefinition BronzeAxe = new()
        {
            Id = "bronze_axe",
            Name = "Bronze Axe",
            Icon = "🪓",
            RequiredSmithingLevel = 1,
            BarRequirements = new() { ["bronze_bar"] = 1 },
            SmithingXpReward = 15,
            NpcGoldPrice = 8,
            MaxNpcBuysPerDay = 20
        };

        // ===== Iron (Level 15) =====
        public static readonly SmithingItemDefinition IronSword = new()
        {
            Id = "iron_sword",
            Name = "Iron Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 15,
            BarRequirements = new() { ["iron_bar"] = 1 },
            SmithingXpReward = 30,
            NpcGoldPrice = 25,
            MaxNpcBuysPerDay = 15
        };

        public static readonly SmithingItemDefinition IronDagger = new()
        {
            Id = "iron_dagger",
            Name = "Iron Dagger",
            Icon = "🗡️",
            RequiredSmithingLevel = 15,
            BarRequirements = new() { ["iron_bar"] = 1 },
            SmithingXpReward = 30,
            NpcGoldPrice = 22,
            MaxNpcBuysPerDay = 15
        };

        // ===== Steel (Level 30) =====
        public static readonly SmithingItemDefinition SteelSword = new()
        {
            Id = "steel_sword",
            Name = "Steel Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 30,
            BarRequirements = new() { ["steel_bar"] = 2 },
            SmithingXpReward = 65,
            NpcGoldPrice = 80,
            MaxNpcBuysPerDay = 10
        };

        public static readonly SmithingItemDefinition SteelMace = new()
        {
            Id = "steel_mace",
            Name = "Steel Mace",
            Icon = "🔨",
            RequiredSmithingLevel = 30,
            BarRequirements = new() { ["steel_bar"] = 2 },
            SmithingXpReward = 65,
            NpcGoldPrice = 75,
            MaxNpcBuysPerDay = 10
        };

        // ===== Mithril (Level 50) =====
        public static readonly SmithingItemDefinition MithrilSword = new()
        {
            Id = "mithril_sword",
            Name = "Mithril Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 50,
            BarRequirements = new() { ["mithril_bar"] = 3 },
            SmithingXpReward = 130,
            NpcGoldPrice = 200,
            MaxNpcBuysPerDay = 7
        };

        public static readonly SmithingItemDefinition MithrilShield = new()
        {
            Id = "mithril_shield",
            Name = "Mithril Shield",
            Icon = "🛡️",
            RequiredSmithingLevel = 50,
            BarRequirements = new() { ["mithril_bar"] = 3 },
            SmithingXpReward = 130,
            NpcGoldPrice = 185,
            MaxNpcBuysPerDay = 7
        };

        // ===== Adamant (Level 70) =====
        public static readonly SmithingItemDefinition AdamantSword = new()
        {
            Id = "adamant_sword",
            Name = "Adamant Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 70,
            BarRequirements = new() { ["adamant_bar"] = 3 },
            SmithingXpReward = 210,
            NpcGoldPrice = 500,
            MaxNpcBuysPerDay = 5
        };

        public static readonly SmithingItemDefinition AdamantHelm = new()
        {
            Id = "adamant_helm",
            Name = "Adamant Helm",
            Icon = "⛑️",
            RequiredSmithingLevel = 70,
            BarRequirements = new() { ["adamant_bar"] = 2 },
            SmithingXpReward = 175,
            NpcGoldPrice = 420,
            MaxNpcBuysPerDay = 5
        };

        // ===== Rune (Level 85) =====
        public static readonly SmithingItemDefinition RuneSword = new()
        {
            Id = "rune_sword",
            Name = "Rune Sword",
            Icon = "⚔️",
            RequiredSmithingLevel = 85,
            BarRequirements = new() { ["runite_bar"] = 4 },
            SmithingXpReward = 360,
            NpcGoldPrice = 1000,
            MaxNpcBuysPerDay = 3
        };

        public static readonly SmithingItemDefinition RuneShield = new()
        {
            Id = "rune_shield",
            Name = "Rune Shield",
            Icon = "🛡️",
            RequiredSmithingLevel = 85,
            BarRequirements = new() { ["runite_bar"] = 3 },
            SmithingXpReward = 310,
            NpcGoldPrice = 900,
            MaxNpcBuysPerDay = 3
        };

        // ===== Dragon (Level 99) =====
        public static readonly SmithingItemDefinition DragonBlade = new()
        {
            Id = "dragon_blade",
            Name = "Dragon Blade",
            Icon = "🐉",
            RequiredSmithingLevel = 99,
            BarRequirements = new() { ["dragon_crystal"] = 1 },
            SmithingXpReward = 600,
            NpcGoldPrice = 2500,
            MaxNpcBuysPerDay = 2
        };
    }
}