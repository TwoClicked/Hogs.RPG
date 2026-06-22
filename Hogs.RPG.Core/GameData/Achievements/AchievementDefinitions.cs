using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.AchievementObjects;
using Hogs.RPG.Core.Entities.PlayerObjects;

namespace Hogs.RPG.Core.GameData.Achievements
{
    public static class AchievementDefinitions
    {
        public static readonly List<AchievementDefinition> All = new()
        {
            // =========================
            // ⚔️ DUNGEONS
            // =========================
            new() { Id = "first_dungeon",    Name = "First Steps",       Icon = "⚔️", Category = "Dungeons", Description = "Complete your first dungeon.",        Condition = ctx => ctx.Player.DungeonRunsCompleted >= 1    },
            new() { Id = "dungeons_10",      Name = "Dungeon Crawler",   Icon = "⚔️", Category = "Dungeons", Description = "Complete 10 dungeons.",              Condition = ctx => ctx.Player.DungeonRunsCompleted >= 10   },
            new() { Id = "dungeons_25",      Name = "Dungeon Veteran",   Icon = "⚔️", Category = "Dungeons", Description = "Complete 25 dungeons.",              Condition = ctx => ctx.Player.DungeonRunsCompleted >= 25   },
            new() { Id = "dungeons_50",      Name = "Dungeon Expert",    Icon = "⚔️", Category = "Dungeons", Description = "Complete 50 dungeons.",              Condition = ctx => ctx.Player.DungeonRunsCompleted >= 50   },
            new() { Id = "dungeons_100",     Name = "Dungeon Master",    Icon = "⚔️", Category = "Dungeons", Description = "Complete 100 dungeons.",             Condition = ctx => ctx.Player.DungeonRunsCompleted >= 100  },
            new() { Id = "dungeons_250",     Name = "Floor Clearer",     Icon = "⚔️", Category = "Dungeons", Description = "Complete 250 dungeons.",             Condition = ctx => ctx.Player.DungeonRunsCompleted >= 250  },
            new() { Id = "dungeons_500",     Name = "Dungeon Legend",    Icon = "⚔️", Category = "Dungeons", Description = "Complete 500 dungeons.",             Condition = ctx => ctx.Player.DungeonRunsCompleted >= 500  },
            new() { Id = "dungeons_750",     Name = "Dungeon Obsessed",  Icon = "⚔️", Category = "Dungeons", Description = "Complete 750 dungeons.",             Condition = ctx => ctx.Player.DungeonRunsCompleted >= 750  },
            new() { Id = "dungeons_1000",    Name = "Dungeon God",       Icon = "⚔️", Category = "Dungeons", Description = "Complete 1,000 dungeons.",           Condition = ctx => ctx.Player.DungeonRunsCompleted >= 1000 },
            new() { Id = "dungeons_1500",    Name = "Eternal Delver",    Icon = "⚔️", Category = "Dungeons", Description = "Complete 1,500 dungeons.",           Condition = ctx => ctx.Player.DungeonRunsCompleted >= 1500 },

            // =========================
            // ⚔️ RAIDS
            // =========================
            new() { Id = "first_raid",   Name = "First Raid",      Icon = "⚔️", Category = "Raids", Description = "Complete your first raid.",    Condition = ctx => ctx.Player.RaidsCompleted >= 1   },
            new() { Id = "raids_10",     Name = "Raid Rookie",     Icon = "⚔️", Category = "Raids", Description = "Complete 10 raids.",           Condition = ctx => ctx.Player.RaidsCompleted >= 10  },
            new() { Id = "raids_20",     Name = "Raid Veteran",    Icon = "⚔️", Category = "Raids", Description = "Complete 20 raids.",           Condition = ctx => ctx.Player.RaidsCompleted >= 20  },
            new() { Id = "raids_40",     Name = "Raid Champion",   Icon = "⚔️", Category = "Raids", Description = "Complete 40 raids.",           Condition = ctx => ctx.Player.RaidsCompleted >= 40  },
            new() { Id = "raids_80",     Name = "Raid Commander",  Icon = "⚔️", Category = "Raids", Description = "Complete 80 raids.",           Condition = ctx => ctx.Player.RaidsCompleted >= 80  },
            new() { Id = "raids_250",    Name = "Raid Legend",     Icon = "⚔️", Category = "Raids", Description = "Complete 250 raids.",          Condition = ctx => ctx.Player.RaidsCompleted >= 250 },

            // =========================
            // ⚔️ BOSS DAMAGE
            // =========================
            new() { Id = "boss_10k",    Name = "Boss Slayer",       Icon = "🔥", Category = "Boss",  Description = "Deal 10,000 total boss damage.",        Condition = ctx => ctx.Player.TotalBossDamage >= 10_000      },
            new() { Id = "boss_100k",   Name = "Boss Destroyer",    Icon = "🔥", Category = "Boss",  Description = "Deal 100,000 total boss damage.",       Condition = ctx => ctx.Player.TotalBossDamage >= 100_000     },
            new() { Id = "boss_1m",     Name = "Boss Obliterator",  Icon = "🔥", Category = "Boss",  Description = "Deal 1,000,000 total boss damage.",     Condition = ctx => ctx.Player.TotalBossDamage >= 1_000_000   },
            new() { Id = "boss_10m",    Name = "Tribal Weapon",     Icon = "🔥", Category = "Boss",  Description = "Deal 10,000,000 total boss damage.",    Condition = ctx => ctx.Player.TotalBossDamage >= 10_000_000  },
            new() { Id = "boss_25m",    Name = "The Unstoppable",   Icon = "🔥", Category = "Boss",  Description = "Deal 25,000,000 total boss damage.",    Condition = ctx => ctx.Player.TotalBossDamage >= 25_000_000  },

            // =========================
            // ⚔️ DEATHS
            // =========================
            new() { Id = "first_death", Name = "Brave or Stupid", Icon = "💀", Category = "Deaths", Description = "Die for the first time.", Condition = ctx => ctx.Player.Deaths >= 1 },

            // =========================
            // 🏹 HUNTING
            // =========================
            new() { Id = "first_hunt",    Name = "First Hunt",           Icon = "🏹", Category = "Hunting", Description = "Complete your first hunt.",       Condition = ctx => ctx.Player.TotalHuntsCompleted >= 1,      IsRetroactiveEligible = false },
            new() { Id = "hunts_50",      Name = "Apprentice Hunter",    Icon = "🏹", Category = "Hunting", Description = "Complete 50 hunts.",              Condition = ctx => ctx.Player.TotalHuntsCompleted >= 50,     IsRetroactiveEligible = false },
            new() { Id = "hunts_250",     Name = "Seasoned Hunter",      Icon = "🏹", Category = "Hunting", Description = "Complete 250 hunts.",             Condition = ctx => ctx.Player.TotalHuntsCompleted >= 250,    IsRetroactiveEligible = false },
            new() { Id = "hunts_500",     Name = "Expert Hunter",        Icon = "🏹", Category = "Hunting", Description = "Complete 500 hunts.",             Condition = ctx => ctx.Player.TotalHuntsCompleted >= 500,    IsRetroactiveEligible = false },
            new() { Id = "hunts_1000",    Name = "Master Hunter",        Icon = "🏹", Category = "Hunting", Description = "Complete 1,000 hunts.",           Condition = ctx => ctx.Player.TotalHuntsCompleted >= 1000,   IsRetroactiveEligible = false },
            new() { Id = "hunts_2500",    Name = "Legendary Hunter",     Icon = "🏹", Category = "Hunting", Description = "Complete 2,500 hunts.",           Condition = ctx => ctx.Player.TotalHuntsCompleted >= 2500,   IsRetroactiveEligible = false },
            new() { Id = "hunts_5000",    Name = "Mythic Hunter",        Icon = "🏹", Category = "Hunting", Description = "Complete 5,000 hunts.",           Condition = ctx => ctx.Player.TotalHuntsCompleted >= 5000,   IsRetroactiveEligible = false },
            new() { Id = "hunts_10000",   Name = "Elite Hunter",         Icon = "🏹", Category = "Hunting", Description = "Complete 10,000 hunts.",          Condition = ctx => ctx.Player.TotalHuntsCompleted >= 10000,  IsRetroactiveEligible = false },
            new() { Id = "hunts_20000",   Name = "Eternal Hunter",       Icon = "🏹", Category = "Hunting", Description = "Complete 20,000 hunts.",          Condition = ctx => ctx.Player.TotalHuntsCompleted >= 20000,  IsRetroactiveEligible = false },
            new() { Id = "first_rare",    Name = "First Rare Drop",      Icon = "✨", Category = "Hunting", Description = "Get your first rare drop.",       Condition = ctx => ctx.Player.TotalRareDrops >= 1,           IsRetroactiveEligible = false },
            new() { Id = "rare_25",       Name = "Lucky Hand",           Icon = "✨", Category = "Hunting", Description = "Get 25 rare drops.",              Condition = ctx => ctx.Player.TotalRareDrops >= 25,          IsRetroactiveEligible = false },
            new() { Id = "rare_100",      Name = "Fortune's Favourite",  Icon = "✨", Category = "Hunting", Description = "Get 100 rare drops.",             Condition = ctx => ctx.Player.TotalRareDrops >= 100,         IsRetroactiveEligible = false },
            new() { Id = "stamina_10k",   Name = "Stamina Beast",        Icon = "🏹", Category = "Hunting", Description = "Spend 10,000 total stamina.",     Condition = ctx => ctx.Player.TotalStaminaSpent >= 10000,    IsRetroactiveEligible = false },

            // =========================
            // ⛏️ GATHERING
            // =========================
            new() { Id = "forest_5k",     Name = "Forest Apprentice", Icon = "🌿", Category = "Gathering", Description = "Spend 5,000 energy in the forest.",   Condition = ctx => ctx.Player.ForestEnergySpent >= 5000,   IsRetroactiveEligible = false },
            new() { Id = "forest_10k",    Name = "Forest Veteran",    Icon = "🌿", Category = "Gathering", Description = "Spend 10,000 energy in the forest.",  Condition = ctx => ctx.Player.ForestEnergySpent >= 10000,  IsRetroactiveEligible = false },
            new() { Id = "forest_25k",    Name = "Forest Master",     Icon = "🌿", Category = "Gathering", Description = "Spend 25,000 energy in the forest.",  Condition = ctx => ctx.Player.ForestEnergySpent >= 25000,  IsRetroactiveEligible = false },
            new() { Id = "mine_5k",       Name = "Apprentice Miner",  Icon = "⛏️", Category = "Gathering", Description = "Spend 5,000 energy in the mine.",     Condition = ctx => ctx.Player.MineEnergySpent >= 5000,     IsRetroactiveEligible = false },
            new() { Id = "mine_10k",      Name = "Veteran Miner",     Icon = "⛏️", Category = "Gathering", Description = "Spend 10,000 energy in the mine.",    Condition = ctx => ctx.Player.MineEnergySpent >= 10000,    IsRetroactiveEligible = false },
            new() { Id = "mine_25k",      Name = "Master Miner",      Icon = "⛏️", Category = "Gathering", Description = "Spend 25,000 energy in the mine.",    Condition = ctx => ctx.Player.MineEnergySpent >= 25000,    IsRetroactiveEligible = false },
            new() { Id = "swamp_5k",      Name = "Swamp Walker",      Icon = "🌱", Category = "Gathering", Description = "Spend 5,000 energy in the swamp.",    Condition = ctx => ctx.Player.SwampEnergySpent >= 5000,    IsRetroactiveEligible = false },
            new() { Id = "swamp_10k",     Name = "Swamp Veteran",     Icon = "🌱", Category = "Gathering", Description = "Spend 10,000 energy in the swamp.",   Condition = ctx => ctx.Player.SwampEnergySpent >= 10000,   IsRetroactiveEligible = false },
            new() { Id = "swamp_25k",     Name = "Swamp Master",      Icon = "🌱", Category = "Gathering", Description = "Spend 25,000 energy in the swamp.",   Condition = ctx => ctx.Player.SwampEnergySpent >= 25000,   IsRetroactiveEligible = false },
            new() { Id = "crystal_found", Name = "Crystal Hunter",    Icon = "🔮", Category = "Gathering", Description = "Find your first Dragon Crystal.",     Condition = ctx => ctx.Player.DragonCrystalFound,          IsRetroactiveEligible = false },

            // =========================
            // ⚒️ BLACKSMITH
            // =========================
            new() { Id = "smithing_5",    Name = "First Spark",        Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 5.",               Condition = ctx => ctx.Player.SmithingLevel >= 5  },
            new() { Id = "smithing_10",   Name = "Apprentice Smith",   Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 10.",              Condition = ctx => ctx.Player.SmithingLevel >= 10 },
            new() { Id = "smithing_30",   Name = "Journeyman Smith",   Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 30.",              Condition = ctx => ctx.Player.SmithingLevel >= 30 },
            new() { Id = "smithing_50",   Name = "Expert Smith",       Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 50.",              Condition = ctx => ctx.Player.SmithingLevel >= 50 },
            new() { Id = "smithing_70",   Name = "Master Smith",       Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 70.",              Condition = ctx => ctx.Player.SmithingLevel >= 70 },
            new() { Id = "smithing_85",   Name = "Grandmaster Smith",  Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 85.",              Condition = ctx => ctx.Player.SmithingLevel >= 85 },
            new() { Id = "smithing_99",   Name = "Legendary Smith",    Icon = "⚒️", Category = "Blacksmith", Description = "Reach Smithing level 99.",              Condition = ctx => ctx.Player.SmithingLevel >= 99 },
            new() { Id = "first_forge",   Name = "First Forge",        Icon = "⚒️", Category = "Blacksmith", Description = "Forge your first item.",                Condition = ctx => ctx.Player.TotalItemsForged >= 1,           IsRetroactiveEligible = false },
            new() { Id = "forged_100",    Name = "Weapon Maker",       Icon = "⚒️", Category = "Blacksmith", Description = "Forge 100 items.",                      Condition = ctx => ctx.Player.TotalItemsForged >= 100,         IsRetroactiveEligible = false },
            new() { Id = "forged_500",    Name = "Arms Dealer",        Icon = "⚒️", Category = "Blacksmith", Description = "Forge 500 items.",                      Condition = ctx => ctx.Player.TotalItemsForged >= 500,         IsRetroactiveEligible = false },
            new() { Id = "dragon_forger", Name = "Dragon Forger",      Icon = "🐉", Category = "Blacksmith", Description = "Forge a Dragon Blade.",                 Condition = ctx => ctx.Player.DragonBladeForged,               IsRetroactiveEligible = false },
            new() { Id = "npc_50k",       Name = "NPC Favourite",      Icon = "🛒", Category = "Blacksmith", Description = "Earn 50,000g total from the NPC shop.", Condition = ctx => ctx.Player.TotalNpcGoldEarned >= 50000,     IsRetroactiveEligible = false },

            // =========================
            // 🧪 ALCHEMIST
            // =========================
            new() { Id = "alchemy_5",     Name = "First Drop",             Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 5.",        Condition = ctx => ctx.Player.AlchemistLevel >= 5  },
            new() { Id = "alchemy_10",    Name = "Apprentice Alchemist",   Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 10.",       Condition = ctx => ctx.Player.AlchemistLevel >= 10 },
            new() { Id = "alchemy_30",    Name = "Journeyman Alchemist",   Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 30.",       Condition = ctx => ctx.Player.AlchemistLevel >= 30 },
            new() { Id = "alchemy_50",    Name = "Expert Alchemist",       Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 50.",       Condition = ctx => ctx.Player.AlchemistLevel >= 50 },
            new() { Id = "alchemy_70",    Name = "Master Alchemist",       Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 70.",       Condition = ctx => ctx.Player.AlchemistLevel >= 70 },
            new() { Id = "alchemy_85",    Name = "Grandmaster Alchemist",  Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 85.",       Condition = ctx => ctx.Player.AlchemistLevel >= 85 },
            new() { Id = "alchemy_99",    Name = "Legendary Alchemist",    Icon = "🧪", Category = "Alchemist", Description = "Reach Alchemist level 99.",       Condition = ctx => ctx.Player.AlchemistLevel >= 99 },
            new() { Id = "first_brew",    Name = "First Brew",             Icon = "🧪", Category = "Alchemist", Description = "Brew your first potion.",         Condition = ctx => ctx.Player.TotalPotionsBrewed >= 1,    IsRetroactiveEligible = false },
            new() { Id = "brewed_50",     Name = "Potion Maker",           Icon = "🧪", Category = "Alchemist", Description = "Brew 50 potions.",                Condition = ctx => ctx.Player.TotalPotionsBrewed >= 50,   IsRetroactiveEligible = false },
            new() { Id = "brewed_250",    Name = "Potion Hoarder",         Icon = "🧪", Category = "Alchemist", Description = "Brew 250 potions.",               Condition = ctx => ctx.Player.TotalPotionsBrewed >= 250,  IsRetroactiveEligible = false },
            new() { Id = "brewed_500",    Name = "Potions Master",         Icon = "🧪", Category = "Alchemist", Description = "Brew 500 potions.",               Condition = ctx => ctx.Player.TotalPotionsBrewed >= 500,  IsRetroactiveEligible = false },
            new() { Id = "elixir_used",   Name = "Elixir Supplier",        Icon = "⚒️", Category = "Alchemist", Description = "Use a Blacksmith's Elixir.",      Condition = ctx => ctx.Player.BlacksmithElixirUsed,       IsRetroactiveEligible = false },

            // =========================
            // 🏕️ TRAILS
            // =========================
            new() { Id = "first_trail",  Name = "First Trail",     Icon = "🏕️", Category = "Trails", Description = "Complete your first trail run.",   Condition = ctx => ctx.Player.TrailsCompleted >= 1   },
            new() { Id = "trails_10",    Name = "Trail Walker",    Icon = "🏕️", Category = "Trails", Description = "Complete 10 trail runs.",           Condition = ctx => ctx.Player.TrailsCompleted >= 10  },
            new() { Id = "trails_25",    Name = "Trail Runner",    Icon = "🏕️", Category = "Trails", Description = "Complete 25 trail runs.",           Condition = ctx => ctx.Player.TrailsCompleted >= 25  },
            new() { Id = "trails_50",    Name = "Trail Veteran",   Icon = "🏕️", Category = "Trails", Description = "Complete 50 trail runs.",           Condition = ctx => ctx.Player.TrailsCompleted >= 50  },
            new() { Id = "trails_75",    Name = "Trail Expert",    Icon = "🏕️", Category = "Trails", Description = "Complete 75 trail runs.",           Condition = ctx => ctx.Player.TrailsCompleted >= 75  },
            new() { Id = "trails_100",   Name = "Trail Legend",    Icon = "🏕️", Category = "Trails", Description = "Complete 100 trail runs.",          Condition = ctx => ctx.Player.TrailsCompleted >= 100 },
            new() { Id = "tokens_500",   Name = "Token Collector", Icon = "🪙",  Category = "Trails", Description = "Earn 500 total Tracker Tokens.",    Condition = ctx => ctx.Player.TotalTrackerTokensEarned >= 500, IsRetroactiveEligible = false },

            // =========================
            // 🐾 PETS
            // =========================
            new() { Id = "first_pet",       Name = "Bonded",             Icon = "🐾", Category = "Pets", Description = "Equip your first pet.",              Condition = ctx => ctx.Player.HighestPetLevel >= 1  },
            new() { Id = "pet_level_10",    Name = "Pet Trainer",        Icon = "🐾", Category = "Pets", Description = "Reach pet level 10.",                Condition = ctx => ctx.Player.HighestPetLevel >= 10 },
            new() { Id = "pet_level_20",    Name = "Pet Expert",         Icon = "🐾", Category = "Pets", Description = "Reach pet level 20.",                Condition = ctx => ctx.Player.HighestPetLevel >= 20 },
            new() { Id = "pet_level_25",    Name = "Pet Master",         Icon = "🐾", Category = "Pets", Description = "Reach pet level 25.",                Condition = ctx => ctx.Player.HighestPetLevel >= 25 },
            new() { Id = "pets_3",          Name = "Collector",          Icon = "🐾", Category = "Pets", Description = "Own 3 different pets.",              Condition = ctx => ctx.Player.TotalPetsOwned >= 3   },
            new() { Id = "capytara",        Name = "Evolution",          Icon = "🐉", Category = "Pets", Description = "Evolve into CapyTara.",              Condition = ctx => ctx.Player.CapyTaraEvolved,       IsRetroactiveEligible = false },
            new() { Id = "hunting_comp",    Name = "Hunting Companion",  Icon = "🐾", Category = "Pets", Description = "Unlock the Hunting Companion.",      Condition = ctx => ctx.Player.HuntingCompanionUnlocked },
            new() { Id = "alchemist_comp",  Name = "Workshop Assistant", Icon = "🧪", Category = "Pets", Description = "Unlock the Alchemist Companion.",    Condition = ctx => ctx.Player.AlchemistCompanionUnlocked },
            new() { Id = "gather_comp",     Name = "Wings of Odin",      Icon = "🌿", Category = "Pets", Description = "Unlock the Gather Companion.",       Condition = ctx => ctx.Player.GatherCompanionUnlocked   },
            new() { Id = "blacksmith_comp", Name = "Forge Friend",       Icon = "🔨", Category = "Pets", Description = "Unlock the Blacksmith Companion.",   Condition = ctx => ctx.Player.BlacksmithCompanionUnlocked },

            // =========================
            // 📈 PROGRESSION
            // =========================
            new() { Id = "level_10",    Name = "Rising Hero",     Icon = "📈", Category = "Progression", Description = "Reach player level 10.",       Condition = ctx => ctx.Player.Level >= 10 },
            new() { Id = "level_20",    Name = "Warrior",         Icon = "📈", Category = "Progression", Description = "Reach player level 20.",       Condition = ctx => ctx.Player.Level >= 20 },
            new() { Id = "level_30",    Name = "Elite Warrior",   Icon = "📈", Category = "Progression", Description = "Reach player level 30.",       Condition = ctx => ctx.Player.Level >= 30 },
            new() { Id = "level_40",    Name = "Champion",        Icon = "📈", Category = "Progression", Description = "Reach player level 40.",       Condition = ctx => ctx.Player.Level >= 40 },
            new() { Id = "level_50",    Name = "Tribal Legend",   Icon = "📈", Category = "Progression", Description = "Reach player level 40.",       Condition = ctx => ctx.Player.Level >= 40 },
            new() { Id = "gear_500",    Name = "Geared Up",       Icon = "⚔️", Category = "Progression", Description = "Reach a gear score of 500.",   Condition = ctx => ctx.GearScore >= 500   },
            new() { Id = "gear_1000",   Name = "Well Equipped",   Icon = "⚔️", Category = "Progression", Description = "Reach a gear score of 1,000.", Condition = ctx => ctx.GearScore >= 1000  },
            new() { Id = "gear_2000",   Name = "Battle Ready",    Icon = "⚔️", Category = "Progression", Description = "Reach a gear score of 2,000.", Condition = ctx => ctx.GearScore >= 2000  },
            new() { Id = "gear_3000",   Name = "Fully Forged",    Icon = "⚔️", Category = "Progression", Description = "Reach a gear score of 3,000.", Condition = ctx => ctx.GearScore >= 3000  },

            // =========================
            // 💰 WEALTH
            // =========================
            new() { Id = "gold_10k",    Name = "First Gold",        Icon = "💰", Category = "Wealth", Description = "Accumulate 10,000 gold.",      Condition = ctx => ctx.Player.Gold >= 10_000    },
            new() { Id = "gold_50k",    Name = "Getting Rich",      Icon = "💰", Category = "Wealth", Description = "Accumulate 50,000 gold.",      Condition = ctx => ctx.Player.Gold >= 50_000    },
            new() { Id = "gold_100k",   Name = "Tribe Millionaire", Icon = "💰", Category = "Wealth", Description = "Accumulate 100,000 gold.",     Condition = ctx => ctx.Player.Gold >= 100_000   },
            new() { Id = "gold_1m",     Name = "Gold Hoarder",      Icon = "💰", Category = "Wealth", Description = "Spend 1,000,000 gold.",        Condition = ctx => ctx.Player.TotalGoldSpent >= 1_000_000 },

            // =========================
            // 🌟 LEGEND
            // =========================
            new() { Id = "dual_crafter",   Name = "Dual Crafter",          Icon = "🌟", Category = "Legend", Description = "Reach level 50 in both Smithing and Alchemy.",        Condition = ctx => ctx.Player.SmithingLevel >= 50 && ctx.Player.AlchemistLevel >= 50                                              },
            new() { Id = "dual_master",    Name = "Dual Master",           Icon = "🌟", Category = "Legend", Description = "Reach level 99 in both Smithing and Alchemy.",        Condition = ctx => ctx.Player.SmithingLevel >= 99 && ctx.Player.AlchemistLevel >= 99                                              },
            new() { Id = "complete_pkg",   Name = "The Complete Package",  Icon = "🌟", Category = "Legend", Description = "Own CapyTara and 2 T5 relics.",                       Condition = ctx => ctx.Player.CapyTaraEvolved && ctx.Slot1RelicRank >= 5 && ctx.Slot2RelicRank >= 5, IsRetroactiveEligible = false },
            new() { Id = "the_legend",     Name = "The Legend",            Icon = "🌟", Category = "Legend", Description = "Reach player level 40, 500 dungeons and 100 raids.",  Condition = ctx => ctx.Player.Level >= 40 && ctx.Player.DungeonRunsCompleted >= 500 && ctx.Player.RaidsCompleted >= 100          },

            // =========================
            // 🗼 TOWER OF DOOM
            // =========================
            new() { Id = "tower_solo_10",  Name = "First Ascent",          Icon = "🗼", Category = "Tower", Description = "Reach floor 10 in a solo Tower run.",   Condition = ctx => ctx.Player.BestSoloTowerFloor >= 10,  IsRetroactiveEligible = false },
            new() { Id = "tower_solo_25",  Name = "Solo Climber",          Icon = "🗼", Category = "Tower", Description = "Reach floor 25 in a solo Tower run.",   Condition = ctx => ctx.Player.BestSoloTowerFloor >= 25,  IsRetroactiveEligible = false },
            new() { Id = "tower_solo_50",  Name = "Lone Ascendant",        Icon = "🗼", Category = "Tower", Description = "Reach floor 50 in a solo Tower run.",   Condition = ctx => ctx.Player.BestSoloTowerFloor >= 50,  IsRetroactiveEligible = false },
            new() { Id = "tower_solo_100", Name = "The Lone Summit",       Icon = "🗼", Category = "Tower", Description = "Reach floor 100 in a solo Tower run.",  Condition = ctx => ctx.Player.BestSoloTowerFloor >= 100, IsRetroactiveEligible = false },
            new() { Id = "tower_duo_10",   Name = "Partners in Doom",      Icon = "🗼", Category = "Tower", Description = "Reach floor 10 in a duo Tower run.",    Condition = ctx => ctx.Player.BestDuoTowerFloor >= 10,   IsRetroactiveEligible = false },
            new() { Id = "tower_duo_25",   Name = "Dynamic Duo",           Icon = "🗼", Category = "Tower", Description = "Reach floor 25 in a duo Tower run.",    Condition = ctx => ctx.Player.BestDuoTowerFloor >= 25,   IsRetroactiveEligible = false },
            new() { Id = "tower_duo_50",   Name = "Unbreakable Bond",      Icon = "🗼", Category = "Tower", Description = "Reach floor 50 in a duo Tower run.",    Condition = ctx => ctx.Player.BestDuoTowerFloor >= 50,   IsRetroactiveEligible = false },
            new() { Id = "tower_duo_100",  Name = "Two at the Summit",     Icon = "🗼", Category = "Tower", Description = "Reach floor 100 in a duo Tower run.",   Condition = ctx => ctx.Player.BestDuoTowerFloor >= 100,  IsRetroactiveEligible = false },
        };
    }
}