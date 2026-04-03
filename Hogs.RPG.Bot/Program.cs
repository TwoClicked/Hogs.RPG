using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Setup;
using Hogs.RPG.Services.Game; // 🔥 IMPORTANT (for BossScheduler)
using Hogs.RPG.Services.TradeServices;
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

        // 🔥 Keep reference so it doesn't start twice
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

            // ✅ Register Discord client properly
            services.AddSingleton(_client);
            services.AddSingleton<DiscordSocketClient>(_client);

            // ✅ InteractionService
            services.AddSingleton<InteractionService>(provider =>
                new InteractionService(
                    provider.GetRequiredService<DiscordSocketClient>(),
                    new InteractionServiceConfig
                    {
                        LogLevel = LogSeverity.Info,
                        DefaultRunMode = RunMode.Async
                    }));

            // ✅ Your services (includes BossScheduler registration)
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



            _interactionService = _services.GetRequiredService<InteractionService>();
            _interactionService.Log += LogAsync;

            Console.WriteLine("[Init] Loading interaction modules...");
            await _interactionService.AddModulesAsync(typeof(Program).Assembly, _services);

            Console.WriteLine("[Init] Registering HogsServer commands Locally...");
            await _interactionService.RegisterCommandsToGuildAsync(
                guildId: 1109193500664287336,
                deleteMissing: true);

            Console.WriteLine("[Init] Slash commands registered.");

            // =========================
            // 🔥 START SCHEDULER HERE
            // =========================
            if (!_schedulerStarted)
            {
                Console.WriteLine("🚀 Starting BossScheduler...");

                var scheduler = _services.GetRequiredService<BossScheduler>();
                _ = scheduler.StartAsync(CancellationToken.None);

                Console.WriteLine("🚀 Starting TradeCleanupService...");

                var tradeCleanup = _services.GetRequiredService<TradeCleanupService>();
                _ = tradeCleanup.StartAsync(CancellationToken.None);

                _schedulerStarted = true;
            }
        }

        private async Task HandleInteractionAsync(SocketInteraction arg)
        {
            Console.WriteLine("🔥 Interaction received");

            if (arg == null)
            {
                Console.WriteLine("❌ Interaction was null");
                return;
            }

            try
            {
                var ctx = new SocketInteractionContext(_client, arg);

                if (_interactionService == null)
                {
                    Console.WriteLine("❌ InteractionService not initialized yet");
                    return;
                }

                var result = await _interactionService.ExecuteCommandAsync(ctx, _services);

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

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}