using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Setup;
using Hogs.RPG.Data;
using Hogs.RPG.Services.Game;
using Hogs.RPG.Services.RaidServices;
using Hogs.RPG.Services.TradeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private InteractionService _interactionService;

        private bool _schedulerStarted = false;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds
            };

            _client = new DiscordSocketClient(config);

            var services = new ServiceCollection();

            services.AddSingleton(_client);
            services.AddSingleton<DiscordSocketClient>(_client);

            services.AddSingleton<InteractionService>(provider =>
                new InteractionService(
                    provider.GetRequiredService<DiscordSocketClient>(),
                    new InteractionServiceConfig
                    {
                        LogLevel = LogSeverity.Info,
                        DefaultRunMode = RunMode.Async
                    }));

            ServiceConfigurator.Configure(services);

            _services = services.BuildServiceProvider();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteractionAsync;

            var token = Environment.GetEnvironmentVariable("HOGS_RPG_TOKEN");

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("HOGS_RPG_TOKEN environment variable missing.");
                return;
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            Console.WriteLine("Hogs RPG Bot started.");

            await Task.Delay(-1);
        }

        private async Task ReadyAsync()
        {
            Console.WriteLine("🔥 Discord client READY");

            // ✅ Auto-migrate database on startup
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
                    await db.Database.MigrateAsync();
                    Console.WriteLine("✅ Database migrated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database migration failed: {ex.Message}");
            }

            _interactionService = _services.GetRequiredService<InteractionService>();
            _interactionService.Log += LogAsync;
            _interactionService.SlashCommandExecuted += HandleSlashCommandExecuted;

            Console.WriteLine("[Init] Loading interaction modules...");
            await _interactionService.AddModulesAsync(typeof(Program).Assembly, _services);

            Console.WriteLine("[Init] Registering HogsServer commands Locally...");
            await _interactionService.RegisterCommandsToGuildAsync(
                guildId: 1109193500664287336,
                deleteMissing: true);

            Console.WriteLine("[Init] Slash commands registered.");

            Console.WriteLine("🚀 Starting LeaderboardUpdater...");

            var leaderboard = _services.GetRequiredService<LeaderboardUpdater>();

            _ = Task.Run(() => leaderboard.RunAsync());

            if (!_schedulerStarted)
            {
                Console.WriteLine("🚀 Starting BossScheduler...");

                var scheduler = _services.GetRequiredService<BossScheduler>();
                _ = scheduler.StartAsync(CancellationToken.None);

                Console.WriteLine("🚀 Starting TradeCleanupService...");

                var tradeCleanup = _services.GetRequiredService<TradeCleanupService>();
                _ = tradeCleanup.StartAsync(CancellationToken.None);

                Console.WriteLine("🚀 Starting RaidTimerService...");

                var raidTimer = _services.GetRequiredService<RaidTimerService>();
                _ = raidTimer.StartAsync(CancellationToken.None);

                _schedulerStarted = true;
            }
        }

        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            var receivedAt = DateTime.UtcNow;
            Console.WriteLine($"\n⚡ Interaction received at {receivedAt:HH:mm:ss.fff}");
            Console.WriteLine($"   Type: {arg.Type}");
            Console.WriteLine($"   Created at (Discord timestamp): {arg.CreatedAt:HH:mm:ss.fff}");

            var discordAge = receivedAt - arg.CreatedAt.UtcDateTime;
            Console.WriteLine($"   ⏱ Interaction age when received: {discordAge.TotalMilliseconds:F1}ms");

            if (discordAge.TotalMilliseconds > 2000)
                Console.WriteLine($"   ☠️ ALREADY DEAD ON ARRIVAL - interaction is {discordAge.TotalMilliseconds:F1}ms old before we even start");
            else if (discordAge.TotalMilliseconds > 1000)
                Console.WriteLine($"   ⚠️ WARNING - interaction is already {discordAge.TotalMilliseconds:F1}ms old");

            if (arg == null)
            {
                Console.WriteLine("❌ Interaction was null");
                return;
            }

            try
            {
                var ctxStart = DateTime.UtcNow;
                var ctx = new SocketInteractionContext(_client, arg);
                Console.WriteLine($"   Context built in {(DateTime.UtcNow - ctxStart).TotalMilliseconds:F1}ms");

                if (_interactionService == null)
                {
                    Console.WriteLine("❌ InteractionService not initialized yet");
                    return;
                }

                var beforeExecute = DateTime.UtcNow;
                Console.WriteLine($"➡️ Executing command at {beforeExecute:HH:mm:ss.fff} (+{(beforeExecute - receivedAt).TotalMilliseconds:F1}ms since received)");

                var result = await _interactionService.ExecuteCommandAsync(ctx, _services);

                var afterExecute = DateTime.UtcNow;
                Console.WriteLine($"✅ Command finished at {afterExecute:HH:mm:ss.fff} (+{(afterExecute - receivedAt).TotalMilliseconds:F1}ms since received, {(afterExecute - beforeExecute).TotalMilliseconds:F1}ms execution)");

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"❌ Command failed: {result.ErrorReason}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Interaction Error] {ex}");
            }
        }

        private async Task HandleSlashCommandExecuted(SlashCommandInfo info, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess && result.Error == InteractionCommandError.UnmetPrecondition)
            {
                await context.Interaction.RespondAsync(result.ErrorReason, ephemeral: true);
            }
        }

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}