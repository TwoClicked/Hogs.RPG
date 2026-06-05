using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Equipment
{
    public static class HunterTrackerGear
    {
        // =========================
        // Each piece gives +0.5% XP and +0.5% materials on /hunt
        // Full 9-piece set bonus: +5% rare material drop rate
        // All pieces are shop-exclusive — bought with Tracker Tokens
        // =========================

        public static readonly EquipmentDefinition HunterHelm = new()
        {
            Id = "hunter_helm",
            Name = "Hunter's Helm",
            Slot = EquipmentSlot.Helmet,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterVest = new()
        {
            Id = "hunter_vest",
            Name = "Hunter's Vest",
            Slot = EquipmentSlot.Body,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterLeggings = new()
        {
            Id = "hunter_leggings",
            Name = "Hunter's Leggings",
            Slot = EquipmentSlot.Legs,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterGloves = new()
        {
            Id = "hunter_gloves",
            Name = "Hunter's Gloves",
            Slot = EquipmentSlot.Gloves,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterBoots = new()
        {
            Id = "hunter_boots",
            Name = "Hunter's Boots",
            Slot = EquipmentSlot.Boots,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterBow = new()
        {
            Id = "hunter_bow",
            Name = "Hunter's Bow",
            Slot = EquipmentSlot.MainHand,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterQuiver = new()
        {
            Id = "hunter_quiver",
            Name = "Hunter's Quiver",
            Slot = EquipmentSlot.OffHand,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterRing = new()
        {
            Id = "hunter_ring",
            Name = "Hunter's Ring",
            Slot = EquipmentSlot.Ring,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };

        public static readonly EquipmentDefinition HunterAmulet = new()
        {
            Id = "hunter_amulet",
            Name = "Hunter's Amulet",
            Slot = EquipmentSlot.Amulet,
            HuntXpBonus = 0.005,
            HuntMaterialBonus = 0.005,
            IsHuntingGear = true
        };
    }
}