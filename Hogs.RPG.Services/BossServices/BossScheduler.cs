using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
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
                await CleanupExpiredBosses();

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task HandleWeeklyBoss(DateTime now)
        {
            if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 16)
                return;

            var active = _bossService
                .GetAllActiveBosses()
                .Any(b => b.Definition.Type == BossType.Weekly);

            if (active) return;

            var boss = await _bossService.SpawnWeeklyBoss();
            if (boss != null)
                await AnnounceBoss(boss, "🔥 Weekly Boss spawned!");
        }

        private async Task HandleDailyBosses(DateTime now)
        {
            await TrySpawn("gravelmaw", now, 12);
            await TrySpawn("primordial_serpent", now, 18);
            await TrySpawn("xerathul", now, 22);
        }

        private async Task TrySpawn(string id, DateTime now, int hour)
        {
            if (now.Hour < hour) return;
            if (_bossService.IsBossActive(id)) return;

            var boss = await _bossService.SpawnBoss(id);
            if (boss != null)
                await AnnounceBoss(boss, "⚔ Daily Boss appeared!");
        }

        private async Task CleanupExpiredBosses()
        {
            var expired = _bossService.GetExpiredBosses();

            foreach (var boss in expired)
            {
                var channel = _client.GetChannel(_channelId) as IMessageChannel;

                if (channel != null)
                    await channel.SendMessageAsync($"⏰ {boss.Definition.Name} escaped!");

                _bossService.RemoveBoss(boss.Definition.Id);
            }
        }

        private async Task AnnounceBoss(ActiveBoss boss, string text)
        {
            var channel = _client.GetChannel(_channelId) as IMessageChannel;
            if (channel == null) return;

            var embed = _bossService.BuildBossEmbed(boss);

            var components = new ComponentBuilder()
                .WithButton("⚔ Attack", $"boss_attack:{boss.Definition.Id}", ButtonStyle.Danger)
                .WithButton("🧪 Heal", $"boss_heal:{boss.Definition.Id}", ButtonStyle.Success)
                .Build();

            var msg = await channel.SendMessageAsync(
                $"@BossRaid {text}",
                embed: embed,
                components: components
            );

            boss.ChannelId = channel.Id;
            boss.MessageId = msg.Id;
        }
    }
}