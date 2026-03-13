namespace Hogs.RPG.Core.GameData.Equipment;

using Hogs.RPG.Core.Entities;

public static class Tier3Warlord
{
    public static readonly EquipmentDefinition WarlordHelm = new()
    {
        Id = "warlord_helm",
        Name = "Warlord Helm",
        Slot = EquipmentSlot.Helmet,
        Defense = 8
    };

    public static readonly EquipmentDefinition WarlordArmor = new()
    {
        Id = "warlord_armor",
        Name = "Warlord Armor",
        Slot = EquipmentSlot.Body,
        Defense = 12
    };

    public static readonly EquipmentDefinition WarlordGreaves = new()
    {
        Id = "warlord_greaves",
        Name = "Warlord Greaves",
        Slot = EquipmentSlot.Legs,
        Defense = 10
    };

    public static readonly EquipmentDefinition ClawGauntlets = new()
    {
        Id = "claw_gauntlets",
        Name = "Claw Gauntlets",
        Slot = EquipmentSlot.Gloves,
        Defense = 6
    };

    public static readonly EquipmentDefinition SabertoothBoots = new()
    {
        Id = "sabertooth_boots",
        Name = "Sabertooth Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 6
    };

    public static readonly EquipmentDefinition SaberFangBlade = new()
    {
        Id = "saber_fang_blade",
        Name = "Saber Fang Blade",
        Slot = EquipmentSlot.MainHand,
        Attack = 18
    };

    public static readonly EquipmentDefinition AntlerShield = new()
    {
        Id = "antler_shield",
        Name = "Antler Shield",
        Slot = EquipmentSlot.OffHand,
        Defense = 14
    };

    public static readonly EquipmentDefinition GriffinBand = new()
    {
        Id = "griffin_band",
        Name = "Griffin Band",
        Slot = EquipmentSlot.Ring,
        Health = 60
    };

    public static readonly EquipmentDefinition RavenEyePendant = new()
    {
        Id = "raven_eye_pendant",
        Name = "Raven Eye Pendant",
        Slot = EquipmentSlot.Amulet,
        Attack = 9
    };
}