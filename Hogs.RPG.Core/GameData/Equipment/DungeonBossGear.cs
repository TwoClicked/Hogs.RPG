using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Equipment
{
    public class DungeonBossGear // Rename this to DungeonGear on restart
    {
        // =========================
        // LEVEL 10 FANCULO
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
            Defense = 15,
            Attack = 25,
            Health = 100
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
    }
}