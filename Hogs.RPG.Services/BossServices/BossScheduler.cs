using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Services.Game;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossScheduler : BackgroundService
    {
        private readonly BossService _bossService;
        private readonly DiscordSocketClient _client;

        private readonly ulong _channelId = 1479614302121103360;

        public BossScheduler(BossService bossService, DiscordSocketClient client)
        {
            _bossService = bossService;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                await HandleWeeklyBoss(now);
                await HandleDailyBosses(now);
                await CleanupExpiredBosses(); // 🔥 NEW

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // =========================
        // WEEKLY BOSS
        // =========================

        private async Task HandleWeeklyBoss(DateTime now)
        {
            if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 16)
                return;

            var weeklyActive = _bossService
                .GetAllActiveBosses()
                .Any(b => b.Definition.Type == BossType.Weekly);

            if (weeklyActive)
                return;

            var boss = await _bossService.SpawnWeeklyBoss();

            if (boss != null)
            {
                await AnnounceBoss(boss, "🔥 A Weekly Boss has spawned!");
            }
        }

        // =========================
        // DAILY BOSSES
        // =========================

        private async Task HandleDailyBosses(DateTime now)
        {
            await TrySpawnDaily("gravelmaw", now, 12);
            await TrySpawnDaily("primordial_serpent", now, 18);
            await TrySpawnDaily("xerathul", now, 22);
        }

        private async Task TrySpawnDaily(string bossId, DateTime now, int hour)
        {
            if (now.Hour < hour)
                return;

            if (_bossService.IsBossActive(bossId))
                return;

            var boss = await _bossService.SpawnBoss(bossId);

            if (boss != null)
            {
                await AnnounceBoss(boss, "⚔ A Daily Boss has appeared!");
            }
        }

        // =========================
        // CLEANUP SYSTEM 🔥
        // =========================

        private async Task CleanupExpiredBosses()
        {
            var expiredBosses = _bossService.GetExpiredBosses();

            foreach (var boss in expiredBosses)
            {
                await HandleBossEscape(boss);
                _bossService.RemoveBoss(boss.Definition.Id);
            }
        }

        private async Task HandleBossEscape(ActiveBoss boss)
        {
            var channel = _client.GetChannel(_channelId) as IMessageChannel;

            if (channel == null)
                return;

            var damageData = boss.DamageDealt;

            if (damageData.Count == 0)
            {
                await channel.SendMessageAsync($"⏰ **{boss.Definition.Name} escaped...** No one dealt damage.");
                return;
            }

            int maxHealth = boss.Definition.MaxHealth;
            int rewardPool = (int)(boss.Definition.RewardGold * 0.5);

            var message = new StringBuilder();

            message.AppendLine($"🏃 **{boss.Definition.Name} has escaped!**\n");
            message.AppendLine("💰 **Reduced Rewards (50%)**:");

            foreach (var entry in damageData)
            {
                var userId = entry.Key;
                var damage = entry.Value;

                double contribution = (double)damage / maxHealth;
                int goldReward = (int)(rewardPool * contribution);

                message.AppendLine($"<@{userId}> earned {goldReward} gold");
            }

            await channel.SendMessageAsync(message.ToString());
        }

        // =========================
        // ANNOUNCEMENT
        // =========================

        private async Task AnnounceBoss(ActiveBoss boss, string message)
        {
            var channel = _client.GetChannel(_channelId) as IMessageChannel;

            if (channel == null)
                return;

            var embed = BuildBossEmbed(boss);

            await channel.SendMessageAsync(
                $"@BossRaid {message}",
                false,
                embed
            );
        }

        // =========================
        // EMBED
        // =========================

        private Embed BuildBossEmbed(ActiveBoss boss)
        {
            var def = boss.Definition;

            return new EmbedBuilder()
                .WithTitle($"🔥 {def.Name} has appeared!")
                .WithDescription(def.Description ?? "A powerful boss challenges all adventurers!")

                .AddField("❤️ Health",
                    $"{GetHealthBar(boss.CurrentHealth, def.MaxHealth)}\n{boss.CurrentHealth}/{def.MaxHealth}",
                    true)

                .AddField("🛡 Defense", def.Defense, true)
                .AddField("💰 Reward", $"{def.RewardGold} Gold", true)

                .AddField("⚔ Abilities",
                    string.IsNullOrWhiteSpace(def.AbilitiesText)
                        ? "Unknown..."
                        : def.AbilitiesText)

                .WithImageUrl(def.ImageUrl)
                .WithColor(Color.DarkRed)
                .WithFooter("⏳ Defeat the boss before it escapes!")
                .WithCurrentTimestamp()
                .Build();
        }

        private string GetHealthBar(int current, int max)
        {
            int bars = 10;
            double percent = (double)current / max;
            int filled = (int)(percent * bars);

            return new string('█', filled) + new string('░', bars - filled);
        }
    }
}