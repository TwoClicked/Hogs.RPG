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
        Health = 70
    };
    public static readonly EquipmentDefinition TyrHelm = new()
    {
        Id = "tyr_helm",
        Name = "Helm of the High Overseer",
        Slot = EquipmentSlot.Helmet,
        Attack = 25,
        Defense = 35,
        Health = 100
    };

    public static readonly EquipmentDefinition ThrolakLeggings = new()
    {
        Id = "thorlak_leggings",
        Name = "Bloodbreaker WarLeggings",
        Slot = EquipmentSlot.Legs,
        Attack = 15,
        Defense = 20,
        Health = 80
    };

    public static readonly EquipmentDefinition PunisherRing = new()
    {
        Id = "punisher_ring",
        Name = "Seal of Collection",
        Slot = EquipmentSlot.Ring,
        Attack = 12,
        Defense = 8,
        Health = 120
    };

    public static readonly EquipmentDefinition GullveigAmulet = new()
    {
        Id = "gullveig_amulet",
        Name = "Seiðr-Touched Amulet",
        Slot = EquipmentSlot.Amulet,
        Attack = 20,
        Defense = 10,
        Health = 60
    };
}