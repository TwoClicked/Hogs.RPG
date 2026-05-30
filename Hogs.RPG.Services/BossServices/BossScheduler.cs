using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Core.Registries;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.GameplayServices;
using Hogs.RPG.Services.PlayerServices;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly Random _random = new();

        private static readonly HashSet<int> SpawnHours = new() { 0, 3, 6, 9, 12, 15, 18, 21 };

        private readonly ulong _bossChannelId = 1485386835180916969;
        private readonly ulong _bossRoleId = 1485387222822948934;

        private HashSet<string> _spawnedToday = new();
        private HashSet<string> _preWarnedToday = new();

        private DateTime _lastReset = DateTime.MinValue;

        public BossScheduler(
            BossService bossService,
            DiscordSocketClient client,
            PlayerService playerService,
            PlayerRepository playerRepository,
            BossStateRepository bossStateRepository,
            IServiceScopeFactory scopeFactory)
        {
            _bossService = bossService;
            _client = client;
            _playerService = playerService;
            _playerRepository = playerRepository;
            _bossStateRepository = bossStateRepository;
            _scopeFactory = scopeFactory;
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
                    // Daily state reset
                    if (_lastReset.Date != now.Date)
                    {
                        try
                        {
                            await _bossStateRepository.ClearOldAsync(now);
                            _spawnedToday = await _bossStateRepository.LoadForDateAsync(now);
                            _preWarnedToday = new HashSet<string>();
                            _lastReset = now.Date;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Failed syncing boss state: {ex.Message}");
                            _spawnedToday = new HashSet<string>();
                            _preWarnedToday = new HashSet<string>();
                        }
                    }

                    await HandlePreBossWarning(now);
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
        // PRE-BOSS WARNING + AUTO-SWAP
        // Fires at minute 55 of the hour before each spawn hour.
        // Sends one embed to the announce channel — no tags.
        // =========================
        private async Task HandlePreBossWarning(DateTime now)
        {
            int nextHour = (now.Hour + 1) % 24;

            if (!SpawnHours.Contains(nextHour) || now.Minute < 55 || now.Minute > 56)
                return;

            var warnKey = $"prewarn_{now.Date:yyyyMMdd}_{nextHour}";
            if (_preWarnedToday.Contains(warnKey))
                return;

            _preWarnedToday.Add(warnKey);

            Console.WriteLine($"⚔️ Pre-boss warning for {nextHour:00}:00 — auto-swapping combat presets...");

            var swappedNames = new List<string>();

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var gearSetRepo = scope.ServiceProvider.GetRequiredService<GearSetRepository>();
                var gearSetService = scope.ServiceProvider.GetRequiredService<GearSetService>();
                var playerRepo = scope.ServiceProvider.GetRequiredService<PlayerRepository>();

                var presets = await gearSetRepo.GetAllSlot1SetsAsync();

                Console.WriteLine($"   Found {presets.Count} player(s) with a combat preset.");

                foreach (var preset in presets)
                {
                    try
                    {
                        await gearSetService.LoadSetAsync(preset.DiscordId, 1);

                        var player = await playerRepo.GetByDiscordIdAsync(preset.DiscordId);
                        swappedNames.Add(player?.Username ?? preset.DiscordId.ToString());

                        Console.WriteLine($"   ✅ Swapped gear for {preset.DiscordId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️ Swap failed for {preset.DiscordId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Pre-boss auto-swap failed: {ex.Message}");
                return;
            }

            // Skip the announcement entirely if nobody had a preset saved
            if (!swappedNames.Any())
            {
                Console.WriteLine("   No presets found — skipping pre-boss announcement.");
                return;
            }

            var channel = _client.GetChannel(1485357755433750549) as IMessageChannel;
            if (channel == null)
            {
                Console.WriteLine("❌ Announce channel not found for pre-boss warning.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("⚔️ Boss incoming in 5 minutes!")
                .WithColor(new Color(0xFF6600))
                .AddField(
                    "🛡️ Combat preset auto-equipped",
                    string.Join(", ", swappedNames))
                .AddField(
                    "📋 Everyone else",
                    "All other players' gear has been left untouched.")
                .WithFooter("Save your fighter build to Gear Set 1 to be included next time.")
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }

        // =========================
        // DAILY BOSS SPAWN
        // =========================
        private async Task HandleDailyBosses(DateTime now)
        {
            Console.WriteLine("➡ Checking scheduled bosses...");

            if (!SpawnHours.Contains(now.Hour) || now.Minute >= 3)
            {
                Console.WriteLine($"   ❌ Outside spawn window ({now:HH:mm})");
                return;
            }

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
                return;
            }

            if (!allDailyBosses.Any())
            {
                Console.WriteLine("⚠️ No daily bosses found in repository");
                return;
            }

            var availableBosses = allDailyBosses
                .Where(b => !_spawnedToday.Contains(b.Id))
                .ToList();

            if (!availableBosses.Any())
            {
                Console.WriteLine("⚠️ All daily bosses already spawned today.");
                return;
            }

            var selected = availableBosses[_random.Next(availableBosses.Count)];

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
                        await channel.SendMessageAsync(message);
                    else
                        Console.WriteLine($"⚠️ Missing channel for boss {boss.Definition.Name}");

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
                components: components);

            boss.ChannelId = channel.Id;
            boss.MessageId = msg.Id;

            Console.WriteLine($"✅ Boss message sent | Channel: {boss.ChannelId} | Message: {boss.MessageId}");
        }
    }
}