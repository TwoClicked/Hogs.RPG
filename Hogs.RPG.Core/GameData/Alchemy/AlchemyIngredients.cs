using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.EquipmentObjects;

namespace Hogs.RPG.Core.GameData.Alchemy
{
    public static class AlchemyIngredients
    {
        // =========================
        // SWAMP GATHER DROPS
        // =========================

        public static readonly ItemDefinition Moonpetal = new()
        {
            Id = "moonpetal",
            Name = "Moonpetal",
            Icon = "🌿",
            Type = "Material",
            Description = "A glowing petal that blooms only in swamp mist.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition Glowshroom = new()
        {
            Id = "glowshroom",
            Name = "Glowshroom",
            Icon = "🍄",
            Type = "Material",
            Description = "A bioluminescent mushroom from the swamp floor.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition SwampRoot = new()
        {
            Id = "swamp_root",
            Name = "Swamp Root",
            Icon = "🌱",
            Type = "Material",
            Description = "A gnarled root used as alchemical flux.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition VenomGland = new()
        {
            Id = "venom_gland",
            Name = "Venom Gland",
            Icon = "💧",
            Type = "Material",
            Description = "A toxic gland harvested from swamp creatures.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition Dreamleaf = new()
        {
            Id = "dreamleaf",
            Name = "Dreamleaf",
            Icon = "🌙",
            Type = "Material",
            Description = "A rare leaf that induces visions when brewed.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition PhoenixAsh = new()
        {
            Id = "phoenix_ash",
            Name = "Phoenix Ash",
            Icon = "🔥",
            Type = "Material",
            Description = "Ash left behind by a phoenix — impossibly rare.",
            SubCategory = "Alchemy"
        };

        // =========================
        // MONSTER HUNT DROPS
        // =========================

        public static readonly ItemDefinition SerpentVenom = new()
        {
            Id = "serpent_venom",
            Name = "Serpent Venom",
            Icon = "🐍",
            Type = "Material",
            Description = "Concentrated venom extracted from a Swamp Serpent.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition AlchemicalCore = new()
        {
            Id = "alchemical_core",
            Name = "Alchemical Core",
            Icon = "⚙️",
            Type = "Material",
            Description = "A dense magical core extracted from a Corrupted Golem.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition EtherealDust = new()
        {
            Id = "ethereal_dust",
            Name = "Ethereal Dust",
            Icon = "👻",
            Type = "Material",
            Description = "Shimmering dust left behind by a Shadow Wraith.",
            SubCategory = "Alchemy"
        };

        public static readonly ItemDefinition PhilosophersStone = new()
        {
            Id = "philosophers_stone",
            Name = "Philosopher's Stone",
            Icon = "💎",
            Type = "Material",
            Description = "An impossibly rare stone dropped by Elder Alchemists.",
            SubCategory = "Alchemy"
        };
    }
}