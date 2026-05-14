using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Data.Repositories;

namespace Hogs.RPG.Services.RelicServices
{
    public class RelicService
    {
        private readonly RelicRepository _repo;
        private readonly Random _random = new();

        public RelicService(RelicRepository repo)
        {
            _repo = repo;
        }

        // =========================
        // 💎 GET ALL RELICS
        // =========================
        public async Task<List<PlayerRelic>> GetRelicsAsync(ulong discordId)
        {
            return await _repo.GetRelicsAsync(discordId);
        }

        // =========================
        // 💎 GET EQUIPPED RELICS
        // =========================
        public async Task<List<PlayerRelic>> GetEquippedRelicsAsync(ulong discordId)
        {
            return await _repo.GetEquippedRelicsAsync(discordId);
        }



        // =========================
        // 💎 EQUIP RELIC
        // =========================
        public async Task<string> EquipRelicAsync(ulong discordId, int relicId, int slotIndex)
        {
            if (slotIndex != 0 && slotIndex != 1)
                return "❌ Invalid slot. Choose slot 1 or slot 2.";

            var relic = await _repo.GetByIdAsync(relicId);

            if (relic == null || relic.DiscordId != discordId)
                return "❌ Relic not found.";

            var allRelics = await _repo.GetRelicsAsync(discordId);

            // Unequip anything currently in that slot
            foreach (var r in allRelics.Where(r => r.IsEquipped && r.SlotIndex == slotIndex))
            {
                r.IsEquipped = false;
            }

            relic.IsEquipped = true;
            relic.SlotIndex = slotIndex;

            await _repo.SaveAsync();

            var def = RelicRegistry.Get(relic.RelicId);
            return $"✅ **{def.Name}** equipped in slot {slotIndex + 1}.";
        }

        // =========================
        // 💎 UPGRADE RELIC
        // =========================
        public async Task<string> UpgradeRelicAsync(ulong discordId, int relicId, int shardTier)
        {
            var relic = await _repo.GetByIdAsync(relicId);

            if (relic == null || relic.DiscordId != discordId)
                return "❌ Relic not found.";

            if (relic.Rank >= 5)
                return "❌ This relic is already at maximum rank.";

            if (relic.Rank != shardTier - 1)
                return $"❌ You need a Tier {relic.Rank + 1} shard to upgrade this relic to Rank {relic.Rank + 1}.";

            var removed = await _repo.RemoveShardAsync(discordId, shardTier, 1);

            if (!removed)
                return $"❌ You don't have a Tier {shardTier} shard.";

            relic.Rank++;
            await _repo.SaveAsync();

            var def = RelicRegistry.Get(relic.RelicId);
            return $"✅ **{def.Name}** upgraded to Rank {relic.Rank}!";
        }

        // =========================
        // 💎 REROLL RELIC
        // =========================
        public async Task<string> RerollRelicAsync(ulong discordId, int relicId, int shardTier)
        {
            var relic = await _repo.GetByIdAsync(relicId);

            if (relic == null || relic.DiscordId != discordId)
                return "❌ Relic not found.";

            var removed = await _repo.RemoveShardAsync(discordId, shardTier, 1);

            if (!removed)
                return $"❌ You don't have a Tier {shardTier} shard to reroll with.";

            var def = RelicRegistry.Get(relic.RelicId);

            // Get all bonus types for this affinity
            var pool = GetBonusPool(def.Affinity);

            // Roll a new bonus (can't be the same as current)
            var available = pool.Where(b => b != relic.BonusType).ToList();

            if (available.Count == 0)
                return "❌ No other bonuses available to roll.";

            relic.BonusType = available[_random.Next(available.Count)];
            await _repo.SaveAsync();

            return $"🎲 Rerolled! **{def.Name}** now has: **{FormatBonus(relic.BonusType)}**";
        }

        // =========================
        // 💎 GIVE RELIC (drop reward)
        // =========================
        public async Task<PlayerRelic> GiveRelicAsync(ulong discordId, int tier)
        {
            // Pick a random affinity and bonus from that pool
            var affinities = Enum.GetValues<RelicAffinity>();
            var affinity = affinities[_random.Next(affinities.Length)];

            var pool = GetBonusPool(affinity);
            var bonusType = pool[_random.Next(pool.Count)];

            // Find a matching definition
            var matchingDef = RelicRegistry.All.Values
                .FirstOrDefault(d => d.Affinity == affinity && d.BonusType == bonusType);

            if (matchingDef == null)
                matchingDef = RelicRegistry.All.Values.First();

            var relic = new PlayerRelic
            {
                DiscordId = discordId,
                RelicId = matchingDef.Id,
                Rank = 1,
                BonusType = bonusType,
                IsEquipped = false,
                SlotIndex = 0
            };

            await _repo.AddRelicAsync(relic);
            return relic;
        }

        // =========================
        // 💎 GIVE SHARD (drop reward)
        // =========================
        public async Task GiveShardAsync(ulong discordId, int tier)
        {
            await _repo.AddShardAsync(discordId, tier, 1);
        }

        public async Task<bool> ConsumeShardAsync(ulong discordId, int tier)
        {
            return await _repo.RemoveShardAsync(discordId, tier, 1);
        }

        // =========================
        // 📊 CALCULATE RELIC BONUSES
        // Called by StatService and RaidService
        // =========================
        public async Task<RelicBonuses> GetRelicBonusesAsync(ulong discordId)
        {
            var equipped = await _repo.GetEquippedRelicsAsync(discordId);
            var bonuses = new RelicBonuses();

            foreach (var relic in equipped)
            {
                var def = RelicRegistry.Get(relic.RelicId);
                float value = def.BonusPerRank[relic.Rank - 1];

                switch (relic.BonusType)
                {
                    case RelicBonusType.AttackPercent:
                        bonuses.AttackPercent += value; break;
                    case RelicBonusType.DefensePercent:
                        bonuses.DefensePercent += value; break;
                    case RelicBonusType.MaxHpPercent:
                        bonuses.MaxHpPercent += value; break;
                    case RelicBonusType.BonusGold:
                        bonuses.BonusGoldPercent += value; break;
                    case RelicBonusType.BonusPlayerXp:
                        bonuses.BonusPlayerXpPercent += value; break;
                    case RelicBonusType.BonusPetXp:
                        bonuses.BonusPetXpPercent += value; break;
                    case RelicBonusType.IncreasedHealPercent:
                        bonuses.IncreasedHealPercent += value; break;
                    case RelicBonusType.ChanceToSavePotion:
                        bonuses.ChanceToSavePotion += value; break;
                    case RelicBonusType.LifeSteal:
                        bonuses.LifeStealPercent += value; break;
                    case RelicBonusType.ExecutionerBonus:
                        bonuses.ExecutionerBonusPercent += value; break;
                    case RelicBonusType.ConsecutiveHitBonus:
                        bonuses.ConsecutiveHitBonusPercent += value; break;
                    case RelicBonusType.ShattersLastLonger:
                        bonuses.ShatterExtraRounds += (int)value; break;
                    case RelicBonusType.TauntCooldownReduction:
                        bonuses.TauntCooldownReduction += (int)value; break;
                    case RelicBonusType.EmpowerLastsLonger:
                        bonuses.EmpowerExtraRounds += (int)value; break;
                    case RelicBonusType.WiderEmpower:
                        bonuses.WiderEmpowerThreshold += (int)value; break;
                    case RelicBonusType.BonusLootRoll:
                        bonuses.BonusLootRollPercent += value; break;
                }
            }

            return bonuses;
        }

        // =========================
        // 🎲 BONUS POOL PER AFFINITY
        // =========================
        public List<RelicBonusType> GetBonusPool(RelicAffinity affinity)
        {
            return affinity switch
            {
                RelicAffinity.Tank => new List<RelicBonusType>
                {
                    RelicBonusType.DefensePercent,
                    RelicBonusType.MaxHpPercent,
                    RelicBonusType.ShattersLastLonger,
                    RelicBonusType.TauntCooldownReduction
                },
                RelicAffinity.Dps => new List<RelicBonusType>
                {
                    RelicBonusType.AttackPercent,
                    RelicBonusType.ExecutionerBonus,
                    RelicBonusType.LifeSteal,
                    RelicBonusType.ConsecutiveHitBonus
                },
                RelicAffinity.Healer => new List<RelicBonusType>
                {
                    RelicBonusType.IncreasedHealPercent,
                    RelicBonusType.ChanceToSavePotion,
                    RelicBonusType.EmpowerLastsLonger,
                    RelicBonusType.WiderEmpower
                },
                RelicAffinity.Universal => new List<RelicBonusType>
                {
                    RelicBonusType.BonusGold,
                    RelicBonusType.BonusPlayerXp,
                    RelicBonusType.BonusPetXp,
                    RelicBonusType.BonusLootRoll
                },
                _ => new List<RelicBonusType>()
            };
        }



        // =========================
        // 💎 GET SHARDS
        // =========================
        public async Task<List<PlayerRelicShard>> GetShardsAsync(ulong discordId)
        {
            return await _repo.GetShardsAsync(discordId);
        }

        // =========================
        // 🏷️ FORMAT BONUS NAME
        // =========================
        public string FormatBonus(RelicBonusType bonus)
        {
            return bonus switch
            {
                RelicBonusType.AttackPercent => "Attack %",
                RelicBonusType.DefensePercent => "Defense %",
                RelicBonusType.MaxHpPercent => "Max HP %",
                RelicBonusType.ShattersLastLonger => "Shatter Lasts Longer",
                RelicBonusType.TauntCooldownReduction => "Taunt Cooldown Reduction",
                RelicBonusType.ExecutionerBonus => "Executioner Bonus",
                RelicBonusType.LifeSteal => "Life Steal",
                RelicBonusType.ConsecutiveHitBonus => "Consecutive Hit Bonus",
                RelicBonusType.IncreasedHealPercent => "Increased Heal %",
                RelicBonusType.ChanceToSavePotion => "Chance to Save Potion",
                RelicBonusType.EmpowerLastsLonger => "Empower Lasts Longer",
                RelicBonusType.WiderEmpower => "Wider Empower",
                RelicBonusType.BonusGold => "Bonus Gold",
                RelicBonusType.BonusPlayerXp => "Bonus Player XP",
                RelicBonusType.BonusPetXp => "Bonus Pet XP",
                RelicBonusType.BonusLootRoll => "Bonus Loot Roll",
                _ => bonus.ToString()
            };
        }
    }
}