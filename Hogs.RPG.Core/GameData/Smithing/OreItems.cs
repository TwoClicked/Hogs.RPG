using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Smithing
{
    public static class OreItems
    {
        // ===== Ores =====
        public static readonly ItemDefinition BronzeOre = new()
        {
            Id = "bronze_ore",
            Name = "Bronze Ore",
            Icon = "🪨",
            Type = "Material",
            Description = "A chunk of bronze ore mined from the ground.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition IronOre = new()
        {
            Id = "iron_ore",
            Name = "Iron Ore",
            Icon = "🪨",
            Type = "Material",
            Description = "A dense lump of iron ore.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition Coal = new()
        {
            Id = "coal",
            Name = "Coal",
            Icon = "🖤",
            Type = "Material",
            Description = "Dark coal used as flux in smelting.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition MithrilOre = new()
        {
            Id = "mithril_ore",
            Name = "Mithril Ore",
            Icon = "💠",
            Type = "Material",
            Description = "A rare glowing ore with magical properties.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition AdamantiteOre = new()
        {
            Id = "adamantite_ore",
            Name = "Adamantite Ore",
            Icon = "🟢",
            Type = "Material",
            Description = "An extremely hard green-tinted ore.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition RuniteOre = new()
        {
            Id = "runite_ore",
            Name = "Runite Ore",
            Icon = "🔵",
            Type = "Material",
            Description = "The rarest ore, deep blue and incredibly dense.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition DragonCrystal = new()
        {
            Id = "dragon_crystal",
            Name = "Dragon Crystal",
            Icon = "🔮",
            Type = "Material",
            Description = "An impossibly rare crystal pulsing with draconic energy. Only found by master miners.",
            SubCategory = "Smithing"
        };

        // ===== Bars =====
        public static readonly ItemDefinition BronzeBar = new()
        {
            Id = "bronze_bar",
            Name = "Bronze Bar",
            Icon = "🟫",
            Type = "Material",
            Description = "A smelted bronze bar ready for forging.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition IronBar = new()
        {
            Id = "iron_bar",
            Name = "Iron Bar",
            Icon = "⬛",
            Type = "Material",
            Description = "A solid iron bar.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition SteelBar = new()
        {
            Id = "steel_bar",
            Name = "Steel Bar",
            Icon = "🩶",
            Type = "Material",
            Description = "A refined steel bar, stronger than iron.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition MithrilBar = new()
        {
            Id = "mithril_bar",
            Name = "Mithril Bar",
            Icon = "🔷",
            Type = "Material",
            Description = "A smelted mithril bar, light and incredibly strong.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition AdamantBar = new()
        {
            Id = "adamant_bar",
            Name = "Adamant Bar",
            Icon = "🟩",
            Type = "Material",
            Description = "A dense adamant bar forged under extreme heat.",
            SubCategory = "Smithing"
        };

        public static readonly ItemDefinition RuniteBar = new()
        {
            Id = "runite_bar",
            Name = "Runite Bar",
            Icon = "🔹",
            Type = "Material",
            Description = "A smelted runite bar of legendary quality.",
            SubCategory = "Smithing"
        };
    }
}