using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Equipment;

public static class EquipmentRegistry
{
    public static readonly Dictionary<string, EquipmentDefinition> All = new()
    {
        { Tier1Hunter.ClawDagger.Id, Tier1Hunter.ClawDagger },
        { Tier1Hunter.BoneBuckler.Id, Tier1Hunter.BoneBuckler },
        { Tier1Hunter.BoneHelm.Id, Tier1Hunter.BoneHelm },
        { Tier1Hunter.LeatherVest.Id, Tier1Hunter.LeatherVest },
        { Tier1Hunter.HunterLeggings.Id, Tier1Hunter.HunterLeggings },
        { Tier1Hunter.HideBoots.Id, Tier1Hunter.HideBoots },
        { Tier1Hunter.FurGloves.Id, Tier1Hunter.FurGloves },
        { Tier1Hunter.FeatherBand.Id, Tier1Hunter.FeatherBand },
        { Tier1Hunter.RavenCharm.Id, Tier1Hunter.RavenCharm }
    };
}