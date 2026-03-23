using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities;
using System.Threading.Tasks;

namespace Hogs.RPG.Services.Game
{
    public class GameEventService
    {
        private readonly DiscordSocketClient _client;

        // ✅ RPG Feed channel
        private readonly ulong _feedChannelId = 1485357755433750549;

        public GameEventService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task SendLevelUpAsync(Player player)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            var embed = new EmbedBuilder()
                .WithTitle("🎉 Level Up!")
                .WithDescription($"<@{player.DiscordId}> reached **Level {player.Level}**!")
                .WithColor(Color.Green)
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }
}