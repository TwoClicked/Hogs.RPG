using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Bot.Setup;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private InteractionService _interactionService;

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

            // register bot instance
            services.AddSingleton(_client);

            // register InteractionService
            services.AddSingleton<InteractionService>(provider =>
                  new InteractionService(_client));

            // register project services (GoogleSheetsService etc.)
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
            _interactionService = _services.GetRequiredService<InteractionService>();

            Console.WriteLine("[Init] Loading interaction modules...");
            await _interactionService.AddModulesAsync(typeof(Program).Assembly, _services);

            //Console.WriteLine("[Init] Registering slash commands globally...");
            //await _interactionService.RegisterCommandsGloballyAsync();
            Console.WriteLine("[Init] Registering HogsServer commands Locally...");
            await _interactionService.RegisterCommandsToGuildAsync(
                guildId: 1109193500664287336, // HOGS SERVER DISCORD ID
                deleteMissing: true); 

            Console.WriteLine("[Init] Slash commands registered.");
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactionService.ExecuteCommandAsync(ctx, _services);
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