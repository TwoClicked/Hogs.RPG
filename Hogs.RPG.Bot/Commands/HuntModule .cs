using Discord.Interactions;
using Hogs.RPG.Services.HuntServices;
using System.Threading.Tasks;

namespace Hogs.RPG.Bot.Commands
{
    public class HuntModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly HuntService _huntService;

        public HuntModule(HuntService huntService)
        {
            _huntService = huntService;
        }

        [SlashCommand("hunt", "Hunt animals for resources")]
        public async Task Hunt(
         [Autocomplete(typeof(HuntAutocompleteHandler))] string target = null,
         [Autocomplete(typeof(HuntAmountAutocompleteHandler))] string amount = "10")
        {
            await DeferAsync();

            int stamina;

            if (amount.ToLower() == "max")
            {
                stamina = -1; // special flag handled in service
            }
            else if (!int.TryParse(amount, out stamina))
            {
                await FollowupAsync("Invalid amount. Use a number or 'max'.");
                return;
            }

            var result = await _huntService.HuntAsync(Context.User.Id, target, stamina);

            await FollowupAsync(result);
        }
    }
}