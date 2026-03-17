using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PlayerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossService
    {
        private readonly BossRepository _bossRepository;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;

        private readonly Dictionary<string, ActiveBoss> _activeBosses = new();

        private static readonly Random _rand = new Random();

        public BossService(
            BossRepository bossRepository,
            PlayerService playerService,
            PlayerRepository playerRepository)
        {
            _bossRepository = bossRepository;
            _playerService = playerService;
            _playerRepository = playerRepository;
        }

        // =========================
        // SPAWNING
        // =========================

        public async Task<ActiveBoss> SpawnWeeklyBoss()
        {
            var bosses = await _bossRepository.GetByTypeAsync(BossType.Weekly);

            if (!bosses.Any())
                return null;

            var boss = bosses[_rand.Next(bosses.Count)];

            if (_activeBosses.ContainsKey(boss.Id))
                return _activeBosses[boss.Id];

            var activeBoss = CreateActiveBoss(boss);

            _activeBosses[boss.Id] = activeBoss;

            return activeBoss;
        }

        public async Task<ActiveBoss> SpawnBoss(string bossId)
        {
            if (_activeBosses.ContainsKey(bossId))
                return _activeBosses[bossId];

            var boss = await _bossRepository.GetByIdAsync(bossId);

            if (boss == null)
                return null;

            var activeBoss = CreateActiveBoss(boss);

            _activeBosses[boss.Id] = activeBoss;

            return activeBoss;
        }

        private ActiveBoss CreateActiveBoss(BossDefinition boss)
        {
            return new ActiveBoss
            {
                Definition = boss,
                CurrentHealth = boss.MaxHealth,
                ExpireAt = DateTime.UtcNow.AddHours(1),
                DamageDealt = new Dictionary<ulong, int>()
            };
        }

        // =========================
        // GETTERS
        // =========================

        public ActiveBoss GetActiveBoss(string bossId)
        {
            if (!_activeBosses.ContainsKey(bossId))
                return null;

            var boss = _activeBosses[bossId];

            if (DateTime.UtcNow >= boss.ExpireAt)
                return null;

            return boss;
        }

        public List<ActiveBoss> GetAllActiveBosses()
        {
            return _activeBosses.Values
                .Where(b => DateTime.UtcNow < b.ExpireAt)
                .ToList();
        }

        public bool IsBossActive(string bossId)
        {
            return _activeBosses.ContainsKey(bossId) &&
                   DateTime.UtcNow < _activeBosses[bossId].ExpireAt;
        }

        public List<ActiveBoss> GetRawBosses()
        {
            return _activeBosses.Values.ToList();
        }

        // =========================
        // ATTACK SYSTEM
        // =========================

        public (int damageDealt, int selfDamage, int roll, string resultText)
            AttackBoss(string bossId, ulong userId, int playerAttack, int playerMaxHp)
        {
            if (!_activeBosses.ContainsKey(bossId))
                return (0, 0, 0, "Boss not found.");

            var boss = _activeBosses[bossId];

            if (DateTime.UtcNow >= boss.ExpireAt)
                return (0, 0, 0, "This boss has already expired.");

            int roll = _rand.Next(-10, 11);

            int baseDamage = Math.Max(1, playerAttack - boss.Definition.Defense);
            int finalDamage = baseDamage;

            int selfDamage = 0;
            string result;

            if (roll == -10)
            {
                selfDamage = (int)(playerMaxHp * 0.75);
                result = $"💀 Critical Fail! You took {selfDamage} damage!";
                return (0, selfDamage, roll, result);
            }

            if (roll < 0)
            {
                double percent = Math.Abs(roll) * 0.05;
                selfDamage = (int)(playerMaxHp * percent);

                result = $"⚠️ Roll {roll}: You hurt yourself for {selfDamage} damage!";
                return (0, selfDamage, roll, result);
            }

            if (roll == 10)
            {
                finalDamage *= 2;
                result = $"✨ Critical Hit! You dealt {finalDamage} damage!";
            }
            else if (roll > 0)
            {
                double multiplier = 1 + (roll * 0.05);
                finalDamage = (int)(finalDamage * multiplier);
                result = $"🎯 Roll {roll}: You dealt {finalDamage} damage!";
            }
            else
            {
                result = $"➖ Neutral hit: {finalDamage} damage";
            }

            boss.CurrentHealth -= finalDamage;
            if (boss.CurrentHealth < 0)
                boss.CurrentHealth = 0;

            if (!boss.DamageDealt.ContainsKey(userId))
                boss.DamageDealt[userId] = 0;

            boss.DamageDealt[userId] += finalDamage;

            return (finalDamage, 0, roll, result);
        }

        // =========================
        // REWARD HANDLING
        // =========================

        public async Task<string> HandleBossDeathAsync(ActiveBoss boss)
        {
            var damageData = boss.DamageDealt;

            if (damageData.Count == 0)
                return $"👑 {boss.Definition.Name} defeated, but no contributors.";

            int maxHealth = boss.Definition.MaxHealth;
            int rewardPool = boss.Definition.RewardGold;

            var leaderboard = damageData
                .OrderByDescending(x => x.Value)
                .Take(3)
                .ToList();

            var sb = new StringBuilder();

            sb.AppendLine($"👑 **{boss.Definition.Name} has been defeated!**\n");
            sb.AppendLine("🏆 Top Players:");

            for (int i = 0; i < leaderboard.Count; i++)
            {
                var (userId, dmg) = (leaderboard[i].Key, leaderboard[i].Value);

                string medal = i switch
                {
                    0 => "🥇",
                    1 => "🥈",
                    2 => "🥉",
                    _ => ""
                };

                sb.AppendLine($"{medal} <@{userId}> — {dmg} dmg");
            }

            sb.AppendLine("\n💰 Rewards:");

            foreach (var entry in damageData)
            {
                var userId = entry.Key;
                var damage = entry.Value;

                double contribution = (double)damage / maxHealth;
                int gold = (int)(rewardPool * contribution);

                var player = await _playerService.GetOrCreatePlayerAsync(userId, "Unknown");
                player.Gold += gold;

                await _playerRepository.UpdatePlayerAsync(player);

                sb.AppendLine($"<@{userId}> +{gold} gold");
            }

            return sb.ToString();
        }

        public async Task<string> HandleBossEscapeAsync(ActiveBoss boss)
        {
            var damageData = boss.DamageDealt;

            if (damageData.Count == 0)
                return $"⏰ {boss.Definition.Name} escaped... no one dealt damage.";

            int maxHealth = boss.Definition.MaxHealth;
            int rewardPool = (int)(boss.Definition.RewardGold * 0.5);

            var sb = new StringBuilder();

            sb.AppendLine($"🏃 **{boss.Definition.Name} has escaped!**\n");
            sb.AppendLine("💰 Reduced Rewards (50%):");

            foreach (var entry in damageData)
            {
                var userId = entry.Key;
                var damage = entry.Value;

                double contribution = (double)damage / maxHealth;
                int gold = (int)(rewardPool * contribution);

                var player = await _playerService.GetOrCreatePlayerAsync(userId, "Unknown");
                player.Gold += gold;

                await _playerRepository.UpdatePlayerAsync(player);

                sb.AppendLine($"<@{userId}> +{gold} gold");
            }

            return sb.ToString();
        }

        // =========================
        // CLEANUP
        // =========================

        public List<ActiveBoss> GetExpiredBosses()
        {
            return _activeBosses.Values
                .Where(b => DateTime.UtcNow >= b.ExpireAt)
                .ToList();
        }

        public void RemoveBoss(string bossId)
        {
            _activeBosses.Remove(bossId);
        }

        public void ClearAllBosses()
        {
            _activeBosses.Clear();
        }
    }
}