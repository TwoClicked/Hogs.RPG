using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;

public static class GlobalBossGear
{
    // ⚔️ PURE DAMAGE
    public static readonly EquipmentDefinition AureliusSword = new()
    {
        Id = "aurelius_sword",
        Name = "Blade of Aurelius",
        Slot = EquipmentSlot.MainHand,
        Attack = 75,
        Defense = 5,
        Health = 20
    };

    // 🛡️ DEFENSE FOCUSED (Tank chest)
    public static readonly EquipmentDefinition XerathulArmor = new()
    {
        Id = "xerathul_armor",
        Name = "Xerathul Abyss Plate",
        Slot = EquipmentSlot.Body,
        Attack = 5,
        Defense = 65,
        Health = 110
    };

    // 🪨 DEFENSE + BRUISER OFFHAND
    public static readonly EquipmentDefinition GravelmawShield = new()
    {
        Id = "gravelmaw_shield",
        Name = "Gravelmaw Bulwark",
        Slot = EquipmentSlot.OffHand,
        Attack = 20,
        Defense = 60,
        Health = 70
    };

    // 🐍 HP / SUSTAIN
    public static readonly EquipmentDefinition SerpentGloves = new()
    {
        Id = "serpent_gloves",
        Name = "Serpent Fang Gloves",
        Slot = EquipmentSlot.Gloves,
        Attack = 8,
        Defense = 12,
        Health = 90
    };

    // 👑 HYBRID LEADER ITEM
    public static readonly EquipmentDefinition TyrHelm = new()
    {
        Id = "tyr_helm",
        Name = "Helm of the High Overseer",
        Slot = EquipmentSlot.Helmet,
        Attack = 25,
        Defense = 35,
        Health = 100
    };

    // 🩸 BRUISER LEGS (balanced offense/defense)
    public static readonly EquipmentDefinition ThrolakLeggings = new()
    {
        Id = "thorlak_leggings",
        Name = "Bloodbreaker WarLeggings",
        Slot = EquipmentSlot.Legs,
        Attack = 18,
        Defense = 28,
        Health = 85
    };

    // 💍 HP HEAVY + SUPPORT
    public static readonly EquipmentDefinition PunisherRing = new()
    {
        Id = "punisher_ring",
        Name = "Seal of Collection",
        Slot = EquipmentSlot.Ring,
        Attack = 20,
        Defense = 20,
        Health = 160
    };

    // 🔮 DAMAGE AMULET
    public static readonly EquipmentDefinition GullveigAmulet = new()
    {
        Id = "gullveig_amulet",
        Name = "Seiðr-Touched Amulet",
        Slot = EquipmentSlot.Amulet,
        Attack = 45,
        Defense = 10,
        Health = 120
    };
}