using Hogs.RPG.Core.Entities;
namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class MaterialItems
    {
        public static readonly ItemDefinition Herb = new()
        {
            Id = "herb",
            Name = "Herb",
            Icon = "<:herbs:1485050887008288888>",
            Type = "Material",
            Description = "A common herb used in alchemy.",
            SubCategory = "Alchemy"
        };
        public static readonly ItemDefinition RedMushroom = new()
        {
            Id = "red_mushroom",
            Name = "Red Mushroom",
            Icon = "<:red_mushroom:1484989913739825173>",
            Type = "Material",
            Description = "A strange mushroom used in potion brewing.",
            SubCategory = "Alchemy"
        };
        public static readonly ItemDefinition CrystalLeaf = new()
        {
            Id = "crystal_leaf",
            Name = "Crystal Leaf",
            Icon = "<:crystal_leaf:1484990368427540561>",
            Type = "Material",
            Description = "A magical glowing leaf.",
            SubCategory = "Alchemy"
        };
    }
}