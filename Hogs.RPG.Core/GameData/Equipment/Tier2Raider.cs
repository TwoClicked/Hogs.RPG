using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Enums;

public static class Tier2Raider
{
    public static readonly EquipmentDefinition RaiderHelm = new()
    {
        Id = "raider_helm",
        Name = "Raider Helm",
        Slot = EquipmentSlot.Helmet,
        Defense = 20
    };

    public static readonly EquipmentDefinition HideWarcoat = new()
    {
        Id = "hide_warcoat",
        Name = "Hide Warcoat",
        Slot = EquipmentSlot.Body,
        Defense = 24
    };

    public static readonly EquipmentDefinition RaiderLegguards = new()
    {
        Id = "raider_legguards",
        Name = "Raider Legguards",
        Slot = EquipmentSlot.Legs,
        Defense = 21
    };

    public static readonly EquipmentDefinition TrackerGloves = new()
    {
        Id = "tracker_gloves",
        Name = "Tracker Gloves",
        Slot = EquipmentSlot.Gloves,
        Defense = 18
    };

    public static readonly EquipmentDefinition RaiderBoots = new()
    {
        Id = "raider_boots",
        Name = "Raider Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 16
    };

    public static readonly EquipmentDefinition FangBlade = new()
    {
        Id = "fang_blade",
        Name = "Fang Blade",
        Slot = EquipmentSlot.MainHand,
        Attack = 20
    };

    public static readonly EquipmentDefinition HornShield = new()
    {
        Id = "horn_shield",
        Name = "Horn Shield",
        Slot = EquipmentSlot.OffHand,
        Health = 50,
        Defense = 14
    };

    public static readonly EquipmentDefinition RaiderBand = new()
    {
        Id = "raider_band",
        Name = "Raider Band",
        Slot = EquipmentSlot.Ring,
        Health = 65,
        Defense = 6,
        Attack = 4
    };

    public static readonly EquipmentDefinition TalonCharm = new()
    {
        Id = "talon_charm",
        Name = "Talon Charm",
        Slot = EquipmentSlot.Amulet,
        Health = 65,
        Defense = 6,
        Attack = 7
    };
}