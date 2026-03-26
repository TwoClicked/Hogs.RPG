using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
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
        private readonly StatService _statService;
        private readonly InventoryService _inventoryService;
        private readonly DiscordSocketClient _client;
        private readonly LevelService _levelService;

        private readonly Dictionary<string, ActiveBoss> _activeBosses = new();

        private readonly ulong _feedChannelId = 1485357755433750549;

        private static readonly Random _rand = new();

        public BossService(
            BossRepository bossRepository,
            PlayerService playerService,
            PlayerRepository playerRepository,
            StatService statService,
            InventoryService inventoryService,
            DiscordSocketClient client,
            LevelService levelService)
        {
            _bossRepository = bossRepository;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _statService = statService;
            _inventoryService = inventoryService;
            _client = client;
            _levelService = levelService;
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
            if (boss == null) return null;

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
                ExpireAt = DateTime.UtcNow.AddHours(1)
            };
        }

        // =========================
        // ATTACK (CORE SYSTEM)
        // =========================
        public async Task AttackAsync(string bossId, ulong userId)
        {
            if (!_activeBosses.TryGetValue(bossId, out var boss))
                return;

            if (boss.IsDead || DateTime.UtcNow >= boss.ExpireAt)
                return;

            var player = await _playerRepository.GetByDiscordIdAsync(userId);
            if (player == null)
                return;

            var (attack, defense, maxHp) = _statService.CalculateStats(player);

            int damage = (int)(attack * (100.0 / (100 + boss.Definition.Defense)));
            damage = Math.Max(1, damage);

            boss.CurrentHealth = Math.Max(0, boss.CurrentHealth - damage);

            // Track DPS
            if (!boss.DamageDealt.ContainsKey(userId))
                boss.DamageDealt[userId] = 0;

            boss.DamageDealt[userId] += damage;
            boss.Participants.Add(userId);

            // 🔥 Death check (safe)
            if (boss.CurrentHealth <= 0 && !boss.IsDead)
            {
                boss.IsDead = true;

                var message = await HandleBossDeathAsync(boss);

                var channel = _client.GetChannel(boss.ChannelId) as IMessageChannel;
                if (channel != null)
                    await channel.SendMessageAsync(message);

                _activeBosses.Remove(bossId);
                return;
            }

            await TryUpdateBossMessageAsync(boss);
        }

        // =========================
        // EXPIRY
        // =========================
        public List<ActiveBoss> GetExpiredBosses()
        {
            return _activeBosses.Values
                .Where(b => DateTime.UtcNow >= b.ExpireAt && !b.IsDead)
                .ToList();
        }

        public async Task<string> HandleBossExpiryAsync(ActiveBoss boss)
        {
            if (boss.DamageDealt.Count == 0)
                return $"💨 {boss.Definition.Name} fled. No one dealt damage.";

            int reward = (int)(boss.Definition.RewardGold * 0.5);

            var sb = new StringBuilder();

            sb.AppendLine($"💨 **{boss.Definition.Name} fled!**\n");
            sb.AppendLine($"💰 Everyone receives {reward} gold\n");

            foreach (var userId in boss.Participants)
            {
                var player = await _playerService.GetOrCreatePlayerAsync(userId, "Unknown");

                // 💰 Apply reward
                player.Gold += reward;

                // 🔥 CRITICAL: normalize HP to prevent 0 / invalid state
                var (atk, def, maxHp) = _statService.CalculateStats(player);

                if (player.Health <= 0)
                    player.Health = maxHp;

                player.Health = Math.Min(player.Health, maxHp);

                await _playerRepository.UpdatePlayerAsync(player);

                sb.AppendLine($"<@{userId}> +{reward} gold");
            }

            return sb.ToString();
        }

        public void RemoveBoss(string bossId)
        {
            _activeBosses.Remove(bossId);
        }

        public List<ActiveBoss> GetAllActiveBosses()
        {
            return _activeBosses.Values
                .Where(b => !b.IsDead && DateTime.UtcNow < b.ExpireAt)
                .ToList();
        }

        public bool IsBossActive(string bossId)
        {
            return _activeBosses.ContainsKey(bossId) &&
                   !_activeBosses[bossId].IsDead &&
                   DateTime.UtcNow < _activeBosses[bossId].ExpireAt;
        }

        public ActiveBoss GetActiveBoss(string bossId)
        {
            if (!_activeBosses.ContainsKey(bossId))
                return null;

            var boss = _activeBosses[bossId];

            if (boss.IsDead || DateTime.UtcNow >= boss.ExpireAt)
                return null;

            return boss;
        }

        public async Task<string> HandleBossDeathAsync(ActiveBoss boss)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"👑 **{boss.Definition.Name} defeated!**\n");

            // =========================
            // 🏆 DPS LEADERBOARD
            // =========================
            sb.AppendLine("🏆 **Top DPS:**");

            var leaderboard = boss.DamageDealt
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            foreach (var (userId, dmg) in leaderboard)
            {
                sb.AppendLine($"• <@{userId}> — **{dmg} dmg**");
            }

            sb.AppendLine();

            // =========================
            // 💰 GOLD + XP
            // =========================
            int reward = boss.Definition.RewardGold;

            sb.AppendLine($"💰 **Everyone receives {reward} gold AND 5000 XP**\n");

            // =========================
            // 🎁 DROPS
            // =========================
            if (!BossDropRegistry.Drops.TryGetValue(boss.Definition.Id, out var lootTable))
                lootTable = new List<BossLoot>();

            var dropResults = new Dictionary<ulong, List<string>>();

            // =========================
            // 👥 REWARD LOOP
            // =========================
            foreach (var userId in boss.Participants)
            {
                var player = await _playerService.GetOrCreatePlayerAsync(userId, "Unknown");

                // 💰 Apply rewards
                player.Gold += reward;
                player.XP += 5000;

                // 🔥 LEVEL UP CHECK
                var (levelMessage, levelsGained) = _levelService.CheckLevelUp(player);

                var playerDrops = new List<string>();

                // =========================
                // 🎁 DROPS (DPS GATED)
                // =========================
                if (boss.DamageDealt.TryGetValue(userId, out var totalDamage) && totalDamage >= 10000)
                {
                    foreach (var loot in lootTable)
                    {
                        int roll = _rand.Next(1, 101);

                        if (roll <= loot.DropChance)
                        {
                            int amount = _rand.Next(loot.MinAmount, loot.MaxAmount + 1);

                            // ✅ GIVE ITEM (no reload after!)
                            await _inventoryService.GiveItemAsync(userId, loot.ItemId, amount);

                            if (InventoryItemDefinitions.All.TryGetValue(loot.ItemId, out var item))
                            {
                                string line = $"{item.Icon} **{item.Name}**";

                                if (amount > 1)
                                    line += $" x{amount}";

                                playerDrops.Add(line);
                            }
                            else
                            {
                                playerDrops.Add($"**{loot.ItemId}** x{amount}");
                            }
                        }
                    }
                }

                if (playerDrops.Count > 0)
                    dropResults[userId] = playerDrops;

                // =========================
                // 🔧 NORMALIZE HEALTH
                // =========================
                var (_, _, maxHp) = _statService.CalculateStats(player);

                if (player.Health <= 0)
                    player.Health = maxHp;

                player.Health = Math.Min(player.Health, maxHp);

                // =========================
                // 💾 SAVE PLAYER
                // =========================
                await _playerRepository.UpdatePlayerAsync(player);

                // =========================
                // 🎉 LEVEL ANNOUNCEMENT
                // =========================
                if (levelsGained > 0)
                {
                    var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;

                    if (channel != null)
                    {
                        await channel.SendMessageAsync(
                            $"🎉 <@{player.DiscordId}> reached **Level {player.Level}**!"
                        );
                    }
                }

                // 🔥 CRITICAL: prevent API overload
                await Task.Delay(75);
            }

            // =========================
            // 🎁 OUTPUT DROPS (CLEAN UI)
            // =========================
            if (boss.Participants.Count > 0)
            {
                sb.AppendLine("\n🎁 **Drops:**\n");

                var winners = new List<string>();
                var unlucky = new List<string>();
                var notEligible = new List<string>();

                foreach (var userId in boss.Participants)
                {
                    var mention = $"<@{userId}>";

                    bool hasDrops = dropResults.ContainsKey(userId);
                    boss.DamageDealt.TryGetValue(userId, out var dmg);

                    if (hasDrops)
                    {
                        var drops = string.Join("\n  • ", dropResults[userId]);
                        winners.Add($"{mention}\n  • {drops}");
                    }
                    else if (dmg >= 10000)
                    {
                        unlucky.Add(mention);
                    }
                    else
                    {
                        notEligible.Add(mention);
                    }
                }

                if (winners.Count > 0)
                {
                    sb.AppendLine("🏆 **Loot Winners**");
                    foreach (var w in winners)
                        sb.AppendLine(w + "\n");
                }

                if (unlucky.Count > 0)
                {
                    sb.AppendLine("😢 **No Drops (Eligible)**");
                    sb.AppendLine(string.Join(", ", unlucky) + "\n");
                }

                if (notEligible.Count > 0)
                {
                    sb.AppendLine("❌ **Not Enough Damage (10,000 required)**");
                    sb.AppendLine(string.Join(", ", notEligible) + "\n");
                }
            }
            else
            {
                sb.AppendLine("\n🎁 *No participants...*");
            }

            return sb.ToString();
        }

        // =========================
        // UI
        // =========================
        public async Task TryUpdateBossMessageAsync(ActiveBoss boss)
        {
            if ((DateTime.UtcNow - boss.LastUiUpdate).TotalSeconds < 2)
                return;

            boss.LastUiUpdate = DateTime.UtcNow;

            var channel = _client.GetChannel(boss.ChannelId) as IMessageChannel;
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
                .AddField("🏆 Top DPS",
                    boss.DamageDealt.Count == 0
                        ? "No attacks yet..."
                        : string.Join("\n",
                            boss.DamageDealt
                                .OrderByDescending(x => x.Value)
                                .Take(20)
                                .Select((x, i) => $"{i + 1}. <@{x.Key}> — {x.Value}")
                        ))
                .WithImageUrl(def.ImageUrl)
                .WithColor(Color.DarkRed)
                .Build();
        }

        private string GetHealthBar(int current, int max)
        {
            int bars = 10;
            int filled = (int)((double)current / max * bars);
            return new string('█', filled) + new string('░', bars - filled);
        }
    }
}