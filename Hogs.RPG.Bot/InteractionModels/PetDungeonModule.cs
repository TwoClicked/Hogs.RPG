using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.GameData.Registries;
using Hogs.RPG.Services.DungeonServices;
using Hogs.RPG.Services.PetServices;

namespace Hogs.RPG.Bot.InteractionModels
{
    public class PetDungeonModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PetDungeonService _petDungeonService;
        private readonly EvolvePetService _evolvePetService;

        public PetDungeonModule(PetDungeonService petDungeonService, EvolvePetService evolvePetService)
        {
            _petDungeonService = petDungeonService;
            _evolvePetService = evolvePetService;
        }

        // =========================
        // /petdungeon — ENTER
        // =========================
        [SlashCommand("petdungeon", "Enter a pet dungeon to hunt for a new pet")]
        public async Task EnterPetDungeon(
            [Autocomplete(typeof(PetDungeonAutocompleteHandler))] string dungeonId)
        {
            await DeferAsync(ephemeral: true);

            var result = await _petDungeonService.StartPetDungeonAsync(Context.User.Id, dungeonId);

            if (result == null || result.Embed == null)
            {
                await FollowupAsync("❌ Something went wrong.", ephemeral: true);
                return;
            }

            var desc = result.Embed.Description ?? "";

            if (desc.StartsWith("❌") || desc.StartsWith("⏳"))
            {
                await FollowupAsync(embed: result.Embed, ephemeral: true);
                return;
            }

            var components = new ComponentBuilder()
                .WithButton("⚔ Attack", "petdungeon_attack", ButtonStyle.Danger)
                .WithButton("🧪 Heal", "petdungeon_heal", ButtonStyle.Success)
                .WithButton("🏃 Flee", "petdungeon_flee", ButtonStyle.Secondary)
                .Build();

            try
            {
                var dm = await Context.User.CreateDMChannelAsync();
                var message = await dm.SendMessageAsync(embed: result.Embed, components: components);
                _petDungeonService.SetDungeonMessage(Context.User.Id, message.Id, dm.Id);
                await FollowupAsync("📬 Check your DMs to continue the pet dungeon.", ephemeral: true);
            }
            catch
            {
                await FollowupAsync("❌ I can't DM you. Please enable DMs.", ephemeral: true);
            }
        }

        // =========================
        // ATTACK BUTTON
        // =========================
        [ComponentInteraction("petdungeon_attack")]
        public async Task Attack()
        {
            if (Context.Interaction is not SocketMessageComponent component) return;
            try { await component.DeferAsync(); } catch { return; }

            var result = await _petDungeonService.AttackAsync(Context.User.Id);
            await _petDungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }

        // =========================
        // HEAL BUTTON
        // =========================
        [ComponentInteraction("petdungeon_heal")]
        public async Task Heal()
        {
            var component = (SocketMessageComponent)Context.Interaction;
            await component.DeferAsync();

            var result = await _petDungeonService.HealAsync(Context.User.Id);
            await _petDungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }

        // =========================
        // FLEE BUTTON
        // =========================
        [ComponentInteraction("petdungeon_flee")]
        public async Task Flee()
        {
            var component = (SocketMessageComponent)Context.Interaction;
            await component.DeferAsync();

            var result = _petDungeonService.Flee(Context.User.Id);
            await _petDungeonService.UpdateDungeonMessageAsync(Context.User.Id, result);
        }

        // =========================
        // /pet-evolve — CHECK STATUS
        // =========================
        [SlashCommand("pet-evolve", "Check evolution progress or evolve your Tier 2 pets into a Primal Chimera")]
        public async Task EvolveCheck()
        {
            await DeferAsync(ephemeral: true);

            var status = await _evolvePetService.GetEvolveStatusAsync(Context.User.Id);

            // Check if all 3 are owned — show confirm button if so
            bool allOwned = !status.Contains("❌");

            if (allOwned && !status.Contains("already own"))
            {
                var components = new ComponentBuilder()
                    .WithButton("✨ Evolve!", "petevolve_confirm", ButtonStyle.Success)
                    .WithButton("❌ Cancel", "petevolve_cancel", ButtonStyle.Secondary)
                    .Build();

                await FollowupAsync(status, components: components, ephemeral: true);
            }
            else
            {
                await FollowupAsync(status, ephemeral: true);
            }
        }

        // =========================
        // EVOLVE CONFIRM BUTTON
        // =========================
        [ComponentInteraction("petevolve_confirm")]
        public async Task EvolveConfirm()
        {
            if (Context.Interaction is not SocketMessageComponent component) return;

            await component.UpdateAsync(msg =>
            {
                msg.Content = "⏳ Evolving...";
                msg.Components = new ComponentBuilder().Build();
            });

            var (success, message) = await _evolvePetService.EvolveAsync(Context.User.Id);

            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = message;
            });
        }

        // =========================
        // EVOLVE CANCEL BUTTON
        // =========================
        [ComponentInteraction("petevolve_cancel")]
        public async Task EvolveCancel()
        {
            if (Context.Interaction is not SocketMessageComponent component) return;

            await component.UpdateAsync(msg =>
            {
                msg.Content = "❌ Evolution cancelled.";
                msg.Components = new ComponentBuilder().Build();
            });
        }
    }
}
