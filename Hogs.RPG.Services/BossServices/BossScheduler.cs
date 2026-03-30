using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.Hosting;

using static Hogs.RPG.Core.Entities.BossDefinition;

namespace Hogs.RPG.Services.Game
{
    public class BossScheduler : BackgroundService
    {
        private readonly BossService _bossService;
        private readonly DiscordSocketClient _client;
        private readonly PlayerService _playerService;
        private readonly PlayerRepository _playerRepository;
        private readonly BossStateRepository _bossStateRepository;

        private readonly Random _random = new();

        // Spawn hours (currently disabled logic below, but kept for later use)  
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
            BossStateRepository bossStateRepository)
        {
            _bossService = bossService;
            _client = client;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _bossStateRepository = bossStateRepository;
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

                        try
                        {
                            await _bossStateRepository.ClearOldAsync(now);
                            _spawnedToday = await _bossStateRepository.LoadForDateAsync(now);

                            _lastReset = now.Date;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Failed syncing boss state: {ex.Message}");

                            // Fallback so system keeps working
                            _spawnedToday = new HashSet<string>();
                        }
                    }

                    // 🔥 Run systems independently so one failure doesn't kill everything
                    await HandleDailyBosses(now);
                    await CleanupExpiredBosses();
                }
                catch (Exception ex)
                {
                    // 🔥 LAST LINE OF DEFENSE (should rarely hit now)
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

            // ✅ Only allow spawn in first 2 minutes of valid hours (disabled for now)
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

            List<BossDefinition> allDailyBosses;

            try
            {

                allDailyBosses = GlobalBossRegistry.GetByType(BossType.Daily);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed loading bosses: {ex.Message}");
                return; // 🔥 DO NOT crash scheduler
            }

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

            // ✅ Safety check
            if (_bossService.IsBossActive(selected.Id))
            {
                Console.WriteLine($"   ❌ Boss already active: {selected.Id}");
                return;
            }

            ActiveBoss boss = null;

            try
            {
                boss = await _bossService.SpawnBoss(selected.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed spawning boss: {ex.Message}");
                return;
            }

            if (boss != null)
            {
                Console.WriteLine($"   ✅ Spawned RANDOM boss: {selected.Name}");

                _spawnedToday.Add(selected.Id);
                _spawnedToday.Add(timeslotKey);

                try
                {
                    await _bossStateRepository.SaveAsync(now, selected.Id);
                    await _bossStateRepository.SaveAsync(now, timeslotKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed saving spawn state: {ex.Message}");
                }

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

            List<ActiveBoss> expiredBosses;

            try
            {
                expiredBosses = _bossService.GetExpiredBosses().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed retrieving expired bosses: {ex.Message}");
                return;
            }

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