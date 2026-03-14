namespace Hogs.RPG.Core.GameData.Equipment;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class Tier2Raider
{
    public static readonly EquipmentDefinition RaiderHelm = new()
    {
        Id = "raider_helm",
        Name = "Raider Helm",
        Slot = EquipmentSlot.Helmet,
        Defense = 5
    };

    public static readonly EquipmentDefinition HideWarcoat = new()
    {
        Id = "hide_warcoat",
        Name = "Hide Warcoat",
        Slot = EquipmentSlot.Body,
        Defense = 8
    };

    public static readonly EquipmentDefinition RaiderLegguards = new()
    {
        Id = "raider_legguards",
        Name = "Raider Legguards",
        Slot = EquipmentSlot.Legs,
        Defense = 6
    };

    public static readonly EquipmentDefinition TrackerGloves = new()
    {
        Id = "tracker_gloves",
        Name = "Tracker Gloves",
        Slot = EquipmentSlot.Gloves,
        Defense = 4
    };

    public static readonly EquipmentDefinition RaiderBoots = new()
    {
        Id = "raider_boots",
        Name = "Raider Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 4
    };

    public static readonly EquipmentDefinition FangBlade = new()
    {
        Id = "fang_blade",
        Name = "Fang Blade",
        Slot = EquipmentSlot.MainHand,
        Attack = 12
    };

    public static readonly EquipmentDefinition HornShield = new()
    {
        Id = "horn_shield",
        Name = "Horn Shield",
        Slot = EquipmentSlot.OffHand,
        Defense = 10
    };

    public static readonly EquipmentDefinition RaiderBand = new()
    {
        Id = "raider_band",
        Name = "Raider Band",
        Slot = EquipmentSlot.Ring,
        Health = 35
    };

    public static readonly EquipmentDefinition TalonCharm = new()
    {
        Id = "talon_charm",
        Name = "Talon Charm",
        Slot = EquipmentSlot.Amulet,
        Attack = 6
    };
}