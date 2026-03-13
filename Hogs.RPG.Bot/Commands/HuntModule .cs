using Discord.Interactions;
using Hogs.RPG.Core.Entities;
using Hogs.RPG.Services.GameplayServices;
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
            [Autocomplete(typeof(HuntAutocompleteHandler))]
            string target = null)
        {
            await DeferAsync();

            var result = await _huntService.HuntAsync(Context.User.Id, target);

            await FollowupAsync(result);
        }
    }
}