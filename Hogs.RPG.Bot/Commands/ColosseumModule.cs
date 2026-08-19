using Discord;
using Discord.Interactions;
using Hogs.RPG.Data.Repositories;
using Hogs.RPG.Services.ColosseumServices;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Bot.InteractionModels;

namespace Hogs.RPG.Bot.Commands
{
    /// <summary>
    /// Entry-point slash commands for the Colosseum: signing up (which kicks
    /// off the DM build flow handled by ColosseumBuildInteractionModule) and
    /// checking current tournament status. The actual build UI lives in DMs,
    /// not here - this module only ever sends the very first DM message.
    /// </summary>
    public class ColosseumModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ColosseumService _colosseumService;
        private readonly ColosseumRepository _colosseumRepository;

        public ColosseumModule(ColosseumService colosseumService, ColosseumRepository colosseumRepository)
        {
            _colosseumService = colosseumService;
            _colosseumRepository = colosseumRepository;
        }

        [SlashCommand("colosseum", "Sign up for the Colosseum tournament")]
        public async Task Signup()
        {
            await DeferAsync(ephemeral: true);

            var (success, message, participant) = await _colosseumService.SignUpAsync(Context.User.Id);

            if (!success || participant == null)
            {
                await FollowupAsync($"❌ {message}", ephemeral: true);
                return;
            }

            try
            {
                var dm = await Context.User.CreateDMChannelAsync();
                var (embed, components) = ColosseumBuildViews.BuildMainMenu(participant);
                await dm.SendMessageAsync(embed: embed, components: components);

                await FollowupAsync("📬 Signed up! Check your DMs to build your loadout.", ephemeral: true);
            }
            catch
            {
                await FollowupAsync(
                    "✅ Signed up, but I couldn't DM you - please enable DMs from server members and run `/colosseum` again, " +
                    "or use `/colosseumbuild` once it's available in-server.",
                    ephemeral: true);
            }
        }

        [SlashCommand("colosseumstatus", "Check the current Colosseum tournament status")]
        public async Task Status()
        {
            await DeferAsync(ephemeral: true);

            var registration = await _colosseumRepository.GetActiveRegistrationAsync();
            if (registration != null)
            {
                var realCount = registration.Participants.Count(p => !p.IsBot);
                await FollowupAsync(
                    $"🏛️ **Registration open!** {realCount}/{registration.MaxRealPlayers} players signed up. " +
                    $"Closes <t:{new DateTimeOffset(registration.RegistrationEndsAt).ToUnixTimeSeconds()}:R>.\n" +
                    $"Buy-in: **{registration.BuyInGold} gold** — use `/colosseum` to sign up.",
                    ephemeral: true);
                return;
            }

            var inProgress = await _colosseumRepository.GetInProgressTournamentAsync();
            if (inProgress != null)
            {
                await FollowupAsync("🏛️ A tournament is currently **in progress** - check the announce channel for match threads.", ephemeral: true);
                return;
            }

            await FollowupAsync("🏛️ No Colosseum tournament is currently open. One opens daily.", ephemeral: true);
        }
    }
}