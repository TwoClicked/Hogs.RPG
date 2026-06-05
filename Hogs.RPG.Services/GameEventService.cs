using Discord;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.PlayerObjects;
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

        public async Task SendSmithingLevelUpAsync(Player player)
        {
            var channel = _client.GetChannel(_feedChannelId) as IMessageChannel;
            if (channel == null) return;

            string milestone = player.SmithingLevel switch
            {
                99 => "⚒️ **MAX LEVEL!** The forge bows to no one.",
                >= 85 => "🔵 Rune tier unlocked.",
                >= 70 => "🟢 Adamant tier unlocked.",
                >= 50 => "💠 Mithril tier unlocked.",
                >= 30 => "🩶 Steel tier unlocked.",
                >= 15 => "⬛ Iron tier unlocked.",
                _ => ""
            };

            var embed = new EmbedBuilder()
                .WithTitle("⚒️ Smithing Level Up!")
                .WithDescription(
                    $"<@{player.DiscordId}> reached **Smithing Level {player.SmithingLevel}**!" +
                    (string.IsNullOrEmpty(milestone) ? "" : $"\n{milestone}"))
                .WithColor(new Color(0xB87333))
                .Build();

            await channel.SendMessageAsync(embed: embed);
        }
    }
}