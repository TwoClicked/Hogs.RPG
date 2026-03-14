using Hogs.RPG.Core.Entities;

namespace Hogs.RPG.Core.GameData.Recipes;

public static class RecipeRegistry
{
    public static readonly Dictionary<string, Recipe> All = new()
    {

        //TIER1 RECIPES
        { Tier1HunterRecipes.ClawDagger.Id, Tier1HunterRecipes.ClawDagger },
        { Tier1HunterRecipes.BoneBuckler.Id, Tier1HunterRecipes.BoneBuckler },
        { Tier1HunterRecipes.LeatherVest.Id, Tier1HunterRecipes.LeatherVest },
        { Tier1HunterRecipes.BoneHelm.Id, Tier1HunterRecipes.BoneHelm },
        { Tier1HunterRecipes.HunterLeggings.Id, Tier1HunterRecipes.HunterLeggings },
        { Tier1HunterRecipes.HideBoots.Id, Tier1HunterRecipes.HideBoots },
        { Tier1HunterRecipes.FurGloves.Id, Tier1HunterRecipes.FurGloves },
        { Tier1HunterRecipes.FeatherBand.Id, Tier1HunterRecipes.FeatherBand },
        { Tier1HunterRecipes.RavenCharm.Id, Tier1HunterRecipes.RavenCharm },


        //TIER2 RECIPES 
        { Tier2RaiderRecipes.FangBlade.Id, Tier2RaiderRecipes.FangBlade },
        { Tier2RaiderRecipes.HideWarcoat.Id, Tier2RaiderRecipes.HideWarcoat },
        { Tier2RaiderRecipes.HornShield.Id, Tier2RaiderRecipes.HornShield },
        { Tier2RaiderRecipes.RaiderBand.Id, Tier2RaiderRecipes.RaiderBand },
        { Tier2RaiderRecipes.RaiderBoots.Id, Tier2RaiderRecipes.RaiderBoots },
        { Tier2RaiderRecipes.RaiderHelm.Id, Tier2RaiderRecipes.RaiderHelm },
        { Tier2RaiderRecipes.TalonCharm.Id, Tier2RaiderRecipes.TalonCharm },
        { Tier2RaiderRecipes.RaiderLegguards.Id, Tier2RaiderRecipes.RaiderLegguards },
        { Tier2RaiderRecipes.TrackerGloves.Id, Tier2RaiderRecipes.TrackerGloves },

        //TIER3 RECIPES
        { Tier3WarlordRecipes.SaberFangBlade.Id, Tier3WarlordRecipes.SaberFangBlade },
        { Tier3WarlordRecipes.WarlordArmor.Id, Tier3WarlordRecipes.WarlordArmor },
        { Tier3WarlordRecipes.AntlerShield.Id, Tier3WarlordRecipes.AntlerShield },
        { Tier3WarlordRecipes.GriffinBand.Id, Tier3WarlordRecipes.GriffinBand },
        { Tier3WarlordRecipes.SabertoothBoots.Id, Tier3WarlordRecipes.SabertoothBoots },
        { Tier3WarlordRecipes.WarlordHelm.Id, Tier3WarlordRecipes.WarlordHelm },
        { Tier3WarlordRecipes.WarlordGreaves.Id, Tier3WarlordRecipes.WarlordGreaves },
        { Tier3WarlordRecipes.RavenEyePendant.Id, Tier3WarlordRecipes.RavenEyePendant },
        { Tier3WarlordRecipes.ClawGauntlets.Id, Tier3WarlordRecipes.ClawGauntlets },

        //TIER4 RECIPES
        { Tier4ChampionRecipes.TitanBlade.Id, Tier4ChampionRecipes.TitanBlade },
        { Tier4ChampionRecipes.ChampionPlate.Id, Tier4ChampionRecipes.ChampionPlate },
        { Tier4ChampionRecipes.TitanShield.Id, Tier4ChampionRecipes.TitanShield },
        { Tier4ChampionRecipes.StormRing.Id, Tier4ChampionRecipes.StormRing },
        { Tier4ChampionRecipes.ShadowstepBoots.Id, Tier4ChampionRecipes.ShadowstepBoots },
        { Tier4ChampionRecipes.ChampionHelm.Id, Tier4ChampionRecipes.ChampionHelm },
        { Tier4ChampionRecipes.ChampionGreaves.Id, Tier4ChampionRecipes.ChampionGreaves },
        { Tier4ChampionRecipes.VoidPendant.Id, Tier4ChampionRecipes.VoidPendant },
        { Tier4ChampionRecipes.StormGauntlets.Id, Tier4ChampionRecipes.StormGauntlets },

        //TIER5 RECIPES
        { Tier5MythicRecipes.WorldbreakerBlade.Id, Tier5MythicRecipes.WorldbreakerBlade },
        { Tier5MythicRecipes.BeastslayerPlate.Id, Tier5MythicRecipes.BeastslayerPlate },
        { Tier5MythicRecipes.ColossusShield.Id, Tier5MythicRecipes.ColossusShield },
        { Tier5MythicRecipes.RavenKingBand.Id, Tier5MythicRecipes.RavenKingBand },
        { Tier5MythicRecipes.SkystriderBoots.Id, Tier5MythicRecipes.SkystriderBoots },
        { Tier5MythicRecipes.MythicCrown.Id, Tier5MythicRecipes.MythicCrown },
        { Tier5MythicRecipes.ColossusLegguards.Id, Tier5MythicRecipes.ColossusLegguards },
        { Tier5MythicRecipes.PendantOfTheWild.Id, Tier5MythicRecipes.PendantOfTheWild },
        { Tier5MythicRecipes.AbyssGauntlets.Id, Tier5MythicRecipes.AbyssGauntlets },

        //ALCHEMY RECIPES
        { AlchemyRecipes.XpPotion.Id, AlchemyRecipes.XpPotion },
        { AlchemyRecipes.HealthPotion.Id, AlchemyRecipes.HealthPotion },

    };
}