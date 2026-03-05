using Discord;
using Discord.WebSocket;
using Hogs.RPG.Bot.Setup;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;

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

            // register project services
            ServiceConfigurator.Configure(services);

            _services = services.BuildServiceProvider();

            _client.Log += LogAsync;

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

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}