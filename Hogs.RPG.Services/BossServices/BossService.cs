using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.GameplayServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossService
    {
        private readonly BossRepository _bossRepository;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly StatService _statService;
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<string, ActiveBoss> _activeBosses = new();
        private readonly Dictionary<string, SemaphoreSlim> _locks = new();
        private readonly Dictionary<ulong, DateTime> _attackCooldowns = new();
        private readonly Dictionary<string, int> _pendingDamage = new();

        private static readonly Random _rand = new();

        public BossService(
            BossRepository bossRepository,
            PlayerService playerService,
            PlayerRepository playerRepository,
            StatService statService,
            DiscordSocketClient client)
        {
            _bossRepository = bossRepository;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _statService = statService;
            _client = client;
        }

        private SemaphoreSlim GetLock(string bossId)
        {
            if (!_locks.ContainsKey(bossId))
                _locks[bossId] = new SemaphoreSlim(1, 1);

            return _locks[bossId];
        }

        // =========================
        // COOLDOWN (FIXED)
        // =========================
        private bool TryCheckCooldown(ulong userId, out int remainingSeconds)
        {
            remainingSeconds = 0;

            if (!_attackCooldowns.TryGetValue(userId, out var lastAttack))
                return true;

            var diff = DateTime.UtcNow - lastAttack;

            if (diff.TotalSeconds >= 5)
                return true;

            remainingSeconds = (int)(5 - diff.TotalSeconds); // FIXED
            return false;
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
            _ = RunBossLoop(boss.Id, _client);

            return activeBoss;
        }

        public async Task<ActiveBoss> SpawnBoss(string bossId)
        {
            if (_activeBosses.ContainsKey(bossId))
                return _activeBosses[bossId];

            var boss = await _bossRepository.GetByIdAsync(bossId);
            if (boss == null) return null;

            var activeBoss = CreateActiveBoss(boss);
            _activeBosses[boss.Id] = activeBoss;
            _ = RunBossLoop(boss.Id, _client);

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

        public void ClearAllBosses()
        {
            _activeBosses.Clear();
            _locks.Clear();
            _attackCooldowns.Clear();
        }

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

        // =========================
        // ATTACK (DEFENSE-AWARE SYSTEM)
        // =========================

        public async Task<(int damage, int selfDamage, string text, bool isDead)>
            AttackBossAsync(string bossId, ulong userId)
        {
            if (!_activeBosses.ContainsKey(bossId))
                return (0, 0, "Boss not found.", false);

            if (!TryCheckCooldown(userId, out int remaining))
                return (0, 0, $"⏳ Wait {remaining}s before attacking again.", false);

            if (!_activeBosses.TryGetValue(bossId, out var boss))
                return (0, 0, "Boss not found.", false);

            var bossLock = GetLock(bossId);

            await bossLock.WaitAsync();

            try
            {
                if (boss.CurrentHealth <= 0)
                    return (0, 0, "Boss already defeated.", false);

                var player = await _playerRepository.GetByDiscordIdAsync(userId);

                if (player == null)
                    return (0, 0, "You need to start your adventure first.", false);

                var stats = _statService.CalculateStats(player);

                int playerAttack = stats.attack;
                int playerDefense = stats.defense;
                int playerMaxHp = stats.health;

                int roll = _rand.Next(-10, 11);

                // =========================
                // 🔥 PLAYER DAMAGE → BOSS
                // =========================
                double bossMitigation = boss.Definition.Defense / (boss.Definition.Defense + 100.0);
                int baseDamage = Math.Max(1, (int)(playerAttack * (1 - bossMitigation)));

                int finalDamage = baseDamage;
                int selfDamage = 0;
                string result;


                // =========================
                // 💀 CRITICAL FAIL (DEFENSE SCALED)
                // =========================
                if (roll == -10)
                {
                    double mitigation = playerDefense / (playerDefense + 100.0);

                    int rawDamage = (int)(playerMaxHp * 0.75);
                    selfDamage = (int)(rawDamage * (1 - mitigation));

                    player.Health = Math.Max(0, player.Health - selfDamage);

                    await _playerRepository.UpdatePlayerAsync(player);

                    _attackCooldowns[userId] = DateTime.UtcNow;

                    return (0, selfDamage, $"💀 Critical Fail! You took {selfDamage} damage!", false);
                }

                // =========================
                // ⚠️ NEGATIVE ROLL (DEFENSE SCALED)
                // =========================
                if (roll < 0)
                {
                    double percent = Math.Abs(roll) * 0.05;
                    int rawDamage = (int)(playerMaxHp * percent);

                    double mitigation = playerDefense / (playerDefense + 100.0);
                    selfDamage = (int)(rawDamage * (1 - mitigation));

                    player.Health = Math.Max(0, player.Health - selfDamage);

                    await _playerRepository.UpdatePlayerAsync(player);

                    _attackCooldowns[userId] = DateTime.UtcNow;

                    return (0, selfDamage, $"⚠️ You were punished for {selfDamage} damage!", false);
                }

                // =========================
                // ✨ CRITICAL HIT
                // =========================
                if (roll == 10)
                {
                    finalDamage *= 2;
                    result = $"✨ Critical Hit! {finalDamage} dmg!";
                }
                else if (roll > 0)
                {
                    finalDamage = (int)(finalDamage * (1 + roll * 0.05));
                    result = $"🎯 {finalDamage} damage!";
                }
                else
                {
                    result = $"➖ {finalDamage} damage";
                }

                // =========================
                // APPLY DAMAGE
                // =========================
                if (!_pendingDamage.ContainsKey(bossId))
                    _pendingDamage[bossId] = 0;

                _pendingDamage[bossId] += finalDamage;

                if (!boss.DamageDealt.ContainsKey(userId))
                    boss.DamageDealt[userId] = 0;

                boss.DamageDealt[userId] = boss.DamageDealt.GetValueOrDefault(userId) + finalDamage;


                if (finalDamage > 0)
                {
                    var hitText = $"+{finalDamage} <@{userId}>";

                    if (boss.RecentHits.Count >= 5)
                        boss.RecentHits.Dequeue();

                    boss.RecentHits.Enqueue(hitText);
                }

                _attackCooldowns[userId] = DateTime.UtcNow;

                bool isDead = false;

                return (finalDamage, 0, result, isDead);
            }
            finally
            {
                bossLock.Release();
            }
        }

        public async Task RunBossLoop(string bossId, DiscordSocketClient client)
        {
            while (_activeBosses.ContainsKey(bossId))
            {
                await Task.Delay(1000); // 1 second

                if (!_activeBosses.TryGetValue(bossId, out var boss))
                    return;
                var bossLock = GetLock(bossId);

                await bossLock.WaitAsync();

                try
                {
                    if (!_pendingDamage.ContainsKey(bossId))
                        continue;

                    int damage = _pendingDamage[bossId];

                    if (damage <= 0)
                        continue;

                    _pendingDamage[bossId] = 0;

                    boss.CurrentHealth = Math.Max(0, boss.CurrentHealth - damage);

                    // ✅ ONLY UI UPDATE HERE
                    await TryUpdateBossMessageAsync(boss, client);

                    if (boss.CurrentHealth <= 0)
                    {
                        var message = await HandleBossDeathAsync(boss);

                        var channel = client.GetChannel(boss.ChannelId) as IMessageChannel;

                        if (channel != null)
                            await channel.SendMessageAsync(message);

                        _activeBosses.Remove(bossId);
                        return;
                    }
                }
                finally
                {
                    bossLock.Release();
                }
            }
        }

        // =========================
        // DEATH
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

        public async Task<string> HandleBossExpiryAsync(ActiveBoss boss)
        {
            var damageData = boss.DamageDealt;

            if (damageData.Count == 0)
                return $"💨 {boss.Definition.Name} fled. No one dealt damage.";

            int maxHealth = boss.Definition.MaxHealth;
            int rewardPool = (int)(boss.Definition.RewardGold * 0.5); // 🔥 50%

            var sb = new StringBuilder();

            sb.AppendLine($"💨 **{boss.Definition.Name} fled!**\n");
            sb.AppendLine("⚔️ Partial Rewards (50%):\n");

            foreach (var entry in damageData)
            {
                var userId = entry.Key;
                var damage = entry.Value;

                double contribution = (double)damage / maxHealth;
                int gold = (int)(rewardPool * contribution);

                if (gold <= 0) continue;

                var player = await _playerService.GetOrCreatePlayerAsync(userId, "Unknown");
                player.Gold += gold;

                await _playerRepository.UpdatePlayerAsync(player);

                sb.AppendLine($"<@{userId}> +{gold} gold");
            }

            return sb.ToString();
        }

        // =========================
        // UI
        // =========================

        public async Task TryUpdateBossMessageAsync(ActiveBoss boss, DiscordSocketClient client)
        {
            if ((DateTime.UtcNow - boss.LastUiUpdate).TotalSeconds < 2)
                return;

            boss.LastUiUpdate = DateTime.UtcNow;

            var channel = client.GetChannel(boss.ChannelId) as IMessageChannel;
            if (channel == null) return;

            var message = await channel.GetMessageAsync(boss.MessageId);

            if (message is IUserMessage msg)
            {
                await msg.ModifyAsync(m =>
                {
                    m.Embed = BuildBossEmbed(boss);
                });
            }
        }

        public Embed BuildBossEmbed(ActiveBoss boss)
        {
            var def = boss.Definition;

            return new EmbedBuilder()
                .WithTitle($"🔥 {def.Name}")
                .WithDescription(def.Description)

                .AddField("❤️ Health",
                    $"{GetHealthBar(boss.CurrentHealth, def.MaxHealth)}\n{boss.CurrentHealth}/{def.MaxHealth}")
                .AddField("⚔️ Recent Hits",
                 boss.RecentHits.Count == 0
                     ? "No recent hits"
                     : string.Join("\n", boss.RecentHits))
                .AddField("🏆 Top Damage",
                    boss.DamageDealt.Count == 0
                        ? "No attacks yet..."
                        : string.Join("\n",
                            boss.DamageDealt
                                .OrderByDescending(x => x.Value)
                                .Take(5)
                                .Select((x, i) => $"{i + 1}. <@{x.Key}> — {x.Value}")
                        ))

                .WithImageUrl(def.ImageUrl)
                .WithColor(Color.DarkRed)
                .Build();
        }
        // ADD THIS METHOD (optional but recommended)
        public bool IsBossDead(ActiveBoss boss)
        {
            return boss.CurrentHealth <= 0;
        }
        private string GetHealthBar(int current, int max)
        {
            int bars = 10;
            int filled = (int)((double)current / max * bars);
            return new string('█', filled) + new string('░', bars - filled);
        }
    }
}