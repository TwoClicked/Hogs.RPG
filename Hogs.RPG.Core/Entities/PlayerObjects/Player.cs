using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Hogs.RPG.Core.Enums;

namespace Hogs.RPG.Core.Entities.PlayerObjects
{
    public class Player
    {
        public int PlayerId { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int Gold { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        // Hunt
        public int HunterStamina { get; set; }
        public string? LastHunterStaminaUpdate { get; set; }

        // Gear
        public string? MainHand { get; set; }
        public string? OffHand { get; set; }
        public string? Helmet { get; set; }
        public string? Body { get; set; }
        public string? Legs { get; set; }
        public string? Gloves { get; set; }
        public string? Boots { get; set; }
        public string? Ring { get; set; }
        public string? Amulet { get; set; }

        // 🔥 STORED IN DB
        public string ActiveBuffsData { get; set; } = "";

        // 🔥 NOT MAPPED (runtime only)
        [NotMapped]
        public List<ActiveBuff> ActiveBuffs { get; set; } = new();

        public bool AutoUseXpPotions { get; set; }

        // Gathering
        public int Energy { get; set; }
        public string? LastEnergyUpdate { get; set; }

        // Bosses
        public string? LastBossAttack { get; set; }

        // Raids
        public string? LastRaidAt { get; set; }
        public int RaidsToday { get; set; } = 0;
        public string? LastRaidDayReset { get; set; }

        // Leaderboard
        public int DungeonRunsCompleted { get; set; }
        public int RaidsCompleted { get; set; }
        public long TotalBossDamage { get; set; }
        public int Deaths { get; set; }
        public int TrailsCompleted { get; set; } = 0;

        // Shop perks
        public DateTime? XpBoostExpiry { get; set; }
        public DateTime? StaminaBoostExpiry { get; set; }

        // =========================
        // TRAIL SYSTEM
        // =========================

        // Tracker Tokens — currency for the Tracker's Camp shop
        public int TrackerTokens { get; set; } = 0;

        // Trails run today — enforces the daily 3-trail cap
        public int TrailsToday { get; set; } = 0;

        // Date string of last trail — used to reset TrailsToday at midnight UTC
        public string? LastTrailDate { get; set; }

        // Whether the player owns the hunting companion pet
        public bool HasHuntingPet { get; set; } = false;

        // Date string of the last trail reset purchase — enforces once-per-day limit
        public string? TrailResetUsedDate { get; set; }

        // =========================
        // HUNTER SET BONUS
        // =========================

        // Granted permanently via /hunter-setcomplete.
        // Consumes all 9 Hunter gear pieces and applies an always-on
        // +4.5% XP, +4.5% materials, +5% rare drop bonus to /hunt.
        // Replaces the need to equip hunter gear when hunting.
        public bool HasHunterSetBonus { get; set; } = false;

        // =========================
        // BLACKSMITH
        // =========================

        // Smithing level — starts at 1, max 99
        public int SmithingLevel { get; set; } = 1;

        // Smithing XP — accumulates toward next level
        public int SmithingXP { get; set; } = 0;

        // Gold earned from NPC shop today — resets at 12 UTC
        public int SmithingEarnedToday { get; set; } = 0;

        // Date of last NPC shop reset — used to reset SmithingEarnedToday
        public string? SmithingLastReset { get; set; }

        // =========================
        // ALCHEMIST
        // =========================

        public int AlchemistLevel { get; set; } = 1;
        public int AlchemistXP { get; set; } = 0;

        // Active buff slots — one stat, one utility running simultaneously
        public string? ActiveStatBuffId { get; set; }
        public DateTime? ActiveStatBuffExpiry { get; set; }
        public string? ActiveUtilityBuffId { get; set; }
        public DateTime? ActiveUtilityBuffExpiry { get; set; }

        // Daily potion use cap — 5 per day
        public int PotionsDrankToday { get; set; } = 0;
        public string? LastPotionDrankDate { get; set; }

        // Blacksmith's Elixir — date player drank it (NpcShopService checks yesterday's date)
        public string? BlacksmithElixirActiveDate { get; set; }

        // =========================
        // ACHIEVEMENTS
        // =========================

        // Total achievements earned — drives milestone bonuses
        public int AchievementCount { get; set; } = 0;

        // Highest earned title — shown on profile
        public string? Title { get; set; }

        // =========================
        // ACHIEVEMENT COUNTERS
        // =========================

        // Hunting
        public int TotalHuntsCompleted { get; set; } = 0;
        public int TotalRareDrops { get; set; } = 0;
        public int TotalStaminaSpent { get; set; } = 0;

        // Gathering
        public int ForestEnergySpent { get; set; } = 0;
        public int MineEnergySpent { get; set; } = 0;
        public int SwampEnergySpent { get; set; } = 0;

        // Blacksmith
        public int TotalItemsForged { get; set; } = 0;
        public bool DragonCrystalFound { get; set; } = false;
        public bool DragonBladeForged { get; set; } = false;
        public int TotalNpcGoldEarned { get; set; } = 0;

        // Alchemist
        public int TotalPotionsBrewed { get; set; } = 0;
        public bool BlacksmithElixirUsed { get; set; } = false;

        // Pets
        public int HighestPetLevel { get; set; } = 0;
        public int TotalPetsOwned { get; set; } = 0;
        public bool CapyTaraEvolved { get; set; } = false;
        public bool HuntingCompanionUnlocked { get; set; } = false;

        // Trails
        public int TotalTrackerTokensEarned { get; set; } = 0;

        // =========================
        // 🔄 BUFF SERIALIZATION
        // =========================
        public void DeserializeBuffs()
        {
            ActiveBuffs.Clear();

            if (string.IsNullOrWhiteSpace(ActiveBuffsData))
                return;

            var buffs = ActiveBuffsData.Split(';');

            foreach (var buff in buffs)
            {
                var parts = buff.Split('|');

                if (parts.Length == 3)
                {
                    ActiveBuffs.Add(new ActiveBuff
                    {
                        Type = Enum.Parse<BuffType>(parts[0]),
                        Value = double.Parse(parts[1]),
                        RemainingUses = int.Parse(parts[2])
                    });
                }
            }
        }

        public void SerializeBuffs()
        {
            ActiveBuffsData = string.Join(";", ActiveBuffs.Select(b =>
                $"{b.Type}|{b.Value}|{b.RemainingUses}"
            ));
        }
    }
}
