using Discord;
using Discord.Interactions;
using Hogs.RPG.Services.PlayerServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class PlayerCommands : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly PlayerService _playerService;
        public PlayerCommands(PlayerService playerService)
        {
            _playerService = playerService;
        }

 
        [SlashCommand("startadventure", "Start your Viking adventure")]
        public async Task Start()
        {

            var player = await _playerService.GetOrCreatePlayerAsync(
                Context.User.Id,
                Context.User.Username
               );

            var embed = new EmbedBuilder()
                .WithTitle($"{player.Username}'s Viking Journey Begins")
                .WithColor(Color.DarkRed)
                .AddField("Level", player.Level, true)
                .AddField("XP", player.XP, true)
                .AddField("Gold", player.Gold, true)
                .AddField("Attack", player.Attack, true)
                .AddField("Defense", player.Defense, true)
                .AddField("Health", player.Health, true)
                .WithFooter("Welcome to the Hogs RPG");

            await RespondAsync(embed: embed.Build());
        }
    }
}
