using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Entities.DungeonObjects;
using Hogs.RPG.Core.Entities.GlobalBossObjects;
using Hogs.RPG.Core.Entities.PetObjects;
using Hogs.RPG.Core.GameData.InventoryItems;
using Hogs.RPG.Core.GameData.Pets;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.AchievementServices;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.InventoryServices;
using Hogs.RPG.Services.PetServices;
using Hogs.RPG.Services.PlayerServices;
using Hogs.RPG.Services.RelicServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.GlobalBossObjects.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordSocketClient _client;

        private readonly Dictionary<string, ActiveBoss> _activeBosses = new();
        private readonly ulong _feedChannelId = 1485357755433750549;
        private static readonly Random _rand = new();

        // =========================
        // SCHEDULED SPAWN HOURS
        // Mirrors BossScheduler.SpawnHours (UTC). Used by
        // GetTimeUntilNextScheduledBoss() for the trade boss-blackout
        // lock. If you ever change the spawn schedule, update both
        // places — or better, move this into a shared static.
        // =========================
        private static readonly HashSet<int> SpawnHours = new() { 0, 3, 6, 9, 12, 15, 18, 21 };

        public BossService(IServiceScopeFactory scopeFactory, DiscordSocketClient client)
        {
            _scopeFactory = scopeFactory;
            _client = client;
        }

        public async Task<ActiveBoss> SpawnBoss(string bossId)
        {
            if (_activeBosses.ContainsKey(bossId))
                return _activeBosses[bossId];

            var boss = GlobalBossRegistry.GetById(bossId);
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

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var petPassiveService = scope.ServiceProvider.GetRequiredService<PetPassiveService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();

            var player = await playerRepo.GetByDiscordIdAsync(userId);
            if (player == null) return;

            var (attack, defense, maxHp) = await statService.CalculateStatsAsync(player);

            int damage = (int)(attack * (100.0 / (100 + boss.Definition.Defense)));
            damage = Math.Max(1, damage);

            // Pet passives
            var pet = await petService.GetEquippedPetAsync(userId);
            PetDefinition petDef = null;
            if (pet != null)
                PetRegistry.All.TryGetValue(pet.PetId, out petDef);

            (damage, _) = petPassiveService.ModifyOutgoingDamage(damage, pet, petDef, boss.CurrentHealth, boss.Definition.MaxHealth);

            // Relic bonuses
            var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

            bool isFirstJoin = !boss.Participants.Contains(userId);
            double bossHpPercent = (double)boss.CurrentHealth / boss.Definition.MaxHealth;

            if (isFirstJoin && bossHpPercent < 0.25)
            {
                boss.CurrentHealth += 10000;
            }

            if (bossHpPercent < 0.50 && relicBonuses.ExecutionerBonusPercent > 0)
                damage = (int)(damage * (1f + relicBonuses.ExecutionerBonusPercent));

            boss.CurrentHealth = Math.Max(0, boss.CurrentHealth - damage);

            if (!boss.DamageDealt.ContainsKey(userId))
                boss.DamageDealt[userId] = 0;

            boss.DamageDealt[userId] += damage;
            boss.Participants.Add(userId);

            player.TotalBossDamage += damage;
            await playerRepo.UpdatePlayerAsync(player);

            // =========================
            // 🏆 ACHIEVEMENT CHECK
            // =========================
            var achievementService = scope.ServiceProvider.GetRequiredService<AchievementService>();
            await achievementService.CheckAndAwardAsync(userId);

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

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();

            foreach (var userId in boss.Participants)
            {
                var player = await playerService.GetOrCreatePlayerAsync(userId, "Unknown");
                var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

                int goldWithBonus = (int)(reward * (1f + relicBonuses.BonusGoldPercent));
                int petXp = (int)(25 * (1f + relicBonuses.BonusPetXpPercent));

                player.Gold += goldWithBonus;

                var (atk, def, maxHp) = await statService.CalculateStatsAsync(player);
                if (player.Health <= 0) player.Health = maxHp;
                player.Health = Math.Min(player.Health, maxHp);

                await playerRepo.UpdatePlayerAsync(player);
                await petService.AddXPAsync(userId, petXp);

                sb.AppendLine($"<@{userId}> +{goldWithBonus} gold");
            }

            return sb.ToString();
        }

        public void RemoveBoss(string bossId) => _activeBosses.Remove(bossId);

        public List<ActiveBoss> GetAllActiveBosses() =>
            _activeBosses.Values.Where(b => !b.IsDead && DateTime.UtcNow < b.ExpireAt).ToList();

        public bool IsBossActive(string bossId) =>
            _activeBosses.ContainsKey(bossId) && !_activeBosses[bossId].IsDead && DateTime.UtcNow < _activeBosses[bossId].ExpireAt;

        public bool IsAnyBossActive() =>
            _activeBosses.Values.Any(b => !b.IsDead && DateTime.UtcNow < b.ExpireAt);

        public ActiveBoss GetActiveBoss(string bossId)
        {
            if (!_activeBosses.ContainsKey(bossId)) return null;
            var boss = _activeBosses[bossId];
            if (boss.IsDead || DateTime.UtcNow >= boss.ExpireAt) return null;
            return boss;
        }

        // =========================
        // TIME UNTIL NEXT SCHEDULED BOSS
        // Pure math against the fixed SpawnHours schedule — no new
        // persisted state. Used by TradeModule to enforce a trading
        // blackout window in the run-up to a scheduled boss spawn.
        // Walks forward hour by hour (max 24 checks) until it finds
        // the next UTC hour that's in SpawnHours.
        // =========================
        public TimeSpan GetTimeUntilNextScheduledBoss()
        {
            var now = DateTime.UtcNow;

            for (int addHours = 0; addHours <= 24; addHours++)
            {
                var candidate = now.AddHours(addHours);
                var candidateHourStart = new DateTime(
                    candidate.Year, candidate.Month, candidate.Day, candidate.Hour, 0, 0, DateTimeKind.Utc);

                bool isSpawnHour = SpawnHours.Contains(candidateHourStart.Hour);
                bool isFutureOrNow = candidateHourStart >= now;

                if (isSpawnHour && isFutureOrNow)
                    return candidateHourStart - now;
            }

            // Should never be reached given SpawnHours always has entries
            // within 24h, but return a safe default rather than throwing.
            return TimeSpan.FromHours(24);
        }

        // =========================
        // BOSS DEATH (TEMP WITH ROLL LOGS)
        // =========================
        public async Task<string> HandleBossDeathAsync(ActiveBoss boss)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"👑 **{boss.Definition.Name} defeated!**\n");

            sb.AppendLine("🏆 **Top DPS:**");
            var leaderboard = boss.DamageDealt.OrderByDescending(x => x.Value).Take(5).ToList();
            foreach (var (userId, dmg) in leaderboard)
                sb.AppendLine($"• <@{userId}> — **{dmg} dmg**");

            sb.AppendLine();

            int reward = boss.Definition.RewardGold;
            sb.AppendLine($"💰 **Everyone receives {reward} gold AND 2500 XP**\n");

            if (!BossDropRegistry.Drops.TryGetValue(boss.Definition.Id, out var lootTable))
                lootTable = new List<BossLoot>();

            var dropResults = new Dictionary<ulong, List<string>>();
            var rollLogs = new Dictionary<ulong, List<string>>();

            var top3 = boss.DamageDealt.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToHashSet();
            var top3Mentions = new List<string>();

            using var scope = _scopeFactory.CreateScope();
            var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();
            var playerService = scope.ServiceProvider.GetRequiredService<PlayerService>();
            var statService = scope.ServiceProvider.GetRequiredService<StatService>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<InventoryService>();
            var levelService = scope.ServiceProvider.GetRequiredService<LevelService>();
            var petService = scope.ServiceProvider.GetRequiredService<PetService>();
            var relicService = scope.ServiceProvider.GetRequiredService<RelicService>();
            var achievementService = scope.ServiceProvider.GetRequiredService<AchievementService>();

            foreach (var userId in boss.Participants)
            {
                var player = await playerService.GetOrCreatePlayerAsync(userId, "Unknown");
                var relicBonuses = await relicService.GetRelicBonusesAsync(userId);

                int gold = (int)(reward * (1f + relicBonuses.BonusGoldPercent));
                int xp = (int)(2500 * (1f + relicBonuses.BonusPlayerXpPercent));
                int petXp = (int)(50 * (1f + relicBonuses.BonusPetXpPercent));

                player.Gold += gold;
                player.XP += xp;

                if (top3.Contains(userId))
                {
                    player.Gold += (int)(250 * (1f + relicBonuses.BonusGoldPercent));
                    player.XP += (int)(2500 * (1f + relicBonuses.BonusPlayerXpPercent));
                    top3Mentions.Add($"<@{userId}>");
                }

                var (levelMessage, levelsGained) = levelService.CheckLevelUp(player);

                var (petLeveled, petLevel) = await petService.AddXPAsync(userId, petXp);

                if (petLeveled)
                {
                    var feedChannel = _client.GetChannel(_feedChannelId) as IMessageChannel;
                    if (feedChannel != null)
                        await feedChannel.SendMessageAsync($"🐾 <@{userId}>'s pet reached **Level {petLevel}**! 🎉");
                }

                var playerDrops = new List<string>();
                var playerRolls = new List<string>();

                boss.DamageDealt.TryGetValue(userId, out var totalDamage);

                if (totalDamage >= 10000)
                {
                    foreach (var loot in lootTable)
                    {
                        int roll = _rand.Next(1, 101);
                        double effectiveChance = loot.DropChance + relicBonuses.BonusLootRollPercent * 100;
                        bool dropped = roll <= effectiveChance;

                        string itemLabel = InventoryItemDefinitions.All.TryGetValue(loot.ItemId, out var itemDef)
                            ? $"{itemDef.Icon} {itemDef.Name}" : loot.ItemId;

                        string rollLine = dropped
                            ? $"✅ **{itemLabel}** — rolled **{roll}** (needed ≤{loot.DropChance}) → **DROP!**"
                            : $"❌ **{itemLabel}** — rolled **{roll}** (needed ≤{loot.DropChance}) → miss";

                        playerRolls.Add(rollLine);

                        if (dropped)
                        {
                            int amount = _rand.Next(loot.MinAmount, loot.MaxAmount + 1);
                            await inventoryService.GiveItemAsync(userId, loot.ItemId, amount);
                            InventoryItemDefinitions.All.TryGetValue(loot.ItemId, out var droppedDef);
                            string line = $"{droppedDef?.Icon ?? ""} **{droppedDef?.Name ?? loot.ItemId}**";
                            if (amount > 1) line += $" x{amount}";
                            playerDrops.Add(line);
                        }
                    }
                }
                else
                {
                    playerRolls.Add($"⚠️ Not eligible — only dealt **{totalDamage} dmg** (10,000 required), no rolls.");
                }

                rollLogs[userId] = playerRolls;
                if (playerDrops.Count > 0)
                    dropResults[userId] = playerDrops;

                var (_, _, maxHp) = await statService.CalculateStatsAsync(player);
                if (player.Health <= 0) player.Health = maxHp;
                player.Health = Math.Min(player.Health, maxHp);

                await playerRepo.UpdatePlayerAsync(player);

                // =========================
                // 🏆 ACHIEVEMENT CHECK
                // =========================
                await achievementService.CheckAndAwardAsync(userId);

                if (levelsGained > 0)
                {
                    var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
                    if (channel != null)
                        await channel.SendMessageAsync($"🎉 <@{player.DiscordId}> reached **Level {player.Level}**!");
                }

                await Task.Delay(75);
            }

            if (top3Mentions.Count > 0)
                sb.AppendLine($"🌟 **Top 3 Bonus:** {string.Join(", ", top3Mentions)} received an extra **250 gold** and **2500 XP** for their outstanding damage!\n");

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
                        winners.Add($"{mention}\n  • {string.Join("\n  • ", dropResults[userId])}");
                    else if (dmg >= 10000)
                        unlucky.Add(mention);
                    else
                        notEligible.Add(mention);
                }

                if (winners.Count > 0) { sb.AppendLine("🏆 **Loot Winners**"); foreach (var w in winners) sb.AppendLine(w + "\n"); }
                if (unlucky.Count > 0) { sb.AppendLine("😢 **No Drops (Eligible)**"); sb.AppendLine(string.Join(", ", unlucky) + "\n"); }
                if (notEligible.Count > 0) { sb.AppendLine("❌ **Not Enough Damage (10,000 required)**"); sb.AppendLine(string.Join(", ", notEligible) + "\n"); }
            }
            else
            {
                sb.AppendLine("\n🎁 *No participants...*");
            }

            // Roll transparency log
            var feedChan = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (feedChan != null && rollLogs.Count > 0)
            {
                var logSb = new StringBuilder();
                logSb.AppendLine($"🎲 **Drop Roll Log — {boss.Definition.Name}**\n");

                foreach (var (userId, rolls) in rollLogs)
                {
                    boss.DamageDealt.TryGetValue(userId, out var dmg);
                    logSb.AppendLine($"**<@{userId}>** *(dealt {dmg} dmg)*");
                    foreach (var roll in rolls) logSb.AppendLine($"  {roll}");
                    logSb.AppendLine();

                    if (logSb.Length > 1700)
                    {
                        await feedChan.SendMessageAsync(logSb.ToString());
                        logSb.Clear();
                    }
                }

                if (logSb.Length > 0)
                    await feedChan.SendMessageAsync(logSb.ToString());
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
                await msg.ModifyAsync(m => { m.Embed = BuildBossEmbed(boss); });
        }

        public Embed BuildBossEmbed(ActiveBoss boss)
        {
            var def = boss.Definition;

            return new EmbedBuilder()
                .WithTitle($"🔥 {def.Name}")
                .WithDescription(def.Description)
                .AddField("❤️ Health", $"{GetHealthBar(boss.CurrentHealth, def.MaxHealth)}\n{boss.CurrentHealth}/{def.MaxHealth}")
                .AddField("🏆 Top DPS",
                    boss.DamageDealt.Count == 0
                        ? "No attacks yet..."
                        : string.Join("\n", boss.DamageDealt.OrderByDescending(x => x.Value).Take(20).Select((x, i) => $"{i + 1}. <@{x.Key}> — {x.Value}")))
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