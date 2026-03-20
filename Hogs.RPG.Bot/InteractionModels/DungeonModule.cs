using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Services.Game;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.InteractionModels
{
    public class DungeonModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DungeonService _dungeonService;

        public DungeonModule(DungeonService dungeonService)
        {
            _dungeonService = dungeonService;
        }

        [SlashCommand("dungeon", "Enter a dungeon")]
        public async Task EnterDungeon(string dungeonId = "crypt_fanculo")
        {
            await DeferAsync();

            var result = await _dungeonService.StartDungeonAsync(Context.User.Id, dungeonId);

            var components = new ComponentBuilder()
                .WithButton("⚔ Attack", "dungeon_attack", ButtonStyle.Danger)
                .WithButton("🧪 Heal", "dungeon_heal", ButtonStyle.Success)
                .WithButton("🏃 Flee", "dungeon_flee", ButtonStyle.Secondary);

            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = result.Embed;
                msg.Components = components.Build();
            });
        }

        [ComponentInteraction("dungeon_attack")]
        public async Task Attack()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            var result = await _dungeonService.AttackAsync(Context.User.Id);

            await component.UpdateAsync(msg =>
            {
                msg.Embed = result.Embed;

                if (result.IsFinished)
                    msg.Components = new ComponentBuilder().Build();
            });
        }

        [ComponentInteraction("dungeon_heal")]
        public async Task Heal()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            await component.DeferAsync();

            var result = await _dungeonService.HealAsync(Context.User.Id);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = result.Embed;

                if (result.IsFinished)
                    msg.Components = new ComponentBuilder().Build();
            });
        }

        [ComponentInteraction("dungeon_flee")]
        public async Task Flee()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            var result = await _dungeonService.FleeAsync(Context.User.Id);

            await component.UpdateAsync(msg =>
            {
                msg.Embed = result.Embed;
                msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}