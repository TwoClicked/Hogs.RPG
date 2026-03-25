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
        private readonly BossRepository _bossRepository;

        private readonly Random _random = new();
        private static readonly HashSet<int> SpawnHours = new() { 0, 3, 6, 9, 12, 15, 18, 21 };

        private readonly ulong _bossChannelId = 1485386835180916969;
        private readonly ulong _bossRoleId = 1485387222822948934;

        // Prevent multiple spawns per day
        private HashSet<string> _spawnedToday = new();

        private DateTime _lastReset = DateTime.MinValue;

        public BossScheduler(
            BossService bossService,
            DiscordSocketClient client,
            PlayerService playerService,
            PlayerRepository playerRepository,
            BossRepository bossRepository)
        {
            _bossService = bossService;
            _client = client;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _bossRepository = bossRepository;
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
                    // 🔥 Sync state from Google Sheets once per day (or first run)
                    if (_lastReset.Date != now.Date)
                    {
                        Console.WriteLine("🔄 Syncing boss state from Google Sheets");

                        await _bossRepository.ClearOldStateAsync(now);
                        _spawnedToday = await _bossRepository.LoadSpawnStateAsync(now);

                        _lastReset = now.Date;
                    }

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
        // DAILY
        // =========================
        private async Task HandleDailyBosses(DateTime now)
        {
            Console.WriteLine("➡ Checking scheduled bosses...");



            // ✅ Only allow spawn in first 2 minutes of valid hours
            if (!SpawnHours.Contains(now.Hour) || now.Minute >= 3)
            {
                Console.WriteLine($"   ❌ Outside spawn window ({now:HH:mm})");
                return;
            }

            // ✅ Prevent multiple spawns in same timeslot
            var timeslotKey = $"timeslot_{now.Date:yyyyMMdd}_{now.Hour}";

            if (_spawnedToday.Contains(timeslotKey))
            {
                Console.WriteLine("   ❌ Already spawned this timeslot");
                return;
            }

            // ✅ Get all DAILY bosses from repository
            var allDailyBosses = await _bossRepository.GetByTypeAsync(BossType.Daily);

            if (!allDailyBosses.Any())
            {
                Console.WriteLine("⚠️ No daily bosses found in repository");
                return;
            }

            // ✅ Filter out bosses already spawned today
            var availableBosses = allDailyBosses
                .Where(b => !_spawnedToday.Contains(b.Id))
                .ToList();

            if (!availableBosses.Any())
            {
                Console.WriteLine("⚠️ All daily bosses already spawned today.");
                return;
            }

            // 🎲 Random selection
            var selected = availableBosses[_random.Next(availableBosses.Count)];

            // ✅ Safety check (should rarely happen but good practice)
            if (_bossService.IsBossActive(selected.Id))
            {
                Console.WriteLine($"   ❌ Boss already active: {selected.Id}");
                return;
            }

            // ✅ Spawn boss
            var boss = await _bossService.SpawnBoss(selected.Id);

            if (boss != null)
            {
                Console.WriteLine($"   ✅ Spawned RANDOM boss: {selected.Name}");

                _spawnedToday.Add(selected.Id);
                _spawnedToday.Add(timeslotKey);

                await _bossRepository.SaveSpawnEntryAsync(now, selected.Id);
                await _bossRepository.SaveSpawnEntryAsync(now, timeslotKey);

                await AnnounceBoss(boss, "⚔ A mysterious boss has appeared!");
            }
            else
            {
                Console.WriteLine($"   ❌ Failed to spawn boss {selected.Id}");
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