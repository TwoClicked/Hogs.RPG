using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class GlobalBossGear
{
    public static readonly EquipmentDefinition AureliusSword = new()
    {
        Id = "aurelius_sword",
        Name = "Blade of Aurelius",
        Slot = EquipmentSlot.MainHand,
        Attack = 75,
        Defense = 10,
        Health = 35
    };

    public static readonly EquipmentDefinition XerathulArmor = new()
    {
        Id = "xerathul_armor",
        Name = "Xerathul Abyss Plate",
        Slot = EquipmentSlot.Body,
        Attack = 10,
        Defense = 50,
        Health = 100
    };

    public static readonly EquipmentDefinition GravelmawShield = new()
    {
        Id = "gravelmaw_shield",
        Name = "Gravelmaw Bulwark",
        Slot = EquipmentSlot.OffHand,
        Attack = 40,
        Defense = 60,
        Health = 50
    };

    public static readonly EquipmentDefinition SerpentGloves = new()
    {
        Id = "serpent_gloves",
        Name = "Serpent Fang Gloves",
        Slot = EquipmentSlot.Gloves,
        Attack = 10,
        Defense = 10,
        Health = 150
    };
}