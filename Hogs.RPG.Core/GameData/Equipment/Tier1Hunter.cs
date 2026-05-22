using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
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
        Defense = 15
    };

    public static readonly EquipmentDefinition LeatherVest = new()
    {
        Id = "leather_vest",
        Name = "Leather Vest",
        Slot = EquipmentSlot.Body,
        Defense = 18
    };

    public static readonly EquipmentDefinition HunterLeggings = new()
    {
        Id = "leather_leggings",
        Name = "leather_leggings",
        Slot = EquipmentSlot.Legs,
        Defense = 16
    };

    public static readonly EquipmentDefinition FurGloves = new ()
    {
        Id = "fur_gloves",
        Name = "Fur Gloves",
        Slot = EquipmentSlot.Gloves,
        Defense = 14
    };

    public static readonly EquipmentDefinition HideBoots = new()
    {
        Id = "hide_boots",
        Name = "Hide Boots",
        Slot = EquipmentSlot.Boots,
        Defense = 12
    };

    public static readonly EquipmentDefinition ClawDagger = new()
    {
        Id = "claw_dagger",
        Name = "Claw Dagger",
        Slot = EquipmentSlot.MainHand,
        Attack = 15
    };

    public static readonly EquipmentDefinition BoneBuckler = new()
    {
        Id = "bone_buckler",
        Name = "Bone Buckler",
        Slot = EquipmentSlot.OffHand,
        Health = 35,
        Defense = 10
    };

    public static readonly EquipmentDefinition FeatherBand = new()
    {
        Id = "feather_band",
        Name = "Feather Band",
        Slot = EquipmentSlot.Amulet,
        Health = 50,
        Defense = 5,
        Attack = 5
    };

    public static readonly EquipmentDefinition RavenCharm = new()
    {
        Id = "raven_charm",
        Name = "Raven Charm",
        Slot = EquipmentSlot.Ring,
        Health = 50,
        Defense = 5,
        Attack = 3
    };
}
