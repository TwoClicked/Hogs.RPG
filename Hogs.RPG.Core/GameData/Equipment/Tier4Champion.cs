namespace Hogs.RPG.Core.GameData.Equipment;

using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class Tier4Champion
{
    public static readonly EquipmentDefinition ChampionHelm = new()
    {
        Id = "champion_helm",
        Name = "Champion Helm",
        Slot = EquipmentSlot.Helmet,
        Defense = 12
    };

    public static readonly EquipmentDefinition ChampionPlate = new()
    {
        Id = "champion_plate",
        Name = "Champion Plate",
        Slot = EquipmentSlot.Body,
        Defense = 18
    };

    public static readonly EquipmentDefinition ChampionGreaves = new()
    {
        Id = "champion_greaves",
        Name = "Champion Greaves",
        Slot = EquipmentSlot.Legs,
        Defense = 15
    };

    public static readonly EquipmentDefinition StormGauntlets = new()
    {
        Id = "storm_gauntlets",
        Name = "Storm Gauntlets",
        Slot = EquipmentSlot.Gloves,
        Defense = 10
    };

    public static readonly EquipmentDefinition ShadowstepBoots = new()
    {
        Id = "shadowstep_boots",
        Name = "Shadowstep Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 10
    };

    public static readonly EquipmentDefinition TitanBlade = new()
    {
        Id = "titan_blade",
        Name = "Titan Blade",
        Slot = EquipmentSlot.MainHand,
        Attack = 25
    };

    public static readonly EquipmentDefinition TitanShield = new()
    {
        Id = "titan_shield",
        Name = "Titan Shield",
        Slot = EquipmentSlot.OffHand,
        Defense = 20
    };

    public static readonly EquipmentDefinition StormRing = new()
    {
        Id = "storm_ring",
        Name = "Storm Ring",
        Slot = EquipmentSlot.Ring,
        Health = 90
    };

    public static readonly EquipmentDefinition VoidPendant = new()
    {
        Id = "void_pendant",
        Name = "Void Pendant",
        Slot = EquipmentSlot.Amulet,
        Attack = 14
    };
}