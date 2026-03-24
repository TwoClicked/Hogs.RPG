using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.Equipment;
using System.Collections.Generic;

namespace Hogs.RPG.Core.GameData.InventoryItems
{
    public static class InventoryItemDefinitions
    {
        public static readonly Dictionary<string, ItemDefinition> All = new()
        {
            // ===== Materials =====

            { MaterialItems.Herb.Id, MaterialItems.Herb },
            { MaterialItems.RedMushroom.Id, MaterialItems.RedMushroom },
            { MaterialItems.CrystalLeaf.Id, MaterialItems.CrystalLeaf },

            // ===== Potions =====

            { PotionItems.XpPotion.Id, PotionItems.XpPotion },
            { PotionItems.HealthPotion.Id, PotionItems.HealthPotion },

            // ===== Hunt Drops =====

            { HuntDropItems.Fur.Id, HuntDropItems.Fur },
            { HuntDropItems.Leather.Id, HuntDropItems.Leather },
            { HuntDropItems.Bone.Id, HuntDropItems.Bone },
            { HuntDropItems.Feather.Id, HuntDropItems.Feather },
            { HuntDropItems.Claws.Id, HuntDropItems.Claws },
            { HuntDropItems.MonsterBlood.Id, HuntDropItems.MonsterBlood },

            { HuntDropItems.Hide.Id, HuntDropItems.Hide },
            { HuntDropItems.Fang.Id, HuntDropItems.Fang },
            { HuntDropItems.Talon.Id, HuntDropItems.Talon },
            { HuntDropItems.Horn.Id, HuntDropItems.Horn },
            { HuntDropItems.SharpClaw.Id, HuntDropItems.SharpClaw },

            { HuntDropItems.SaberFang.Id, HuntDropItems.SaberFang },
            { HuntDropItems.GriffinFeather.Id, HuntDropItems.GriffinFeather },
            { HuntDropItems.GiantAntler.Id, HuntDropItems.GiantAntler },
            { HuntDropItems.ThickHide.Id, HuntDropItems.ThickHide },
            { HuntDropItems.DarkFeather.Id, HuntDropItems.DarkFeather },

            { HuntDropItems.StormFeather.Id, HuntDropItems.StormFeather },
            { HuntDropItems.AncientHide.Id, HuntDropItems.AncientHide },
            { HuntDropItems.ShadowClaw.Id, HuntDropItems.ShadowClaw },
            { HuntDropItems.TitanAntler.Id, HuntDropItems.TitanAntler },
            { HuntDropItems.VoidFeather.Id, HuntDropItems.VoidFeather },

            { HuntDropItems.MythicHide.Id, HuntDropItems.MythicHide },
            { HuntDropItems.SkyTalon.Id, HuntDropItems.SkyTalon },
            { HuntDropItems.AbyssClaw.Id, HuntDropItems.AbyssClaw },
            { HuntDropItems.ColossusAntler.Id, HuntDropItems.ColossusAntler },
            { HuntDropItems.DeathFeather.Id, HuntDropItems.DeathFeather },

            // ===== Tier 1 Hunter Gear =====

            { CraftableHuntGear.ClawDagger.Id, CraftableHuntGear.ClawDagger },
            { CraftableHuntGear.BoneBuckler.Id, CraftableHuntGear.BoneBuckler },
            { CraftableHuntGear.LeatherVest.Id, CraftableHuntGear.LeatherVest },
            { CraftableHuntGear.BoneHelm.Id, CraftableHuntGear.BoneHelm },
            { CraftableHuntGear.HunterLeggings.Id, CraftableHuntGear.HunterLeggings },
            { CraftableHuntGear.HideBoots.Id, CraftableHuntGear.HideBoots },
            { CraftableHuntGear.FurGloves.Id, CraftableHuntGear.FurGloves },
            { CraftableHuntGear.FeatherBand.Id, CraftableHuntGear.FeatherBand },
            { CraftableHuntGear.RavenCharm.Id, CraftableHuntGear.RavenCharm },

            // ===== Tier 2 Raider Gear =====

            { CraftableHuntGear.FangBlade.Id, CraftableHuntGear.FangBlade },
            { CraftableHuntGear.HornShield.Id, CraftableHuntGear.HornShield },
            { CraftableHuntGear.HideWarcoat.Id, CraftableHuntGear.HideWarcoat },
            { CraftableHuntGear.RaiderHelm.Id, CraftableHuntGear.RaiderHelm },
            { CraftableHuntGear.RaiderLegguards.Id, CraftableHuntGear.RaiderLegguards },
            { CraftableHuntGear.RaiderBoots.Id, CraftableHuntGear.RaiderBoots },
            { CraftableHuntGear.TrackerGloves.Id, CraftableHuntGear.TrackerGloves },
            { CraftableHuntGear.RaiderBand.Id, CraftableHuntGear.RaiderBand },
            { CraftableHuntGear.TalonCharm.Id, CraftableHuntGear.TalonCharm },

            // ===== Tier 3 Warlord Gear =====

            { CraftableHuntGear.SaberFangBlade.Id, CraftableHuntGear.SaberFangBlade },
            { CraftableHuntGear.AntlerShield.Id, CraftableHuntGear.AntlerShield },
            { CraftableHuntGear.WarlordArmor.Id, CraftableHuntGear.WarlordArmor },
            { CraftableHuntGear.WarlordHelm.Id, CraftableHuntGear.WarlordHelm },
            { CraftableHuntGear.WarlordGreaves.Id, CraftableHuntGear.WarlordGreaves },
            { CraftableHuntGear.SabertoothBoots.Id, CraftableHuntGear.SabertoothBoots },
            { CraftableHuntGear.ClawGauntlets.Id, CraftableHuntGear.ClawGauntlets },
            { CraftableHuntGear.GriffinBand.Id, CraftableHuntGear.GriffinBand },
            { CraftableHuntGear.RavenEyePendant.Id, CraftableHuntGear.RavenEyePendant },

            // ===== Tier 4 Champion Gear =====

            { CraftableHuntGear.TitanBlade.Id, CraftableHuntGear.TitanBlade },
            { CraftableHuntGear.TitanShield.Id, CraftableHuntGear.TitanShield },
            { CraftableHuntGear.ChampionPlate.Id, CraftableHuntGear.ChampionPlate },
            { CraftableHuntGear.ChampionHelm.Id, CraftableHuntGear.ChampionHelm },
            { CraftableHuntGear.ChampionGreaves.Id, CraftableHuntGear.ChampionGreaves },
            { CraftableHuntGear.ShadowstepBoots.Id, CraftableHuntGear.ShadowstepBoots },
            { CraftableHuntGear.StormGauntlets.Id, CraftableHuntGear.StormGauntlets },
            { CraftableHuntGear.StormRing.Id, CraftableHuntGear.StormRing },
            { CraftableHuntGear.VoidPendant.Id, CraftableHuntGear.VoidPendant },

            // ===== Tier 5 Mythic Gear =====

            { CraftableHuntGear.WorldbreakerBlade.Id, CraftableHuntGear.WorldbreakerBlade },
            { CraftableHuntGear.ColossusShield.Id, CraftableHuntGear.ColossusShield },
            { CraftableHuntGear.BeastslayerPlate.Id, CraftableHuntGear.BeastslayerPlate },
            { CraftableHuntGear.MythicCrown.Id, CraftableHuntGear.MythicCrown },
            { CraftableHuntGear.ColossusLegguards.Id, CraftableHuntGear.ColossusLegguards },
            { CraftableHuntGear.SkystriderBoots.Id, CraftableHuntGear.SkystriderBoots },
            { CraftableHuntGear.AbyssGauntlets.Id, CraftableHuntGear.AbyssGauntlets },
            { CraftableHuntGear.RavenKingBand.Id, CraftableHuntGear.RavenKingBand },
            { CraftableHuntGear.PendantOfTheWild.Id, CraftableHuntGear.PendantOfTheWild },

             // ===== Hunt Rare Drops =====

            { HuntRareDrops.WolfTrophy.Id, HuntRareDrops.WolfTrophy },
            { HuntRareDrops.BoarTusk.Id, HuntRareDrops.BoarTusk },
            { HuntRareDrops.StagAntler.Id, HuntRareDrops.StagAntler },
            { HuntRareDrops.AncientFeather.Id, HuntRareDrops.AncientFeather },
            { HuntRareDrops.BearHeart.Id, HuntRareDrops.BearHeart },
            { HuntRareDrops.AlphaFang.Id, HuntRareDrops.AlphaFang },
            { HuntRareDrops.StormTalon.Id, HuntRareDrops.StormTalon },
            { HuntRareDrops.SaberRelic.Id, HuntRareDrops.SaberRelic },
            { HuntRareDrops.GriffinCore.Id, HuntRareDrops.GriffinCore },
            { HuntRareDrops.StormRelic.Id, HuntRareDrops.StormRelic },
            { HuntRareDrops.AncientCore.Id, HuntRareDrops.AncientCore },
            { HuntRareDrops.MythicHeart.Id, HuntRareDrops.MythicHeart },
            { HuntRareDrops.SkyRelic.Id, HuntRareDrops.SkyRelic },

            //Fanculo dungeon drops
            {DungeonBossDrops.FanculoHelmItem.Id, DungeonBossDrops.FanculoHelmItem },

            // ===== Boss Equipment Drops =====
            
            { DailyWeeklyBossDrops.AureliusSwordItem.Id, DailyWeeklyBossDrops.AureliusSwordItem },



            { DailyWeeklyBossDrops.XerathulArmorItem.Id, DailyWeeklyBossDrops.XerathulArmorItem },
            { DailyWeeklyBossDrops.GravelmawShieldItem.Id, DailyWeeklyBossDrops.GravelmawShieldItem },
            { DailyWeeklyBossDrops.SerpentAmuletItem.Id, DailyWeeklyBossDrops.SerpentAmuletItem },
        };
    }
}