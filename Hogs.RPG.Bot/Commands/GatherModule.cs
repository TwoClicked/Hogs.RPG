using Discord.Interactions;
using Hogs.RPG.Services.GatheringServices;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

public class GatherModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GatherService _gatherService;

    public GatherModule(GatherService gatherService)
    {
        _gatherService = gatherService;
    }

    [SlashCommand("gather", "Gather materials from an area")]
    public async Task Gather(
        [Autocomplete(typeof(GatherAreaAutocompleteHandler))]
        string area,
        int energy)
    {

        await DeferAsync(); // part of interactionmodulebase... (Due to the gahter call for loops it can take longer then 3s, Defer (15Min open call) (CHECK LATER: DOES DEFER KEEP THE LINE OPEN FOR 15 MINUTES????)

        var result = await _gatherService.GatherAsync(Context.User.Id, area, energy);

        await FollowupAsync(result);

    }
}