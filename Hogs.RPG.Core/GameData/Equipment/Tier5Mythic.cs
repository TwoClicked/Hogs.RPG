using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Enums;

public static class Tier5Mythic
{
    public static readonly EquipmentDefinition MythicCrown = new()
    {
        Id = "mythic_crown",
        Name = "Mythic Crown",
        Slot = EquipmentSlot.Helmet,
        Defense = 45,
        Attack = 18
    };

    public static readonly EquipmentDefinition BeastslayerPlate = new()
    {
        Id = "beastslayer_plate",
        Name = "Beastslayer Plate",
        Slot = EquipmentSlot.Body,
        Defense = 58
    };

    public static readonly EquipmentDefinition ColossusLegguards = new()
    {
        Id = "colossus_legguards",
        Name = "Colossus Legguards",
        Slot = EquipmentSlot.Legs,
        Defense = 50
    };

    public static readonly EquipmentDefinition AbyssGauntlets = new()
    {
        Id = "abyss_gauntlets",
        Name = "Abyss Gauntlets",
        Slot = EquipmentSlot.Gloves,
        Defense = 42
    };

    public static readonly EquipmentDefinition SkystriderBoots = new()
    {
        Id = "skystrider_boots",
        Name = "Skystrider Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 40
    };

    public static readonly EquipmentDefinition WorldbreakerBlade = new()
    {
        Id = "worldbreaker_blade",
        Name = "Worldbreaker Blade",
        Slot = EquipmentSlot.MainHand,
        Attack = 48
    };

    public static readonly EquipmentDefinition ColossusShield = new()
    {
        Id = "colossus_shield",
        Name = "Colossus Shield",
        Slot = EquipmentSlot.OffHand,
        Health = 130,
        Defense = 35
    };

    public static readonly EquipmentDefinition RavenKingBand = new()
    {
        Id = "raven_king_band",
        Name = "Raven King Band",
        Slot = EquipmentSlot.Ring,
        Health = 150,
        Defense = 14,
        Attack = 8
    };

    public static readonly EquipmentDefinition PendantOfTheWild = new()
    {
        Id = "pendant_of_the_wild",
        Name = "Pendant of the Wild",
        Slot = EquipmentSlot.Amulet,
        Health = 150,
        Defense = 14,
        Attack = 20
    };
}