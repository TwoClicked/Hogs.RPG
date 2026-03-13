using Hogs.RPG.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hogs.RPG.Core.GameData.Equipment;

public static class Tier1Hunter
{

    public static readonly EquipmentDefinition BoneHelm = new()
    {
        Id = "bone_helm",
        Name = "Bone Helm",
        Slot = EquipmentSlot.Helmet,
        Defense = 2
    };

    public static readonly EquipmentDefinition LeatherVest = new()
    {
        Id = "leather_vest",
        Name = "Leather Vest",
        Slot = EquipmentSlot.Body,
        Defense = 4
    };

    public static readonly EquipmentDefinition HunterLeggings = new()
    {
        Id = "hunter_leggings",
        Name = "Hunter Leggings",
        Slot = EquipmentSlot.Legs,
        Defense = 3
    };

    public static readonly EquipmentDefinition FurGloves = new ()
    {
        Id = "fur_gloves",
        Name = "Fur Gloves",
        Slot = EquipmentSlot.Gloves,
        Defense = 2
    };

    public static readonly EquipmentDefinition HideBoots = new()
    {
        Id = "hide_boots",
        Name = "Hide Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 2
    };

    public static readonly EquipmentDefinition ClawDagger = new()
    {
        Id = "claw_dagger",
        Name = "Claw Dagger",
        Slot = EquipmentSlot.MainHand,
        Attack = 6
    };

    public static readonly EquipmentDefinition BoneBuckler = new()
    {
        Id = "bone_buckler",
        Name = "Bone Buckler",
        Slot = EquipmentSlot.OffHand,
        Defense = 6
    };

    public static readonly EquipmentDefinition FeatherBand = new()
    {
        Id = "feather_band",
        Name = "Feather Band",
        Slot = EquipmentSlot.Amulet,
        Attack = 4,
        Defense = 4
    };

    public static readonly EquipmentDefinition RavenCharm = new()
    {
        Id = "raven_charm",
        Name = "Raven Charm",
        Slot = EquipmentSlot.Ring,
        Health = 15
    };
}
