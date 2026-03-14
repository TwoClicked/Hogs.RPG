using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class MaterialItems
    {
        public static readonly ItemDefinition Herb = new()
        {
            Id = "herb",
            Name = "Herb",
            Icon = "🌿",
            Type = "Material",
            Description = "A common herb used in alchemy."
        };

        public static readonly ItemDefinition RedMushroom = new()
        {
            Id = "red_mushroom",
            Name = "Red Mushroom",
            Icon = "🍄",
            Type = "Material",
            Description = "A strange mushroom used in potion brewing."
        };

        public static readonly ItemDefinition CrystalLeaf = new()
        {
            Id = "crystal_leaf",
            Name = "Crystal Leaf",
            Icon = "🍃",
            Type = "Material",
            Description = "A magical glowing leaf."
        };
    }
}