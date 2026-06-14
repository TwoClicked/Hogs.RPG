using Discord.Interactions;
using Hogs.RPG.Bot.Autocomplete;
using Hogs.RPG.Bot.Preconditions;
using Hogs.RPG.Core.Enums;
using Hogs.RPG.Services.HuntServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    [BossLock]
    [GearSwapLock]
    public class HuntModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly HuntService _huntService;

        public HuntModule(HuntService huntService)
        {
            _huntService = huntService;
        }

        [SlashCommand("hunt", "Hunt animals for resources")]
        public async Task Hunt(
            [Autocomplete(typeof(HuntCategoryAutocompleteHandler))] string category,
            [Autocomplete(typeof(HuntAutocompleteHandler))] string target = null,
            [Autocomplete(typeof(HuntAmountAutocompleteHandler))] string amount = "10")
        {
            await DeferAsync(ephemeral: true);

            // =========================
            // VALIDATION
            // =========================
            if (string.IsNullOrWhiteSpace(target))
            {
                await FollowupAsync("Select a target to hunt.", ephemeral: true);
                return;
            }

            int stamina;

            // =========================
            // PARSE INPUT
            // =========================
            if (!int.TryParse(amount, out stamina))
            {
                if (amount.ToLower() == "max")
                {
                    stamina = -1;
                }
                else
                {
                    await FollowupAsync("Invalid amount. Use a number or 'max'.", ephemeral: true);
                    return;
                }
            }

            // =========================
            // EXECUTE
            // =========================
            var result = await _huntService.HuntAsync(Context.User.Id, target, stamina);

            await FollowupAsync(result, ephemeral: true);
        }
    }
}