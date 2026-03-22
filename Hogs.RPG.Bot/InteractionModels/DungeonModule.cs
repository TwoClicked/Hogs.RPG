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

        // =========================
        // ENTER DUNGEON
        // =========================
        [SlashCommand("dungeon", "Enter a dungeon")]
        public async Task EnterDungeon(string dungeonId = "crypt_fanculo")
        {
            await DeferAsync(ephemeral: true);

            var result = await _dungeonService.StartDungeonAsync(Context.User.Id, dungeonId);

            // Handle errors
            if (result.Embed.Description.Contains("already") ||
                result.Embed.Description.Contains("must"))
            {
                await FollowupAsync(embed: result.Embed, ephemeral: true);
                return;
            }

            var components = new ComponentBuilder()
                .WithButton("⚔ Attack", "dungeon_attack", ButtonStyle.Danger)
                .WithButton("🧪 Heal", "dungeon_heal", ButtonStyle.Success)
                .WithButton("🏃 Flee", "dungeon_flee", ButtonStyle.Secondary)
                .Build();

            try
            {
                var dm = await Context.User.CreateDMChannelAsync();

                var message = await dm.SendMessageAsync(
                    embed: result.Embed,
                    components: components
                );

                // 🔥 Store DM message reference
                _dungeonService.SetDungeonMessage(Context.User.Id, message.Id, dm.Id);

                await FollowupAsync("📬 Check your DMs to continue the dungeon.", ephemeral: true);
            }
            catch
            {
                await FollowupAsync("❌ I can't DM you. Please enable DMs.", ephemeral: true);
            }
        }

        // =========================
        // ATTACK
        // =========================
        [ComponentInteraction("dungeon_attack")]
        public async Task Attack()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            await component.DeferAsync();

            var result = await _dungeonService.AttackAsync(Context.User.Id);

            await _dungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }

        // =========================
        // HEAL
        // =========================
        [ComponentInteraction("dungeon_heal")]
        public async Task Heal()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            await component.DeferAsync();

            var result = await _dungeonService.HealAsync(Context.User.Id);

            await _dungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }

        // =========================
        // FLEE
        // =========================
        [ComponentInteraction("dungeon_flee")]
        public async Task Flee()
        {
            var component = (SocketMessageComponent)Context.Interaction;

            await component.DeferAsync();

            var result = await _dungeonService.FleeAsync(Context.User.Id);

            await _dungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }
    }
}