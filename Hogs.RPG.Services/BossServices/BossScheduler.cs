using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossScheduler : BackgroundService
    {
        private readonly BossService _bossService;
        private readonly DiscordSocketClient _client;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;

        private readonly ulong _bossChannelId = 1485386835180916969;
        private readonly ulong _bossRoleId = 1485387222822948934;

        // Prevent multiple spawns per day
        private readonly HashSet<string> _spawnedToday = new();

        public BossScheduler(
            BossService bossService,
            DiscordSocketClient client,
            PlayerService playerService,
            PlayerRepository playerRepository)
        {
            _bossService = bossService;
            _client = client;
            _playerService = playerService;
            _playerRepository = playerRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("🚀 BossScheduler started");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                Console.WriteLine($"\n[Scheduler Tick] {now:HH:mm:ss}");

                try
                {
                    // Reset daily spawn tracker at midnight
                    if (now.Hour == 0 && now.Minute < 1)
                    {
                        Console.WriteLine("🔄 Resetting daily spawn tracker");
                        _spawnedToday.Clear();
                    }

                    await HandleWeeklyBoss(now);
                    await HandleDailyBosses(now);
                    await CleanupExpiredBosses();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"💥 Scheduler crash: {ex}");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        // =========================
        // WEEKLY
        // =========================
        private async Task HandleWeeklyBoss(DateTime now)
        {
            Console.WriteLine("➡ Checking weekly boss spawn...");

            if (now.DayOfWeek != DayOfWeek.Sunday || now.Hour < 16)
            {
                Console.WriteLine("❌ Not time for weekly boss.");
                return;
            }

            var active = _bossService
                .GetAllActiveBosses()
                .Any(b => b.Definition.Type == BossType.Weekly);

            if (active)
            {
                Console.WriteLine("❌ Weekly boss already active.");
                return;
            }

            var boss = await _bossService.SpawnWeeklyBoss();

            if (boss != null)
            {
                Console.WriteLine($"✅ Weekly boss spawned: {boss.Definition.Name}");
                await AnnounceBoss(boss, "🔥 Weekly Boss spawned!");
            }
            else
            {
                Console.WriteLine("❌ No weekly boss available.");
            }
        }

        // =========================
        // DAILY
        // =========================
        private async Task HandleDailyBosses(DateTime now)
        {
            Console.WriteLine("➡ Checking daily bosses...");

            // Add/remove bosses here
            await TrySpawn("gravelmaw_02", now, 12);
            await TrySpawn("primordial_serpent_03", now, 18);
            await TrySpawn("xerathul_04", now, 22);
        }

        private async Task TrySpawn(string id, DateTime now, int hour)
        {
            Console.WriteLine($"   → Checking spawn for {id} at hour {hour}");

            if (now.Hour < hour)
            {
                Console.WriteLine($"   ❌ Too early for {id}");
                return;
            }

            if (_spawnedToday.Contains(id))
            {
                Console.WriteLine($"   ❌ {id} already spawned today");
                return;
            }

            if (_bossService.IsBossActive(id))
            {
                Console.WriteLine($"   ❌ {id} already active");
                return;
            }

            var boss = await _bossService.SpawnBoss(id);

            if (boss != null)
            {
                Console.WriteLine($"   ✅ Spawned {id}");

                _spawnedToday.Add(id);

                await AnnounceBoss(boss, "⚔ Daily Boss appeared!");
            }
            else
            {
                Console.WriteLine($"   ❌ Boss {id} not found in repo");
            }
        }

        // =========================
        // EXPIRY CLEANUP
        // =========================
        private async Task CleanupExpiredBosses()
        {
            Console.WriteLine("➡ Checking expired bosses...");

            var expiredBosses = _bossService.GetExpiredBosses().ToList();

            Console.WriteLine($"Expired bosses: {expiredBosses.Count}");

            foreach (var boss in expiredBosses)
            {
                Console.WriteLine($"💨 Boss expired: {boss.Definition.Name}");

                try
                {
                    var message = await _bossService.HandleBossExpiryAsync(boss);

                    var channel = _client.GetChannel(boss.ChannelId) as IMessageChannel;

                    if (channel != null)
                    {
                        await channel.SendMessageAsync(message);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Missing channel for boss {boss.Definition.Name}");
                    }

                    _bossService.RemoveBoss(boss.Definition.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error handling expired boss {boss.Definition.Name}: {ex.Message}");
                }
            }
        }

        // =========================
        // ANNOUNCE
        // =========================
        private async Task AnnounceBoss(ActiveBoss boss, string text)
        {
            Console.WriteLine($"📢 Announcing boss: {boss.Definition.Name}");

            var channel = _client.GetChannel(_bossChannelId) as IMessageChannel;

            if (channel == null)
            {
                Console.WriteLine("❌ Announcement channel not found!");
                return;
            }

            var embed = _bossService.BuildBossEmbed(boss);

            var components = new ComponentBuilder()
                .WithButton("⚔ Attack", $"boss_attack:{boss.Definition.Id}", ButtonStyle.Danger)
                .Build();

            var msg = await channel.SendMessageAsync(
                $"<@&{_bossRoleId}> **{boss.Definition.Name}** has appeared!\n{text}",
                embed: embed,
                components: components
            );

            boss.ChannelId = channel.Id;
            boss.MessageId = msg.Id;

            Console.WriteLine($"✅ Boss message sent | Channel: {boss.ChannelId} | Message: {boss.MessageId}");
        }
    }
}