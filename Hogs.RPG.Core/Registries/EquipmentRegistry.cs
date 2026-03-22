using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Equipment;

public static class EquipmentRegistry
{
    public static readonly Dictionary<string, EquipmentDefinition> All = new()
    {

        //TIER1 GEAR
        { Tier1Hunter.ClawDagger.Id, Tier1Hunter.ClawDagger },
        { Tier1Hunter.BoneBuckler.Id, Tier1Hunter.BoneBuckler },
        { Tier1Hunter.BoneHelm.Id, Tier1Hunter.BoneHelm },
        { Tier1Hunter.LeatherVest.Id, Tier1Hunter.LeatherVest },
        { Tier1Hunter.HunterLeggings.Id, Tier1Hunter.HunterLeggings },
        { Tier1Hunter.HideBoots.Id, Tier1Hunter.HideBoots },
        { Tier1Hunter.FurGloves.Id, Tier1Hunter.FurGloves },
        { Tier1Hunter.FeatherBand.Id, Tier1Hunter.FeatherBand },
        { Tier1Hunter.RavenCharm.Id, Tier1Hunter.RavenCharm },

        //TIER2 GEAR
        { Tier2Raider.FangBlade.Id, Tier2Raider.FangBlade },
        { Tier2Raider.HideWarcoat.Id, Tier2Raider.HideWarcoat },
        { Tier2Raider.HornShield.Id, Tier2Raider.HornShield },
        { Tier2Raider.RaiderBand.Id, Tier2Raider.RaiderBand },
        { Tier2Raider.RaiderBoots.Id, Tier2Raider.RaiderBoots },
        { Tier2Raider.RaiderHelm.Id, Tier2Raider.RaiderHelm },
        { Tier2Raider.RaiderLegguards.Id, Tier2Raider.RaiderLegguards },
        { Tier2Raider.TalonCharm.Id, Tier2Raider.TalonCharm },
        { Tier2Raider.TrackerGloves.Id, Tier2Raider.TrackerGloves },

        //TIER3 GEAR
        { Tier3Warlord.SaberFangBlade.Id, Tier3Warlord.SaberFangBlade },
        { Tier3Warlord.WarlordArmor.Id, Tier3Warlord.WarlordArmor },
        { Tier3Warlord.AntlerShield.Id, Tier3Warlord.AntlerShield },
        { Tier3Warlord.GriffinBand.Id, Tier3Warlord.GriffinBand },
        { Tier3Warlord.SabertoothBoots.Id, Tier3Warlord.SabertoothBoots },
        { Tier3Warlord.WarlordHelm.Id, Tier3Warlord.WarlordHelm },
        { Tier3Warlord.WarlordGreaves.Id, Tier3Warlord.WarlordGreaves },
        { Tier3Warlord.RavenEyePendant.Id, Tier3Warlord.RavenEyePendant },
        { Tier3Warlord.ClawGauntlets.Id, Tier3Warlord.ClawGauntlets },

        //TIER4 GEAR
        { Tier4Champion.TitanBlade.Id, Tier4Champion.TitanBlade },
        { Tier4Champion.ChampionPlate.Id, Tier4Champion.ChampionPlate },
        { Tier4Champion.TitanShield.Id, Tier4Champion.TitanShield },
        { Tier4Champion.StormRing.Id, Tier4Champion.StormRing },
        { Tier4Champion.ShadowstepBoots.Id, Tier4Champion.ShadowstepBoots },
        { Tier4Champion.ChampionHelm.Id, Tier4Champion.ChampionHelm },
        { Tier4Champion.ChampionGreaves.Id, Tier4Champion.ChampionGreaves },
        { Tier4Champion.VoidPendant.Id, Tier4Champion.VoidPendant },
        { Tier4Champion.StormGauntlets.Id, Tier4Champion.StormGauntlets },

        //TIER5 GEAR
        { Tier5Mythic.WorldbreakerBlade.Id, Tier5Mythic.WorldbreakerBlade },
        { Tier5Mythic.BeastslayerPlate.Id, Tier5Mythic.BeastslayerPlate },
        { Tier5Mythic.ColossusShield.Id, Tier5Mythic.ColossusShield },
        { Tier5Mythic.RavenKingBand.Id, Tier5Mythic.RavenKingBand },
        { Tier5Mythic.SkystriderBoots.Id, Tier5Mythic.SkystriderBoots },
        { Tier5Mythic.MythicCrown.Id, Tier5Mythic.MythicCrown },
        { Tier5Mythic.ColossusLegguards.Id, Tier5Mythic.ColossusLegguards },
        { Tier5Mythic.PendantOfTheWild.Id, Tier5Mythic.PendantOfTheWild },
        { Tier5Mythic.AbyssGauntlets.Id, Tier5Mythic.AbyssGauntlets },

        //FANCULO GEAR
        {FanculoGear.FanculoHelm.Id, FanculoGear.FanculoHelm }
    };
}