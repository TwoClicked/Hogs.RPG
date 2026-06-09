using Hogs.RPG.Core.Entities.EquipmentObjects;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.GameData.Equipment
{
    public class DungeonBossGear
    {
        // =========================
        // LEVEL 5 - ROT FATHER MALCHOR (NEW)
        // =========================
        public static readonly EquipmentDefinition MalchorGrips = new()
        {
            Id = "malchor_grips",
            Name = "Malchor's Blightweave Grips",
            Slot = EquipmentSlot.Gloves,
            Defense = 12,
            Attack = 12,
            Health = 50
        };

        // =========================
        // LEVEL 10 - FANCULO
        // =========================
        public static readonly EquipmentDefinition FanculoHelm = new()
        {
            Id = "fanculo_helm",
            Name = "Fanculo Horned Helm",
            Slot = EquipmentSlot.Helmet,
            Defense = 20,
            Attack = 20,
            Health = 75
        };

        // =========================
        // LEVEL 15 - HROTHGAR
        // =========================
        public static readonly EquipmentDefinition HrothgarRing = new()
        {
            Id = "hrothgar_ring",
            Name = "Hrothgar's Frostbound Ring",
            Slot = EquipmentSlot.Ring,
            Attack = 45,
        };

        // =========================
        // LEVEL 17 - AURELION THE OATHBREAKER
        // =========================
        public static readonly EquipmentDefinition OathcrushLegguards = new()
        {
            Id = "oathcrush_legguards",
            Name = "Oathcrush Legguards",
            Slot = EquipmentSlot.Legs,
            Defense = 28,
            Attack = 30,
            Health = 110
        };

        // =========================
        // LEVEL 18 - AMPHIVOS TATEROUS
        // =========================
        public static readonly EquipmentDefinition TaterousBattleaxe = new()
        {
            Id = "taterous_battleaxe",
            Name = "Taterous Battleaxe",
            Slot = EquipmentSlot.MainHand,
            Defense = 8,
            Attack = 34,
            Health = 115
        };

        // =========================
        // LEVEL 20 - LUMINARA
        // =========================
        public static readonly EquipmentDefinition LuminaraAmulet = new()
        {
            Id = "luminara_amulet",
            Name = "Luminara's Moonlit Amulet",
            Slot = EquipmentSlot.Amulet,
            Defense = 10,
            Attack = 35,
            Health = 120
        };

        // =========================
        // LEVEL 22 - SKARR THE CLOWN OF CARNAGE
        // =========================
        public static readonly EquipmentDefinition SkarrSawbladeshield = new()
        {
            Id = "skarr_sawbladeshield",
            Name = "Skarr's Sawblade Shield",
            Slot = EquipmentSlot.OffHand,
            Defense = 32,
            Attack = 38,
            Health = 132
        };

        // =========================
        // LEVEL 23 - GRIMBLEX THE GILDED TYRANT
        // =========================
        public static readonly EquipmentDefinition ShadowsaphireSignet = new()
        {
            Id = "shadowsaphire_signet",
            Name = "Shadowsaphire Signet",
            Slot = EquipmentSlot.Ring,
            Defense = 40,
            Health = 250
        };

        // =========================
        // LEVEL 25 - THORKELL
        // =========================
        public static readonly EquipmentDefinition ThorkellBoots = new()
        {
            Id = "thorkell_boots",
            Name = "Boots of the Warborn",
            Slot = EquipmentSlot.Boots,
            Defense = 25,
            Attack = 40,
            Health = 150
        };

        // =========================
        // LEVEL 27 - GRITCH THE GILDED PROWLER
        // =========================
        public static readonly EquipmentDefinition GritchWarplate = new()
        {
            Id = "gritch_warplate",
            Name = "Gritch's Star-Iron Warplate",
            Slot = EquipmentSlot.Body,
            Defense = 5,
            Attack = 50,
            Health = 75
        };
    }
}
