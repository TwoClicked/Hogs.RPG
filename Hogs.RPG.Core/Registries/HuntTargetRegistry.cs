using Hogs.RPG.Core.Entities.GameLoopObjects;

namespace Hogs.RPG.GameData.Hunts;

public static class HuntTargetRegistry
{
    public static readonly Dictionary<string, HuntTarget> All = new()
    {

        //TIER1 HUNTS
        { ForestTargets.Wolf.Id, ForestTargets.Wolf },
        { ForestTargets.Pig.Id, ForestTargets.Pig },
        { ForestTargets.Turkey.Id, ForestTargets.Turkey },
        { ForestTargets.Raccoon.Id, ForestTargets.Raccoon },
        { ForestTargets.Fox.Id, ForestTargets.Fox },
        { ForestTargets.BloodBoar.Id, ForestTargets.BloodBoar },

        //TIER2 HUNTS
        { WildTargets.Eagle.Id, WildTargets.Eagle },
        { WildTargets.Bear.Id, WildTargets.Bear },
        { WildTargets.Bull.Id, WildTargets.Bull },
        { WildTargets.Shark.Id, WildTargets.Shark },
        { WildTargets.Lynx.Id, WildTargets.Lynx },

        //TIER3 HUNTS
        { DeepTargets.Sabertooth.Id, DeepTargets.Sabertooth },
        { DeepTargets.Griffin.Id, DeepTargets.Griffin },
        { DeepTargets.WarElk.Id, DeepTargets.WarElk },
        { DeepTargets.DireBear.Id, DeepTargets.DireBear },
        { DeepTargets.ShadowRaven.Id, DeepTargets.ShadowRaven },

        //TIER4 HUNTS
        { StormTargets.StormEagle.Id, StormTargets.StormEagle },
        { StormTargets.AncientBear.Id, StormTargets.AncientBear },
        { StormTargets.NightStalker.Id, StormTargets.NightStalker },
        { StormTargets.TitanElk.Id, StormTargets.TitanElk },
        { StormTargets.VoidRaven.Id, StormTargets.VoidRaven },

        //TIER5 HUNTS
        { MythicTargets.MythicBear.Id, MythicTargets.MythicBear },
        { MythicTargets.SkyTyrant.Id, MythicTargets.SkyTyrant },
        { MythicTargets.AbyssStalker.Id, MythicTargets.AbyssStalker },
        { MythicTargets.ColossusElk.Id, MythicTargets.ColossusElk },
        { MythicTargets.DeathRaven.Id, MythicTargets.DeathRaven },
    };
}