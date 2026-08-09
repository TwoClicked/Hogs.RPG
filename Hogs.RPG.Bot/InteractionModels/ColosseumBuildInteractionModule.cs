using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Hogs.RPG.Core.Entities.ColosseumObjects;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Core.Enums.PlayerEnums;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.ColosseumServices;

namespace Hogs.RPG.Bot.InteractionModels
{
    /// <summary>
    /// All button/select-menu handlers for the Colosseum DM build flow.
    /// Every handler re-fetches the participant/build from the DB by
    /// Context.User.Id rather than relying on any in-memory session state -
    /// the DB is the single source of truth, so this module stays fully
    /// stateless and safe across bot restarts. Actual rendering (embeds +
    /// components) lives in ColosseumBuildViews; this module is purely
    /// "load state -> mutate via ColosseumService -> re-render".
    /// </summary>
    public class ColosseumBuildInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ColosseumService _colosseumService;
        private readonly ColosseumRepository _colosseumRepository;

        public ColosseumBuildInteractionModule(ColosseumService colosseumService, ColosseumRepository colosseumRepository)
        {
            _colosseumService = colosseumService;
            _colosseumRepository = colosseumRepository;
        }

        // =========================
        // NAVIGATION
        // =========================
        [ComponentInteraction("colosseum_main_menu")]
        public async Task MainMenu()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildMainMenu(participant);
            await UpdateAsync(embed, components);
        }

        [ComponentInteraction("colosseum_gear_menu")]
        public async Task GearMenu()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildGearSlotPicker(participant.Build!);
            await UpdateAsync(embed, components);
        }

        [ComponentInteraction("colosseum_pet_menu")]
        public async Task PetMenu()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildPetPicker(participant.Build!);
            await UpdateAsync(embed, components);
        }

        [ComponentInteraction("colosseum_buffs_menu")]
        public async Task BuffsMenu()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildBuffsMenu(participant.Build!);
            await UpdateAsync(embed, components);
        }

        // =========================
        // GEAR
        // =========================
        [ComponentInteraction("colosseum_gear_slot_select")]
        public async Task SelectGearSlot(string[] values)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var slot = Enum.Parse<EquipmentSlot>(values[0]);
            var (embed, components) = ColosseumBuildViews.BuildGearItemPicker(participant.Build!, slot);
            await UpdateAsync(embed, components);
        }

        [ComponentInteraction("colosseum_gear_item_select:*")]
        public async Task SelectGearItem(string slotName, string[] values)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var slot = Enum.Parse<EquipmentSlot>(slotName);
            var chosen = values[0] == "baseline" ? null : values[0];

            var (success, message) = await _colosseumService.SetGearAsync(participant.Id, slot, chosen);

            // Re-fetch so the summary reflects the change (or shows the
            // error state unchanged if the purchase was rejected).
            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildGearSlotPicker(participant.Build!);
            await UpdateAsync(embed, components, success ? null : message);
        }

        // =========================
        // PET & PASSIVE
        // =========================
        [ComponentInteraction("colosseum_pet_select")]
        public async Task SelectPet(string[] values)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var chosen = values[0] == "baseline" ? null : values[0];
            var (success, message) = await _colosseumService.SetPetAsync(participant.Id, chosen);

            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildPetPicker(participant.Build!);
            await UpdateAsync(embed, components, success ? null : message);
        }

        [ComponentInteraction("colosseum_passive_select")]
        public async Task SelectPassive(string[] values)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            PetPassive? chosen = values[0] == "none" ? null : Enum.Parse<PetPassive>(values[0]);
            var (success, message) = await _colosseumService.SetPassiveAsync(participant.Id, chosen);

            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildPetPicker(participant.Build!);
            await UpdateAsync(embed, components, success ? null : message);
        }

        // =========================
        // BUFFS
        // =========================
        [ComponentInteraction("colosseum_buff_buy:*")]
        public async Task BuyBuff(string statName)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var stat = Enum.Parse<BuffStat>(statName);
            var (success, message) = await _colosseumService.BuyBuffAsync(participant.Id, stat);

            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildBuffsMenu(participant.Build!);
            await UpdateAsync(embed, components, success ? null : message);
        }

        [ComponentInteraction("colosseum_buff_remove:*")]
        public async Task RemoveBuff(string statName)
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var stat = Enum.Parse<BuffStat>(statName);
            var (success, message) = await _colosseumService.RemoveBuffAsync(participant.Id, stat);

            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildBuffsMenu(participant.Build!);
            await UpdateAsync(embed, components, success ? null : message);
        }

        // =========================
        // LOCK IN
        // =========================
        [ComponentInteraction("colosseum_lock_confirm_ask")]
        public async Task AskLockConfirm()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildLockConfirm(participant.Build!);
            await UpdateAsync(embed, components);
        }

        [ComponentInteraction("colosseum_lock_confirmed")]
        public async Task ConfirmLock()
        {
            var participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (success, message) = await _colosseumService.LockBuildAsync(participant.Id);

            participant = await LoadParticipantOrRespondAsync();
            if (participant == null) return;

            var (embed, components) = ColosseumBuildViews.BuildMainMenu(participant);
            await UpdateAsync(embed, components, success ? null : message);
        }

        // =========================
        // HELPERS
        // =========================

        // Loads the calling user's active Colosseum participant (with build).
        // If none exists - e.g. they clicked a stale button from a previous
        // tournament, or a bug elsewhere - edits the message into a dead
        // end rather than throwing, since there's nothing useful to do here.
        private async Task<ColosseumParticipant?> LoadParticipantOrRespondAsync()
        {
            var participant = await _colosseumRepository.GetActiveParticipantByDiscordIdAsync(Context.User.Id);

            if (participant?.Build == null)
            {
                if (Context.Interaction is SocketMessageComponent component)
                {
                    await component.UpdateAsync(msg =>
                    {
                        msg.Content = "❌ No active Colosseum build found - this may be from a past tournament.";
                        msg.Embed = null;
                        msg.Components = new ComponentBuilder().Build();
                    });
                }
                return null;
            }

            return participant;
        }

        // Edits the DM message in place with a new view. Optionally appends
        // a one-line error/status message above the embed (e.g. "not enough
        // AP") without needing a separate ephemeral response.
        private async Task UpdateAsync(Embed embed, MessageComponent components, string? statusLine = null)
        {
            if (Context.Interaction is not SocketMessageComponent component) return;

            await component.UpdateAsync(msg =>
            {
                msg.Content = statusLine != null ? $"⚠️ {statusLine}" : "";
                msg.Embed = embed;
                msg.Components = components;
            });
        }
    }
}